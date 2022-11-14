using System;
using System.Collections.Generic;
using System.Text;

namespace SqsMessageHandle.Services.S3
{
    public class AWSS3Data
    {

        public string AccessKeyId { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
        /// <summary>
        /// s3存储桶名称
        /// </summary>
        public string BucketName { get; set; }
        /// <summary>
        /// s3下载链接地址
        /// </summary>
        public string AwsS3Url { get; set; }
        /// <summary>
        /// s3存储桶上传文件对应目录，默认分为Dev/Test/Pro分开对应 开发/测试/正式。具体视业务流程而定
        /// </summary>
        public string AwsS3Folder { get; set; }

        /// <summary>
        /// 单号(s3目录规则：存储桶名/指定文件目录/单号/文件)
        /// </summary>
        public string FileID { get; set; }

        /// <summary>
        /// 文件名(s3目录规则：存储桶名/指定文件目录/单号/文件)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 过期时间（分钟）
        /// </summary>
        public double Duration { get; set; }
        public bool IsProduction { get; set; }

    }
}
