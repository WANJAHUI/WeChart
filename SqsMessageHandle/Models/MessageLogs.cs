using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models
{
    [DynamoDBTable("APICenter_Message_Logs")]
    public class MessageLogs

    {
        [DynamoDBHashKey]
        public string MessageId { get; set; }
        [DynamoDBProperty]
        public long CreateDate { get; set; }
        [DynamoDBProperty]
        public string MsgType { get; set; }
        [DynamoDBProperty]
        public string AppId { get; set; }
        [DynamoDBProperty]
        public string Content { get; set; }
        [DynamoDBProperty]
        public string Status { get; set; }
        [DynamoDBProperty]
        public string ErrMessage { get; set; }
        [DynamoDBProperty]
        public string MD5Content { get; set; }

        [DynamoDBProperty]
        public string SendDate { get; set; }
        [DynamoDBProperty]
        public string EmailGuid { get; set; }
        [DynamoDBProperty]
        public string EmailType { get; set; }
        [DynamoDBProperty]
        public int Times { get; set; }

    }
}
