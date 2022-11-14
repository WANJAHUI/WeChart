using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SqsMessageHandle.Services.S3
{
    public class S3SimpleOperator
    {
        private readonly AWSS3Data _options;
        private readonly int presigndate = 1;
        public S3SimpleOperator(IOptions<AWSS3Data> options)
        {
            _options = options.Value;
        }

        AmazonS3Client CreateClient()
        {
            return new AmazonS3Client(
                _options.AccessKeyId,
                _options.SecretKey,
                new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Region),
                });
        }

        public async Task<Stream> GetFileStreamAsync(string key)
        {
            var ms = new MemoryStream();

            //await response.ResponseStream.CopyToAsync(ms);
            //return ms;
            using (var client = CreateClient())
            {
                using (var response = await client.GetObjectAsync(new GetObjectRequest()
                {
                    BucketName = _options.BucketName,
                    Key = key,
                }))
                {
                    //response.WriteResponseStreamToFile("C:\\Users\\Documents\\test.png");
                    await response.ResponseStream.CopyToAsync(ms);
                    ms.Position = 0;
                }
            }
            return ms;
        }

        public async Task<string> GetFullFilePathAsync(string key)
        {
            using (var client = CreateClient())
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.Now.AddDays(presigndate)
                };
                return await Task.FromResult(client.GetPreSignedURL(request));
            }
        }
        public async Task<string> GetDownLoadFilePathAsync(string key)
        {
            using (var client = CreateClient())
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.Now.AddDays(presigndate)
                };
                request.ResponseHeaderOverrides.ContentDisposition = "attachment";
                return await Task.FromResult(client.GetPreSignedURL(request));
            }
        }

        public async Task<string> GetDownLoadFilePathAsync(string key, DateTime expires)
        {
            using (var client = CreateClient())
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    Verb = HttpVerb.GET,
                    Expires = expires,
                };
                request.ResponseHeaderOverrides.ContentDisposition = "attachment";
                return await Task.FromResult(client.GetPreSignedURL(request));
            }
        }

        public async Task<string> UploadFileAsync(string filePath, string fileName, Stream stream)
        {
            using (var client = CreateClient())
            {
                string key = $"{filePath}/{Guid.NewGuid():N}_{fileName}";
                var request = new PutObjectRequest()
                {
                    BucketName = _options.BucketName,
                    CannedACL = S3CannedACL.AuthenticatedRead,
                    Key = key,
                    InputStream = stream,
                };

                var res = await client.PutObjectAsync(request);
                if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return key;
                }
            }

            return null;
        }

        public async Task<string> UploadFileByContentTypeAsync(string filePath, string fileName, Stream stream, string contentType)
        {
            using (var client = CreateClient())
            {
                string key = $"{filePath}/{Guid.NewGuid():N}_{fileName}";
                var request = new PutObjectRequest()
                {
                    BucketName = _options.BucketName,
                    CannedACL = S3CannedACL.AuthenticatedRead,
                    Key = key,
                    InputStream = stream
                };
                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }

                var res = await client.PutObjectAsync(request);
                if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return key;
                }
            }

            return null;
        }

        public async Task<string> UploadFileAsync(string filePath, string fileName, Stream stream, string bucketName)
        {
            using (var client = CreateClient())
            {
                string key = $"{filePath}/{Guid.NewGuid():N}_{fileName}";
                var request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.AuthenticatedRead,
                    Key = key,
                    InputStream = stream,
                };

                var res = await client.PutObjectAsync(request);
                if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return key;
                }
            }

            return null;
        }

        public async Task<string> UploadOpenFileAsync(string filePath, string fileName, Stream stream)
        {
            using (var client = CreateClient())
            {
                string key = $"{filePath}/{Guid.NewGuid():N}_{fileName}";
                var request = new PutObjectRequest()
                {
                    BucketName = _options.BucketName,
                    CannedACL = S3CannedACL.PublicRead,
                    Key = key,
                    InputStream = stream
                };

                var res = await client.PutObjectAsync(request);
                if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return GeneratePreSignedURL(client, _options.BucketName, key);
                }
            }

            return null;
        }
        private string GeneratePreSignedURL(AmazonS3Client client, string bucketName, string objectKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddDays(presigndate)
            };
            string url = client.GetPreSignedURL(request);
            if (!string.IsNullOrEmpty(url) && url.IndexOf('?') > 0)
                url = url.Substring(0, url.IndexOf('?'));
            return url;
        }

        public async Task<string> UploadStaticFileAsync(string filePath, string fileName, Stream stream)
        {
            using (var client = CreateClient())
            {
                string key = $"{filePath}/{fileName}";
                var request = new PutObjectRequest()
                {
                    BucketName = _options.BucketName,
                    //CannedACL = S3CannedACL.AuthenticatedRead,
                    Key = key,
                    InputStream = stream,
                };

                var res = await client.PutObjectAsync(request);
                if (res.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return key;
                }
            }

            return null;
        }

        public async Task ReadStreamAsync(string key, Action<Stream> read)
        {
            using (var client = CreateClient())
            {
                using (var response = await client.GetObjectAsync(new GetObjectRequest()
                {
                    BucketName = _options.BucketName,
                    Key = key,
                }))
                {
                    read(response.ResponseStream);
                }
            }
        }
        public async Task<List<string>> GetFileInfoListAsync(string path)
        {
            var msList = new List<string>();
            using (var client = CreateClient())
            {
                try
                {
                    var list = await client.ListObjectsAsync(_options.BucketName, path);
                    foreach (var item in list.S3Objects)
                    {
                        msList.Add(item.Key);
                    }
                }
                catch (Exception ex)
                {
                    return msList;
                }
            }
            return msList;
        }

        public async Task<string> GetFullFilePathAsync(string key, DateTime Expires)
        {
            using (var client = CreateClient())
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    Verb = HttpVerb.GET,
                    Expires = Expires
                };
                return await Task.FromResult(client.GetPreSignedURL(request));
            }
        }

        public async Task<bool> DeleteFileAsync(string path)
        {
            using (var client = CreateClient())
            {
                try
                {
                    await client.DeleteObjectAsync(_options.BucketName, path);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
