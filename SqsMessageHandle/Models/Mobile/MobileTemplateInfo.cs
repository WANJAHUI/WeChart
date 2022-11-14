using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models.Mobile
{
    [DynamoDBTable("APICenter_CAPI_Dictionary")]
    public class MobileTemplateInfo
    {


        [DynamoDBHashKey]
        public string ID { get; set; }
        [DynamoDBProperty]
        public long CreateDate { get; set; }
        [DynamoDBProperty]
        public string AliSign { get; set; }
        [DynamoDBProperty]
        public string AliTemplateCode { get; set; }
        [DynamoDBProperty]
        public string TciSign { get; set; }
        [DynamoDBProperty]
        public string TcTemplateCode { get; set; }
    }
}
