using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Qiniu.Conf;
using Qiniu.RPC;
using System.Diagnostics;
using Windows.Web.Http;
using System.Threading.Tasks;
using Windows.Web.Http.Headers;
using System.Linq;

namespace Qiniu.IO
{
    static class MultiPart
    {
        public static Encoding encoding = Config.Encoding;

        public static string RandomBoundary()
        {
            return String.Format("----------{0:N}", Guid.NewGuid());
        }

        public static string FormDataContentType(string boundary)
        {
            return "multipart/form-data; boundary=" + boundary;
        }

        private static Stream GetPostStream(Stream putStream, string fileName, NameValueCollection formData, string boundary)
        {
            Stream postDataStream = new System.IO.MemoryStream();

            //adding form data

            string formDataHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
                "Content-Disposition: form-data; name=\"{0}\";" + Environment.NewLine + Environment.NewLine + "{1}";

            foreach (string key in formData.Keys)
            {
                byte[] formItemBytes = Encoding.UTF8.GetBytes(string.Format(formDataHeaderTemplate,
                                                                                                    key, formData[key]));
                postDataStream.Write(formItemBytes, 0, formItemBytes.Length);
            }

            //adding file,Stream data
            #region adding file data

            string fileHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
                Environment.NewLine + "Content-Type: application/octet-stream" + Environment.NewLine + Environment.NewLine;
            byte[] fileHeaderBytes = Encoding.UTF8.GetBytes(string.Format(fileHeaderTemplate,
                                                                                               "file", fileName));
            postDataStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while ((bytesRead = putStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                postDataStream.Write(buffer, 0, bytesRead);
            }
            putStream.Dispose();
            #endregion

            #region adding end
            byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + "--" + boundary + "--" + Environment.NewLine);
            postDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            #endregion

            return postDataStream;

        }

        private static Stream GetPostStream(string filePath, NameValueCollection formData, string boundary)
        {
            Stream postDataStream = new System.IO.MemoryStream();

            //adding form data

            string formDataHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
                "Content-Disposition: form-data; name=\"{0}\";" + Environment.NewLine + Environment.NewLine + "{1}";

            foreach (string key in formData.Keys)
            {
                byte[] formItemBytes = Encoding.UTF8.GetBytes(string.Format(formDataHeaderTemplate,
                                                                                                    key, formData[key]));
                postDataStream.Write(formItemBytes, 0, formItemBytes.Length);
            }

            //adding file data
            #region adding file data
            FileInfo fileInfo = new FileInfo(filePath);
            string fileHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"" +
                Environment.NewLine + "Content-Type: application/octet-stream" + Environment.NewLine + Environment.NewLine;
            byte[] fileHeaderBytes = Encoding.UTF8.GetBytes(string.Format(fileHeaderTemplate,
                                                                                               "file", fileInfo.FullName));
            postDataStream.Write(fileHeaderBytes, 0, fileHeaderBytes.Length);
            FileStream fileStream = fileInfo.OpenRead();
            byte[] buffer = new byte[1024];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                postDataStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Dispose();
            #endregion

            #region adding end
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes(Environment.NewLine + "--" + boundary + "--" + Environment.NewLine);
            postDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            #endregion

            return postDataStream;
        }

        public async static Task<CallRet> MultiPost(string url, NameValueCollection formData, string fileName, IWebProxy proxy = null)
        {
            string boundary = RandomBoundary();

            HttpClient client = new HttpClient();

            Stream formDataStream = new MemoryStream();

            FileInfo fileInfo = new FileInfo(fileName);
            using (FileStream fileStream = fileInfo.OpenRead())
            {
                formDataStream = GetPostStream(fileName, formData, boundary);
                formDataStream.Position = 0;

                var requestContent = new HttpStreamContent(formDataStream.AsRandomAccessStream());

                requestContent.Headers.ContentType = new HttpMediaTypeHeaderValue("multipart/form-data; boundary =" +boundary);

                requestContent.Headers.ContentLength = (uint)formDataStream.Length;

                try
                {
                    var resp = await client.PostAsync(new Uri(url), requestContent);
                    return await Client.HandleResult(resp);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    return new CallRet(Windows.Web.Http.HttpStatusCode.BadRequest, e);
                }
            }

            
        }

        public async static Task<CallRet> MultiPost(string url, NameValueCollection formData, Stream inputStream, IWebProxy proxy = null)
        {
            string boundary = RandomBoundary();

            HttpClient client = new HttpClient();
            HttpMultipartContent content = new HttpMultipartContent();
            content.Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);

            //WebRequest webRequest = WebRequest.Create(url);

            //if (proxy != null)
            //{
            //    webRequest.Proxy = proxy;
            //}

            //webRequest.Method = "POST";
            //webRequest.ContentType = "multipart/form-data; boundary=" + boundary;

            Stream postDataStream = GetPostStream(inputStream, formData["key"], formData, boundary);
            content.Add(new HttpStreamContent(postDataStream.AsInputStream()));

            //webRequest.ContentLength = postDataStream.Length;
            //Stream reqStream = webRequest.GetRequestStream();
            //postDataStream.Position = 0;

            //byte[] buffer = new byte[1024];
            //int bytesRead = 0;

            //while ((bytesRead = postDataStream.Read(buffer, 0, buffer.Length)) != 0)
            //{
            //    reqStream.Write(buffer, 0, bytesRead);
            //}
            //postDataStream.Dispose();
            //reqStream.Dispose();

            try
            {
                var resp = await client.PostAsync(new Uri(url), content);
                return await RPC.Client.HandleResult(resp);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return new CallRet(Windows.Web.Http.HttpStatusCode.BadRequest, e);
            }
        }
    }
}
