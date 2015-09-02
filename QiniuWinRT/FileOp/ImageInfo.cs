using System;
using Qiniu.RPC;
using System.Threading.Tasks;

namespace Qiniu.FileOp
{
	public static class ImageInfo
	{
		public static string MakeRequest (string url)
		{
			return url + "?imageInfo";
		}

		public async static Task<ImageInfoRet> Call (string url)
		{
			CallRet callRet = await FileOpClient.Get (url);
			return new ImageInfoRet (callRet);
		}
	}
}
