using System;
using System.Net;
using System.IO;
using Qiniu.RPC;
using Windows.Web.Http;

namespace Qiniu.Auth
{
	public class PutAuthClient : Client
	{
		public string UpToken { get; set; }

		public PutAuthClient (string upToken)
		{
			UpToken = upToken;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="body"></param>
		public override void SetAuth (HttpRequestMessage request,HttpClient client, Stream body)
		{
			string authHead = "UpToken " + UpToken;
            client.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("UpToken",UpToken);
        }
	}
}
