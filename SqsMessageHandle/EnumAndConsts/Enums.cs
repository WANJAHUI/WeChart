using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.EnumAndConsts
{
    public enum Msgtype
    {
        mobile = 1,
        email = 2,
        workwechattext = 3,
        workwechat_miniprogram_notice = 4,
        workwechat_taskcard = 5,
        workwechat_markdown = 6,
        workwechat_news = 7

    }
    public enum MsgStatus
    {
        发送失败 = 1,
        推送中 = 2,
        推送成功 = 3,
        推送失败 = 4,
        重试中 = 5
    }
    public enum ContentType
    {
        Text = 0,
        Html = 1

    }
    public enum MailSendType
    {
        Exchange = 0,
        SMTP = 1
    }
    public enum Mobilemsgtype
    {
        ali = 0,
        tc = 1
    }
    public static class Consts
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        public static string AgentId => Environment.GetEnvironmentVariable("agentid");
        /// <summary>
        /// 企业号ID
        /// </summary>
        public static string CorpId => Environment.GetEnvironmentVariable("corpid");
        /// <summary>
        /// 应用的Secret
        /// </summary>
        public static string CorpSecret => Environment.GetEnvironmentVariable("appsecret");
        /// <summary>
        /// Redis连接
        /// </summary>
        public static string Redis => Environment.GetEnvironmentVariable("redis");
    }

    
}
