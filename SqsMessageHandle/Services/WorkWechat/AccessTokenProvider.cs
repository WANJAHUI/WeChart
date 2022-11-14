using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Senparc.Weixin.Work.Entities;
using SqsMessageHandle.EnumAndConsts;
using SqsMessageHandle.Models.WorkWechat;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SqsMessageHandle.Services.WorkWechat
{
    public class AccessTokenProvider
    {
        //  const string ACCESS_TOKEN_KEY = "WechatEnterprise:AccessToken:{0}";
        readonly IHttpClientFactory _httpClientFactory;
        public AccessTokenProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GetAccessToken(string CorpId,string CorpSecret,string AgendId)
        {
      
            var token = this.GetFromRedis(CorpId, CorpSecret , AgendId);

            if (string.IsNullOrEmpty(token))
                token = await GetFromRemote(CorpId, CorpSecret, AgendId);
            return token;
        }
        /// <summary>
        /// 从Redis获取
        /// </summary>
        /// <returns></returns>
        private string GetFromRedis(string CorpId, string CorpSecret,string AgentId)
        {

            using (var redis = new CSRedis.CSRedisClient(Consts.Redis))
            {
                var token = redis.Get<string>(string.Join(CorpId, CorpSecret,AgentId));
                Console.WriteLine($"CorpId{CorpId},CorpSecret{CorpSecret} ,token：{token}");
                return token;
            }
        }
        /// <summary>
        /// 远程获取
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetFromRemote(string CorpId, string CorpSecret,string AgentId)
        {


            using (var client = _httpClientFactory.CreateClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                Console.WriteLine("正式获取token开始");

                var url = $"https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={CorpId}&corpsecret={CorpSecret}";
                var data = await client.GetAsync(url);

                var json = await data.Content.ReadAsStringAsync();

                var token = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessToken>(json);
                Console.WriteLine("正式获取token结束");

                if (!string.IsNullOrEmpty(token.access_token))
                {
                    using (var redis = new CSRedis.CSRedisClient(Consts.Redis))
                    {
                        //存储到redis，存储周期中<10秒得被忽略
                        redis.Set(string.Join(CorpId, CorpSecret,AgentId), token.access_token, token.expires_in - 300);
                    }
                }
                return token.access_token;
            }


        }
    }
}
