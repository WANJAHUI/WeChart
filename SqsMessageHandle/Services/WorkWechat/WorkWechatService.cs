using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Senparc.CO2NET.Helpers.Serializers;
using Senparc.Weixin;
using Senparc.Weixin.CommonAPIs;
using Senparc.Weixin.Work.AdvancedAPIs;
using Senparc.Weixin.Work.AdvancedAPIs.Mass;
using Senparc.Weixin.Work.Containers;
using Senparc.Weixin.Work.Entities;
using SqsMessageHandle.EnumAndConsts;
using SqsMessageHandle.Models;
using SqsMessageHandle.Models.WorkWechat;

namespace SqsMessageHandle.Services.WorkWechat
{
    public class WorkWechatService
    {
        private readonly AccessTokenProvider _accessTokenProvider;
        public event Events.LogHandler OnLogger;
        private static string _urlFormat = Config.ApiWorkHost + "/cgi-bin/message/send?access_token={0}";
        public WorkWechatService(AccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }
        public async Task<string> GetQYTokenResult(ApplicationAccessInfo info)
        {
            OnLogger?.Invoke("获取token开始");
            var accesstoken = await _accessTokenProvider.GetAccessToken(info.CorpId,info.CorpSecret,info.AgentId);
            OnLogger?.Invoke($"获取token结束{accesstoken}");
            return accesstoken;

        }
        public async Task<(bool,string)> sendTextMessage(WorkWechatTextMessageModel Message, ApplicationAccessInfo info)
        {
            var accesstokenresult = await this.GetQYTokenResult(info);
            //var data = new
            //{
            //    touser = Message.touser,
            //    toparty = Message.toparty,
            //    totag = Message.totag,
            //    msgtype = "text",
            //    agentid = Consts.AgentId,
            //    text = new
            //    {
            //        Message.text
            //    },
            //    safe = Message.safe,
            //    enable_duplicate_check = 0,
            //    duplicate_check_interval = 1800
            //};
            //JsonSetting jsonSetting = new JsonSetting(ignoreNulls: true);
            OnLogger?.Invoke($"发哦哪个方法结束开始");
            //var result = CommonJsonSend.Send<MassResult>(accesstokenresult, _urlFormat, data, CommonJsonSendType.POST, 10000, checkValidationResult: false, jsonSetting);
            var result = await MassApi.SendTextAsync(accesstokenresult, info.AgentId.ToString(), Message.text, Message.touser, Message.toparty, Message.totag, Message.safe);
            if (result.errcode != 0)
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送文本消息失败!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (false,result.errmsg);
            }
            else
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送文本消息成功!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (true, "");
            }
        }
        public async Task<(bool, string)> sendMiniProgramNotice(SendMiniProgramNoticeData Message, ApplicationAccessInfo info)
        {
            var accesstokenresult = await this.GetQYTokenResult(info);
      
            //var data = new
            //{
            //    touser = Message.touser,
            //    toparty = Message.toparty,
            //    totag = Message.totag,
            //    msgtype = "text",
            //    agentid = Consts.AgentId,
            //    text = new
            //    {
            //        Message.text
            //    },
            //    safe = Message.safe,
            //    enable_duplicate_check = 0,
            //    duplicate_check_interval = 1800
            //};
            //JsonSetting jsonSetting = new JsonSetting(ignoreNulls: true);
            //var result = CommonJsonSend.Send<MassResult>(accesstokenresult, _urlFormat, data, CommonJsonSendType.POST, 10000, checkValidationResult: false, jsonSetting);
            var result = await MassApi.SendMiniNoticeCardAsync(accesstokenresult, Message);
            if (result.errcode != 0)
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送小程序推送消息失败!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (false, result.errmsg);
            }
            else
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送小程序推送消息成功!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (true, "");
            }
        }
        public async Task<(bool, string)> sendTaskCard(SendTaskCardNoticeNewData Message,ApplicationAccessInfo info)
        {
            var accesstokenresult = await this.GetQYTokenResult(info);

            //var data = new
            //{
            //    touser = Message.touser,
            //    toparty = Message.toparty,
            //    totag = Message.totag,
            //    msgtype = "text",
            //    agentid = Consts.AgentId,
            //    text = new
            //    {
            //        Message.text
            //    },
            //    safe = Message.safe,
            //    enable_duplicate_check = 0,
            //    duplicate_check_interval = 1800
            //};
            //JsonSetting jsonSetting = new JsonSetting(ignoreNulls: true);
            //var result = CommonJsonSend.Send<MassResult>(accesstokenresult, _urlFormat, data, CommonJsonSendType.POST, 10000, checkValidationResult: false, jsonSetting);
             //var result = await MassApi.SendTaskCard(accesstokenresult, Message);
            string _urlFormat = Config.ApiWorkHost + "/cgi-bin/message/send?access_token={0}";
            Message.agentid =int.Parse(info.AgentId);
            Message.msgtype = "interactive_taskcard";

             var result =await  CommonJsonSend.SendAsync<MassResult>(accesstokenresult, _urlFormat, Message, CommonJsonSendType.POST, 10000, checkValidationResult: false);
            if (result.errcode != 0)
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送任务卡片消息失败!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (false, result.errmsg);
            }
            else
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送任务卡片消息成功!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (true, "");
            }
        }
        public async Task<(bool, string)> sendMarkdownAsync(WorkWechatMarkdownModel Message, ApplicationAccessInfo info)
        {
            var accesstokenresult = await this.GetQYTokenResult(info);

           
            var result = await MassApi.SendMarkdownAsync(accesstokenresult, info.AgentId.ToString(), Message.text,Message.touser,Message.toparty,Message.toparty);
            if (result.errcode != 0)
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送markdown消息失败!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (false, result.errmsg);
            }
            else
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送markdown消息成功!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (true, "");
            }
        }
        public async Task<(bool, string)> sendNewsAsync(WorkWechatNewsMessageModel Message, ApplicationAccessInfo info)
        {
            var accesstokenresult = await this.GetQYTokenResult(info);


            var result = await MassApi.SendNewsAsync(accesstokenresult, info.AgentId.ToString(), Message.articles, Message.touser, Message.toparty, Message.toparty);
            if (result.errcode != 0)
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送图文消息失败!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (false, result.errmsg);
            }
            else
            {
                var message = $"消息主体{Newtonsoft.Json.JsonConvert.SerializeObject(Message)},发送图文消息成功!结果:{Newtonsoft.Json.JsonConvert.SerializeObject(result)}";
                OnLogger?.Invoke(message);
                return (true, "");
            }
        }

    }
}
