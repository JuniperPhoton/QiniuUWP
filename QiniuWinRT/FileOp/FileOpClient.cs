using System;
using System.Net;
using System.IO;
using Qiniu.RPC;
using System.Diagnostics;
using Windows.Web.Http;
using System.Threading.Tasks;

namespace Qiniu.FileOp
{
	static class FileOpClient
	{
		public async static Task<CallRet> Get (string url)
		{
			try {
                HttpClient client = new HttpClient();
                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                msg.Headers.Add("User-Agent", Conf.Config.USER_AGENT);
                var resp = await client.SendRequestAsync(msg);
                return await HandleResult(resp);

			} catch (Exception e) {
				Debug.WriteLine (e.ToString ());
				return new CallRet (Windows.Web.Http.HttpStatusCode.BadRequest, e);
			}
		}

		public async static Task<CallRet> HandleResult(HttpResponseMessage response)
        {
            var statusCode = response.StatusCode;
            var msg = await response.Content.ReadAsStringAsync();

            return new CallRet(statusCode, msg);
        }
    }
}
