using Qiniu.RPC;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace Qiniu.RPC
{
    public class Client
    {
        private static HttpMediaTypeHeaderValue contentType = new HttpMediaTypeHeaderValue("application/json");

        public virtual void SetAuth(HttpRequestMessage request,HttpClient client, Stream body)
        {
        }

        public async Task<CallRet> Call(string url)
        {
            Debug.WriteLine("Client.Post ==> URL: " + url);
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
                msg.Headers.Add("User-Agent", Conf.Config.USER_AGENT);
                var resp = await client.SendRequestAsync(msg);
                return await HandleResult(resp);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return new CallRet(Windows.Web.Http.HttpStatusCode.BadRequest, e);
            }
        }

        public async Task<CallRet> CallWithBinary(string url, HttpMediaTypeHeaderValue contentType, Stream body, long length)
        {
            Debug.WriteLine("Client.PostWithBinary ==> URL: {0} Length:{1}", url, length);
            try
            {
                HttpClient client = new HttpClient();
                body.Position = 0;
                HttpStreamContent streamContent = new HttpStreamContent(body.AsRandomAccessStream());
                HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
                req.Content = streamContent;
                SetAuth(req,client, body);
                var resp = await client.SendRequestAsync(req);
                return await HandleResult(resp);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return new CallRet(Windows.Web.Http.HttpStatusCode.BadRequest, e);
            }
        }

        public async static Task<CallRet> HandleResult(HttpResponseMessage response)
        {
            var statusCode = response.StatusCode;
            var resp = await response.Content.ReadAsStringAsync();

            return new CallRet(statusCode, resp);
        }
    }
}