using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models.WorkWechat
{
  public  class AccessToken
    {
        public int errcode { get; set; }
        public string errmsg { get; set; }
        public int expires_in { get; set; }
        public string access_token { get; set; }
    }
}
