using Qiniu.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qiniu.RS;
using Qiniu.RPC;
using Qiniu.Conf;
using Qiniu.Util;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Qiniu.PFOP
{
    public class Mkzip
    {

        /// <summary>
        /// 多文件压缩存储为用户提供了批量文件的压缩存储功能
        /// POST /pfop/ HTTP/1.1
        /// Host: api.qiniu.com  
        /// Content-Type: application/x-www-form-urlencoded  
        /// Authorization: <AccessToken>  
        /// bucket = <bucket>
        /// mkzip/<mode>
        /// /url/<Base64EncodedURL>
        /// /alias/<Base64EncodedAlias>
        /// /url/<Base64EncodedURL>
        /// ...  
        /// </summary>
        public async Task<string> doMkzip(string bucket, string existKey, string newFileName, string[] urls, string pipeline)
        {
            if (bucket == null || string.IsNullOrEmpty(existKey) || string.IsNullOrEmpty(newFileName) || urls.Length < 0 || pipeline == null)
            {
                throw new Exception("params error");
            }
            string entryURI = bucket + ":" + newFileName;
            string urlString = "";
            for (int i = 0; i < urls.Length; i++)
            {
                string urlEntry = "/url/" + Qiniu.Util.Base64URLSafe.ToBase64URLSafe(urls[i]);
                urlString += urlEntry;
            }
            string fop = System.Net.WebUtility.UrlEncode("mkzip/1" + urlString + "|saveas/" + Qiniu.Util.Base64URLSafe.ToBase64URLSafe(entryURI));

            string body = string.Format("bucket={0}&key={1}&fops={2}&pipeline={3}", bucket, existKey, fop, pipeline);

            Encoding curEncoding = Encoding.UTF8;

            QiniuAuthClient authClient = new QiniuAuthClient();
            CallRet ret = await authClient.CallWithBinary(Config.API_HOST + "/pfop/",new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue( "application/x-www-form-urlencoded"), StreamEx.ToStream(body), body.Length);
            if (ret.OK)
            {
                try
                {
                    PersistentId pid = JsonConvert.DeserializeObject<PersistentId>(ret.Response);
                    return pid.persistentId;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                throw new Exception(ret.Response);
            }
        }
    }
}
