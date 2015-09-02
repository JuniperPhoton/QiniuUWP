using System;
using System.Text;
using System.Net;
using System.IO;
using Qiniu.Util;
using Qiniu.RPC;
using Qiniu.Conf;
using Qiniu.Auth.digest;
using PCLCrypto;
using Windows.Web.Http;

namespace Qiniu.Auth
{
	public class QiniuAuthClient : Client
	{
		protected Mac mac;

		public QiniuAuthClient (Mac mac = null)
		{
			this.mac = mac == null ? new Mac () : mac;
		}

		private string SignRequest (HttpRequestMessage requestMsg, byte[] body)
		{
			Uri u = requestMsg.RequestUri;

            var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha1);
            CryptographicHash hasher = algorithm.CreateHash(this.mac.SecretKey);
            hasher.Append(body);
            byte[] mac = hasher.GetValueAndReset();
            string macBase64 = Convert.ToBase64String(mac);

				string pathAndQuery = requestMsg.RequestUri.PathAndQuery;
				byte[] pathAndQueryBytes = Config.Encoding.GetBytes (pathAndQuery);
				using (MemoryStream buffer = new MemoryStream()) {
					buffer.Write (pathAndQueryBytes, 0, pathAndQueryBytes.Length);
					buffer.WriteByte ((byte)'\n');
					if (body.Length > 0) {
						buffer.Write (body, 0, body.Length);
					}
                hasher.Dispose();
					return  this.mac.AccessKey + ":" + macBase64;
				}
		}

		public override void SetAuth (HttpRequestMessage requestMsg,HttpClient client, Stream body)
		{
			string pathAndQuery = requestMsg.RequestUri.PathAndQuery;
			byte[] pathAndQueryBytes = Config.Encoding.GetBytes (pathAndQuery);
			using (MemoryStream buffer = new MemoryStream()) {
				string digestBase64 = null;
				if (requestMsg.Headers["Content-Type"] == "application/x-www-form-urlencoded" && body != null) {
					if (!body.CanSeek) {
						throw new Exception ("stream can not seek");
					}
					Util.IO.Copy (buffer, body);
					digestBase64 = SignRequest (requestMsg, buffer.ToArray());
				} else {
					buffer.Write (pathAndQueryBytes, 0, pathAndQueryBytes.Length);
					buffer.WriteByte ((byte)'\n');
					digestBase64 = mac.Sign (buffer.ToArray ());
				}
				string authHead = "QBox " + digestBase64;
				requestMsg.Headers["Authorization"]=authHead;
			}
		}
	}
}
