using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Cache.CsRedis;
using Senparc.Weixin.Entities;
using Senparc.Weixin.Work;
using Senparc.Weixin.Work.Containers;
using SqsMessageHandle.EnumAndConsts;
using SqsMessageHandle.Models.WorkWechat;
using SqsMessageHandle.Services.WorkWechat;
using Senparc.CO2NET.Cache;
using System.Net.Http;
using SqsMessageHandle.Services.DynamoDb;
using SqsMessageHandle.Services;
using Senparc.Weixin.Work.AdvancedAPIs.Mass;
using SqsMessageHandle.Services.S3;
using SqsMessageHandle.Services.Email;
using SqsMessageHandle.Models;
using SqsMessageHandle.Services.Mobile;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SqsMessageHandle
{
    public class Function
    {
        private ServiceCollection _iocServcie;
        private IConfigurationRoot config;
        private readonly ServiceProvider _serviceProvider;
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json", false, false);
            Console.WriteLine("��� appsettings.json ���");

             config = configBuilder.Build();
            _iocServcie = new ServiceCollection();
            _iocServcie.AddOptions();
            _iocServcie.AddHttpClient();
            _iocServcie.AddScoped<AccessTokenProvider, AccessTokenProvider>();
            _iocServcie.AddScoped<WorkWechatService, WorkWechatService>();
             this.Init();
            _iocServcie.AddScoped<DynamoDbOperator, DynamoDbOperator>();
            _iocServcie.AddScoped<ExchangeEmailservice, ExchangeEmailservice>();
            _iocServcie.Configure<AWSS3Data>(config.GetSection("AWSS3Data"));
             _iocServcie.Configure<MailOptions>(config.GetSection("MailOptions"));
            _iocServcie.Configure<MobileOptions>(config.GetSection("MobileOptions"));
            _iocServcie.Configure<DynamoDBOptions>(config.GetSection("DynamoDBOptions"));
            _iocServcie.AddScoped<SmtpEmailService, SmtpEmailService>();
            _iocServcie.AddScoped<CommonMessageService, CommonMessageService>();
            _iocServcie.AddScoped<ApplicationAccessService, ApplicationAccessService>();
            _iocServcie.AddScoped<S3SimpleOperator, S3SimpleOperator>();
            _iocServcie.AddScoped<MobileService, MobileService>();
            _serviceProvider = _iocServcie.BuildServiceProvider(); ;


        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            var serviceProvider = _iocServcie.BuildServiceProvider();
            context.Logger.LogLine($"Processed message {Newtonsoft.Json.JsonConvert.SerializeObject(message)}");

            if (message.Attributes == null || !message.MessageAttributes.ContainsKey("MessageType"))
            {
                context.Logger.Log($"��Ч����Ϣ���ͣ�{message.Body}");
                return;
            }
            var messageType = (Msgtype)Convert.ToInt16(message.MessageAttributes["MessageType"].StringValue);
            var messageId ="";
            if (message.MessageAttributes.ContainsKey("MessageId"))
            {
                
                messageId = message.MessageAttributes["MessageId"].StringValue;
                context.Logger.Log($"��ϢID:{messageId}");
            }
            var appid = "";
            if (message.MessageAttributes.ContainsKey("AppId"))
            {

                appid = message.MessageAttributes["AppId"].StringValue;
                context.Logger.Log($"��ĿId:{appid}");
            }
            context.Logger.Log($"��Ϣ����:{messageType}");
            try
            {
                // var _service = _iocServcie.BuildServiceProvider().GetService<WorkWechatService>();
                ApplicationAccessInfo info;
                var issuccess = true;
                var errmessage = "";
                if (string.IsNullOrEmpty(appid) || appid == "giledadmin")
                {
                    info = new ApplicationAccessInfo { CorpSecret = Consts.CorpSecret, CorpId = Consts.CorpId, AgentId = Consts.AgentId };
                }
                else
                {
                    var accessService = _serviceProvider.GetRequiredService<ApplicationAccessService>();
                    info =await accessService.GetAccessInfoAsync(appid);
                    if (info == null)
                    {
                        throw new Exception($"��Ч��appId{appid},����ϵ����Ա����");
                    }
                }
            
                switch (messageType)
                {
                    case Msgtype.email:
                        var emailresult = await this.sendEmail(message.Body);
                        issuccess = emailresult.Item1; errmessage = emailresult.Item2; break;
                    case Msgtype.workwechattext:
                       // context.Logger.Log("���뵽��Ϣ����");
                        var  result= await this.sendTextMessage(message.Body,info,context);
                        issuccess = result.Item1;errmessage = result.Item2;
                       // context.Logger.Log("��������Ϣ����");
                        break;
                    case Msgtype.workwechat_miniprogram_notice:
                       
                        var minimodel = Newtonsoft.Json.JsonConvert.DeserializeObject<SendMiniProgramNoticeData>(message.Body);
                        var _miniservice = serviceProvider.GetService<WorkWechatService>();
                        _miniservice.OnLogger += a => context.Logger.Log(a);
                        var miniresult = await _miniservice.sendMiniProgramNotice(minimodel,info);
                        issuccess = miniresult.Item1; errmessage = miniresult.Item2;
                     
                        break;
                    case Msgtype.workwechat_taskcard:
                        var taskcardresult = await this.sendTaskCardMessage(message.Body, info,context);
                        issuccess = taskcardresult.Item1; errmessage = taskcardresult.Item2;
                        break;
                    case Msgtype.workwechat_markdown:
                        var markdownresult = await this.sendMarkdownAsync(message.Body, info,context);
                        issuccess = markdownresult.Item1; errmessage = markdownresult.Item2;
                        break;
                    case Msgtype.workwechat_news:
                        var newsresult = await this.sendNewsMessage(message.Body, info, context);
                        issuccess = newsresult.Item1; errmessage = newsresult.Item2;
                        break;
                    case Msgtype.mobile:
                        var mobileresult = await this.sendMobileMessage(message.Body, context);
                        issuccess = mobileresult.Item1; errmessage = mobileresult.Item2;
                        break;
                    default: issuccess = false; errmessage="��Ч����Ϣ����"; context.Logger.Log($"��Ч����Ϣ���ͣ�{messageType}"); break;
                }
                if (!string.IsNullOrEmpty(messageId))
                {
                    var logservice = serviceProvider.GetService<CommonMessageService>();
                   // context.Logger.Log($"��ϢID�޸ģ�{messageId}");
                    await  logservice.DealLogs(messageId,issuccess,errmessage);
                }

                // TODO: Do interesting work based on the new message
                await Task.CompletedTask;
                context.Logger.Log($"projectId=messageFunction|messageType={messageType.ToString()}|status=success|messageId={messageId}");
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(messageId))
                {
                   // context.Logger.Log($"������ϢmessageId{messageId}");
                      var logservice = serviceProvider.GetService<CommonMessageService>();
                    await logservice.DealLogs(messageId, false, ex.Message);
                }
                context.Logger.Log($"projectId=messageFunction|messageType={messageType.ToString()}|status=false|errmessage={ ex.Message}|messageId={messageId??""}");
                context.Logger.Log($"������Ϣ�����쳣{ Newtonsoft.Json.JsonConvert.SerializeObject(message)};������Ϣ{ex.Message}��ջ��Ϣ;{ex.InnerException?.StackTrace};{ex.InnerException?.Message}");
            }
        }
        #region ���崦���߼�
        private async Task<(bool, string)> sendTextMessage(string body,ApplicationAccessInfo info,ILambdaContext context)
        {
          //  context.Logger.Log("���뵽��Ϣ���;����߼�");
               var model = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkWechatTextMessageModel>(body);
            var _service = _serviceProvider.GetService<WorkWechatService>();
            _service.OnLogger += a => context.Logger.Log(a);
            var result = await _service.sendTextMessage(model, info);
           // context.Logger.Log("���뵽��Ϣ���ͽ����߼�");
            return result;
        }
        private async Task<(bool, string)> sendTaskCardMessage(string body, ApplicationAccessInfo info, ILambdaContext context)
        {
           // context.Logger.Log("���뵽��Ϣ���;����߼�");
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<SendTaskCardNoticeNewData>(body);
        //    context.Logger.Log($"��Ϣת�����{Newtonsoft.Json.JsonConvert.SerializeObject(model)}");
            var _service = _serviceProvider.GetService<WorkWechatService>();
            _service.OnLogger += a => context.Logger.Log(a);
            var result = await _service.sendTaskCard(model,info);
         //   context.Logger.Log("���뵽��Ϣ���ͽ����߼�");
            return result;
        }
        private async Task<(bool, string)> sendNewsMessage(string body, ApplicationAccessInfo info, ILambdaContext context)
        {
            // context.Logger.Log("���뵽��Ϣ���;����߼�");
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkWechatNewsMessageModel>(body);
            //    context.Logger.Log($"��Ϣת�����{Newtonsoft.Json.JsonConvert.SerializeObject(model)}");
            var _service = _serviceProvider.GetService<WorkWechatService>();
            _service.OnLogger += a => context.Logger.Log(a);
            var result = await _service.sendNewsAsync(model, info);
            //   context.Logger.Log("���뵽��Ϣ���ͽ����߼�");
            return result;
        }
        private async Task<(bool, string)> sendMarkdownAsync(string body, ApplicationAccessInfo info, ILambdaContext context)
        {
           // context.Logger.Log("���뵽��Ϣ���;����߼�");
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkWechatMarkdownModel>(body);
            var _service = _serviceProvider.GetService<WorkWechatService>();
            _service.OnLogger += a => context.Logger.Log(a);
            var result = await _service.sendMarkdownAsync(model,info);
         //   context.Logger.Log("���뵽��Ϣ���ͽ����߼�");
            return result;
        }

        private async Task<(bool, string)> sendMobileMessage(string body, ILambdaContext context)
        {
            //  context.Logger.Log("���뵽��Ϣ���;����߼�");
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<MobileMessageModel>(body);
            var _service = _serviceProvider.GetService<MobileService>();
            //_service.OnLogger += a => context.Logger.Log(a);
            var result = await _service.SendMobileMessage(model);
            // context.Logger.Log("���뵽��Ϣ���ͽ����߼�");
            return result;
        }
        private async Task<(bool, string)> sendEmail(string  body)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<EmailMessaeModel>(body);
            if (model.sendtype == MailSendType.Exchange)
            {
                var _service = _serviceProvider.GetService<ExchangeEmailservice>();
                var result = await _service.SendEmailByExchange(model.subject, model.contenttype, model.content, model.touser, model.cc,model.bcc, model.attachments);
                return result;

            }
            else
            {
                var _service = _serviceProvider.GetService<SmtpEmailService>();
                var result = await _service.SendAttachmentEmailAsync(model.touser,model.subject,model.contenttype,model.content,model.attachments,model.cc,model.bcc);
                return result;
            }


        }
        #endregion

        private void Init()
        {
            
           
            var senparcSetting = new SenparcSetting { IsDebug=false,Cache_Redis_Configuration= EnumAndConsts.Consts.Redis };
            var senparcWeixinSetting = new SenparcWeixinSetting { IsDebug= false, Items = new SenparcWeixinSettingItemCollection() };

            config.GetSection("SenparcSetting").Bind(senparcSetting);
            config.GetSection("SenparcWeixinSetting").Bind(senparcWeixinSetting);
           
            _iocServcie.AddSenparcGlobalServices(config);//Senparc.CO2NET ȫ��ע��

            IRegisterService register = RegisterService.Start(senparcSetting)
                                                        //���� UseSenparcGlobal() �ĸ����÷��� CO2NET Demo��https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore/Startup.cs
                                                        .UseSenparcGlobal();

            register.ChangeDefaultCacheNamespace("DefaultCO2NETCache");
            Senparc.CO2NET.APM.Config.EnableAPM = false;
          
            

            //����ȫ��ʹ��Redis���棨���裬������
            var redisConfigurationStr = senparcSetting.Cache_Redis_Configuration;

            Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);

            //���»�������ȫ�ֻ�������Ϊ Redis
            Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow();//��ֵ�Ի�����ԣ��Ƽ���
            Console.WriteLine("���� Redis UseKeyValue ����");        
            register.UseSenparcWeixinCacheCsRedis();
           
          
        }
    }
}
