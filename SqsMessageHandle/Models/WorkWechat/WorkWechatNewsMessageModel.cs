using Senparc.NeuChar.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models.WorkWechat
{
   public class WorkWechatNewsMessageModel
    {
        public string text { get; set; }
        public string toparty { get; set; }
        public string touser { get; set; }
        public string totag { get; set; }
        public int safe { get; set; }
        public List<Article> articles { get; set; }
    }
  
}
