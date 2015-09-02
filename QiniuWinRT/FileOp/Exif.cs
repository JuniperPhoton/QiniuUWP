using System;
using Qiniu.RPC;
using System.Threading.Tasks;

namespace Qiniu.FileOp
{
	public static class Exif
	{
		public static string MakeRequest (string url)
		{
			return url + "?exif";
		}

		public async static Task<ExifRet> Call (string url)
		{
			CallRet callRet = await FileOpClient.Get (url);
			return new ExifRet (callRet);
		}
	}
}
