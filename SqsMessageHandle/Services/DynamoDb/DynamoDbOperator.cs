using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SqsMessageHandle.Services.DynamoDb
{
    public class DynamoDbOperator 
    {


        AmazonDynamoDBClient client = null;
        DynamoDBContext dBContext = null;
        DynamoDBOptions _options;
   
        public DynamoDbOperator(IOptions<DynamoDBOptions> options)
        {
            RegionEndpoint _regionEndpoint = RegionEndpoint.CNNorthWest1;
   
            _options = options.Value;
            //var isprod = Environment.GetEnvironmentVariable("DynamoIsProduction");


            if (_options.IsProduction )
            {
                _regionEndpoint = RegionEndpoint.CNNorth1;
            }
            else
            {
                _regionEndpoint = RegionEndpoint.CNNorthWest1;
            }
          //  _LogService.LogInformation($"_options.AccessKeyId:{_options.AccessKeyId} _options.SecretKey:{_options.SecretKey} _regionEndpoint:{_regionEndpoint.DisplayName}");
            client = new AmazonDynamoDBClient(_options.AccessKeyId, _options.SecretKey, _regionEndpoint);
            dBContext = new DynamoDBContext(client, new DynamoDBContextConfig
            {
                Conversion = DynamoDBEntryConversion.V2,
                ConsistentRead = true,
                TableNamePrefix = _options.BaseKey
            });
        }



        /// <summary>
        /// 添加或删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">model</param>
        /// <returns></returns>
        public async Task AddOrUpdateAsync<T>(T model)
        {
            await dBContext.SaveAsync(model);
        }

        /// <summary>
        /// 批量添加对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">批量对象</param>
        /// <returns></returns>
        public async Task AddBatchAsync<T>(IEnumerable<T> models)
        {
            var bookBatch = dBContext.CreateBatchWrite<T>();
            bookBatch.AddPutItems(models);
            await bookBatch.ExecuteAsync();
        }



        /// <summary>
        /// 根据分区id获取一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkid">分区键id</param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, AttributeValue>>> GetAsyn(string tableName)
        {
            var request = new ScanRequest
            {
                TableName = tableName,
            };
            var result = await client.ScanAsync(request);
            return result.Items;
        }

        public async Task<List<Dictionary<string, AttributeValue>>> GetWithFilterAsyn(string tableName, int limit)
        {
            var request = new ScanRequest
            {
                TableName = tableName,
                Limit = limit
            };
            var result = await client.ScanAsync(request);
            return result.Items;
        }

        public async Task<T> GetAsynByPkid<T>(object pkid)
        {

            return await dBContext.LoadAsync<T>(pkid, rangeKey: null);
        }

        /// <summary>
        /// FromQuery 查询具有复合主键（分区键和排序键）的表。如果表的主键只有分区键，则不支持 Query 操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter">查询条件</param>
        /// <param name="limit">数量</param>
        /// <returns></returns>
        public async Task<List<T>> FromQueryAsyncByPkAndRengKey<T>(QueryFilter filter, int limit)
        {
            //filter.AddCondition("IsDelete", QueryOperator.Equal, false);
            QueryOperationConfig config = new QueryOperationConfig()
            {
                Limit = limit, // 2 items/page.
                Select = SelectValues.AllAttributes,
                ConsistentRead = true,
                Filter = filter
            };

            var result = await dBContext.FromQueryAsync<T>(config).GetNextSetAsync();
            return result;
        }

        /// <summary>
        /// Query 查询具有复合主键（分区键和排序键）的表。如果表的主键只有分区键，则不支持 Query 操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pk"></param>
        /// <param name="scanConditions"></param>
        /// <returns></returns>
        public async Task<List<T>> QueryAsyncByPkAndRengKey<T>(object pk, List<ScanCondition> scanConditions)
        {

            //scanConditions.Add(new ScanCondition("IsDelete", ScanOperator.Equal, false));

            DynamoDBOperationConfig dynamoDBOperationConfig = new DynamoDBOperationConfig()
            {
                QueryFilter = scanConditions
            };

            var search1 = await dBContext.QueryAsync<T>(pk, dynamoDBOperationConfig).GetNextSetAsync();
            return search1;
        }

        /// <summary>
        /// 扫描查询 支持所有list查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scanConditions"></param>
        /// <returns></returns>
        public async Task<List<T>> ScanAsync<T>(List<ScanCondition> scanConditions)
        {
            //scanConditions.Add(new ScanCondition("IsDelete", ScanOperator.Equal, false));
            var itemsWithWrongPrice = await dBContext.ScanAsync<T>(scanConditions).GetNextSetAsync();
            return itemsWithWrongPrice;
        }

        /// <summary>
        /// 物理删除莫Hashid的所有行数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Id">HashKey</param>
        /// <returns></returns>
        public async Task<bool> DelAsync<T>(object Id)
        {
            await dBContext.DeleteAsync<T>(Id);
            var deletedobj = dBContext.LoadAsync<T>(Id, new DynamoDBContextConfig
            {
                ConsistentRead = true
            });
            if (deletedobj == null)
                return true;

            return false;
        }


        /// <summary>
        /// 物理删除莫Hashid的所有行数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Id">HashKey</param>
        /// <returns></returns>
        public async Task<bool> DelAsync<T>(object Id, object rangeKey)
        {
            await dBContext.DeleteAsync<T>(Id, rangeKey);
            return true;
        }
    }
}
