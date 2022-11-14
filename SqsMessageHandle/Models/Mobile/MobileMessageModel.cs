using SqsMessageHandle.EnumAndConsts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models
{
  public  class MobileMessageModel
    {
        public string userIphone { get; set; }
        public string content { get; set; }
        public string templateid { get; set; }
        public Mobilemsgtype mobilemsgtype { get; set; }
    }
  


}
