using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Services.DynamoDb
{
    public class DynamoDBOptions
    {
        public string AccessKeyId { get; set; }

        public string SecretKey { get; set; }

        public string BaseKey { get; set; }

        public bool IsProduction { get; set; }

    }
}
