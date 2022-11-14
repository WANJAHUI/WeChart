using Amazon.DynamoDBv2.DocumentModel;
using SqsMessageHandle.Models;
using SqsMessageHandle.Services.DynamoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqsMessageHandle.Services
{
  public   class ApplicationAccessService
    {
        private readonly DynamoDbOperator _dynamoDbOperator;
        public ApplicationAccessService(DynamoDbOperator dynamoDbOperator)
        {
            _dynamoDbOperator = dynamoDbOperator;
        }

     
        public async Task<ApplicationAccessInfo> GetAccessInfoAsync(string AppId)
        {
            //List<ScanCondition> scs = new List<ScanCondition>();
            //var sc = new ScanCondition("MessageId", ScanOperator.Equal, messageId);
            //scs.Add(sc);
            var query = new QueryFilter("ID", QueryOperator.Equal, new DynamoDBEntry[] { AppId });
            //var cfg = new DynamoDBOperationConfig
            //{
            //    QueryFilter = scs
            //};

            var result = await _dynamoDbOperator.FromQueryAsyncByPkAndRengKey<ApplicationAccessInfo>(query, 1);
            Console.WriteLine($"AppId{AppId};{Newtonsoft.Json.JsonConvert.SerializeObject(result)}");
            if (result.Count() == 0)
                return null;
            return result.FirstOrDefault();
        }
    }
}
