using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using SqsMessageHandle.EnumAndConsts;
using SqsMessageHandle.Models;
using SqsMessageHandle.Services.DynamoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqsMessageHandle.Services
{
   public  class CommonMessageService
    {
        private readonly DynamoDbOperator _dynamoDbOperator;
        public CommonMessageService(DynamoDbOperator dynamoDbOperator )
        {
            _dynamoDbOperator = dynamoDbOperator;
        }

        public async Task  DealLogs(string messageId, bool isSuccess = true,string errMessage="")
        {
            if (string.IsNullOrEmpty(messageId))
                return;
            var log = await this.GetMessageLogsAsync(messageId);
            Console.WriteLine($"获取log日志消息{Newtonsoft.Json.JsonConvert.SerializeObject(log)}");
            if (log != null)
            {
                if (isSuccess)
                {
                    log.Status = MsgStatus.推送成功.ToString();

                }
                else
                    log.Status = MsgStatus.推送失败.ToString();
                log.Times = log.Times + 1;
                log.SendDate = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss") ;
                log.ErrMessage =string.IsNullOrEmpty(log.ErrMessage)? errMessage:(log.ErrMessage+"||"+errMessage);
                await _dynamoDbOperator.AddOrUpdateAsync(log);
                Console.WriteLine($"修改log日志消息{Newtonsoft.Json.JsonConvert.SerializeObject(log)}");
            }

        }
        //目前messageId 格式变化
        private async Task<MessageLogs> GetMessageLogsAsync(string dataId)
        {
            var datas = dataId.Split('@', StringSplitOptions.RemoveEmptyEntries);
            var messageId = datas[0];
            var query = new QueryFilter("MessageId", QueryOperator.Equal, new DynamoDBEntry[] { messageId });
            if (datas.Length > 1)
            {
                query.AddCondition("CreateDate", QueryOperator.Equal, Convert.ToInt64(datas[1]));
            }
            //List<ScanCondition> scs = new List<ScanCondition>();
            //var sc = new ScanCondition("MessageId", ScanOperator.Equal, messageId);
            //scs.Add(sc);
          //  var query = new QueryFilter("MessageId",QueryOperator.Equal, new DynamoDBEntry[] { messageId });
            //var cfg = new DynamoDBOperationConfig
            //{
            //    QueryFilter = scs
            //};

            var result = await _dynamoDbOperator.FromQueryAsyncByPkAndRengKey<MessageLogs>(query,1);
            Console.WriteLine($"messageid{messageId};{Newtonsoft.Json.JsonConvert.SerializeObject(result)}");
            if (result.Count() == 0)
                return null;
            return result.First();
        }
        

    }
}
