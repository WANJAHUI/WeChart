using Senparc.Weixin.Work.AdvancedAPIs.Mass;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Models.WorkWechat
{
    public class SendTaskCardNoticeNewData
    {
        public string touser
        {
            get;
            set;
        }

        public string toparty
        {
            get;
            set;
        }

        public string totag
        {
            get;
            set;
        }

        public string msgtype
        {
            get;
            set;
        } = "interactive_taskcard";

        public int agentid
        {
            get;
            set;
        }

        public Taskcard_Notice interactive_taskcard
        {
            get;
            set;
        }

        //
        // Summary:
        //     表示是否开启重复消息检查，0表示否，1表示是，默认0
        public int enable_id_trans
        {
            get;
            set;
        } = 0;

        //
        // Summary:
        //     表示是否开启重复消息检查，0表示否，1表示是，默认0
        public int enable_duplicate_check
        {
            get;
            set;
        } = 0;
        //
        // Summary:
        //     表示是否重复消息检查的时间间隔，默认1800s，最大不超过4小时
        public int duplicate_check_interval
        {
            get;
            set;
        } = 1800;

    }
}
