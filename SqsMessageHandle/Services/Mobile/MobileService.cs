using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Profile;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using SqsMessageHandle.Models;
using SqsMessageHandle.Models.Mobile;
using SqsMessageHandle.Services.DynamoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aliyun.Acs.Core.Http;
using Aliyun.Acs.Core.Exceptions;
using Newtonsoft.Json.Linq;
using qcloudsms_csharp;

namespace SqsMessageHandle.Services.Mobile
{
    public class MobileService
    {
        private MobileOptions _mobileOptions;
        private readonly DynamoDbOperator _dynamoDbOperator;
        public MobileService(IOptions<MobileOptions> options, DynamoDbOperator dynamoDbOperator)
        {
            _mobileOptions = options.Value;
            _dynamoDbOperator = dynamoDbOperator;
        }
        public async Task<(bool, string)> SendMobileMessage(MobileMessageModel model)
        {
            var template = await this.GetTemplateInfoAsync(model.templateid);
            var success = false;
            var reason = "";
            if (!string.IsNullOrEmpty(template.AliTemplateCode))
            {
               var alresult = await this.SendAliMobile(model,template);
                if (alresult.Item1)
                {
                    success = true;
                    reason += "阿里接口发送成功";
                }
                else
                {
                    success = false;
                    reason += $"阿里接口发送失败,原因:{alresult.Item2}@";
                }
            }
            if (!success && !string.IsNullOrEmpty(template.TcTemplateCode))
            {
              var tcresult= await this.SendTcMobile(model, template);
                if (!tcresult.Item1)
                {
                    success = false;
                    reason += $"腾讯接口有发送失败,原因:{tcresult.Item2}";
                }
                else
                {
                    success = true;
                    reason += $"腾讯接口有发送成功,原因:{tcresult.Item2}";
                }    
            }
            return (success,reason);
            //if (model.mobilemsgtype == EnumAndConsts.Mobilemsgtype.ali)
            //    return await this.SendAliMobile(model, template);
            //else
            //    return await this.SendTcMobile(model,template);

        }
        private async Task<(bool, string)> SendAliMobile(MobileMessageModel model, MobileTemplateInfo info)
        {
            IClientProfile profile = DefaultProfile.GetProfile(_mobileOptions.aliregion, _mobileOptions.aliaccessKeyId,_mobileOptions.aliaccessSecret);
            DefaultAcsClient client = new DefaultAcsClient(profile);
            var  request = new CommonRequest();
            request.Method = MethodType.POST;
            request.Domain = "dysmsapi.aliyuncs.com";
            request.Version = "2017-05-25";
            request.Action = "SendSms";
            // request.Protocol = ProtocolType.HTTP;
            request.AddQueryParameters("PhoneNumbers", model.userIphone);
            request.AddQueryParameters("SignName", info.AliSign);
            request.AddQueryParameters("TemplateCode", info.AliTemplateCode);
            request.AddQueryParameters("TemplateParam", model.content);
            var  response = client.GetCommonResponse(request);
            var json = System.Text.Encoding.UTF8.GetString(response.HttpResponse.Content);
            JToken val = null;
            if (JObject.Parse(json)?.TryGetValue("Code", out val) ?? false)
            {
                var code = val?.Value<string>();
                if ("OK".Equals(code, StringComparison.OrdinalIgnoreCase))
                {
                    return (true, "");
                }
                else
                {
                    return (false, json);
                }
            }
            else
                return (false,json);

        }
        private async Task<(bool, string)> SendTcMobile(MobileMessageModel model, MobileTemplateInfo info)
        {
            SmsSingleSender ssender = new SmsSingleSender(Convert.ToInt32(_mobileOptions.tcaccessKeyId), _mobileOptions.tcaccessSecret);
            var result = ssender.sendWithParam("86", model.userIphone, Convert.ToInt32(info.TcTemplateCode), Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,string>>(model.content).Values.ToArray(),info.TciSign, "", "");
           // var  smsResult = JsonConvert.DeserializeObject<QcloudSmsResult>(result.ToString());
        //    Logger.Info($"腾讯云短信发送end phone:{phone } templateId:{templateId } parameters:{JsonConvert.SerializeObject(parameters)} tplData:{tplData } result:{result.ToString()}");
            if (result.result == 0)
            {
                return (true,"") ;
            }
            else
                return (false,result.errMsg);
          

        }
       
        public async Task<MobileTemplateInfo> GetTemplateInfoAsync(string TemplateId)
        {
            //List<ScanCondition> scs = new List<ScanCondition>();
            //var sc = new ScanCondition("MessageId", ScanOperator.Equal, messageId);
            //scs.Add(sc);
            var query = new QueryFilter("ID", QueryOperator.Equal, new DynamoDBEntry[] { TemplateId });
            //var cfg = new DynamoDBOperationConfig
            //{
            //    QueryFilter = scs
            //};

            var result = await _dynamoDbOperator.FromQueryAsyncByPkAndRengKey<MobileTemplateInfo>(query, 1);
            Console.WriteLine($"AppId{TemplateId};{Newtonsoft.Json.JsonConvert.SerializeObject(result)}");
            return result.FirstOrDefault();
        }
    }
}
