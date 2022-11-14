using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models
{
    [DynamoDBTable("APICenter_CAPI_Dictionary")]
    public class ApplicationAccessInfo

    {
        [DynamoDBHashKey]
        public string ID { get; set; }
        [DynamoDBProperty]
        public long CreateDate { get; set; }
        [DynamoDBProperty]
        public string AppSecret { get; set; }
        [DynamoDBProperty]
        public string CorpId { get; set; }
        [DynamoDBProperty]
        public string CorpSecret { get; set; }
        [DynamoDBProperty]
        public string AgentId { get; set; }
    }
}
