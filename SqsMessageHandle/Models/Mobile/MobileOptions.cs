using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models
{

    public class MobileOptions
    {
        //阿里acs
        public string aliregion { get; set; }
        public string aliaccessKeyId { get; set; }
        public string aliaccessSecret { get; set; }
        //腾讯云
        public string tcregion { get; set; }
        public string tcaccessKeyId { get; set; }
        public string tcaccessSecret { get; set; }

    }
}
