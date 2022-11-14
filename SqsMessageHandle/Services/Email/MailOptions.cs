using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Services.Email
{
  public  class MailOptions
    {
        /// <summary>
        /// 登陆用户名/ Exchange域
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// smtp邮件服务器 
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// smtp邮件服务器端口 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否使用默认凭证
        /// </summary>
        public bool UseDefaultCredentials { get; set; } = true;

        /// <summary>
        /// 是否使用安全套接字层 (SSL) 加密
        /// </summary>
        public bool EnableSsl { get; set; } = false;

        /// <summary>
        /// 发件人
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 登录账号
        /// </summary>
        public string UserName { get; set; }
    
        /// <summary>
        /// 登陆密码
        /// </summary>
        public string Password { get; set; }
    }
}
