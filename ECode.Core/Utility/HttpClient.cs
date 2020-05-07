using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ECode.IO;

namespace ECode.Utility
{
    public class HttpResponse
    {
        ~HttpResponse()
        {
            if (this.Stream != null)
            {
                this.Stream.Dispose();
                this.Stream = null;
            }
        }


        public HttpStatusCode StatusCode
        { get; set; }

        public WebHeaderCollection Headers
        { get; set; }

        public CookieCollection Cookies
        { get; set; }

        public SmartStream Stream
        { get; set; }
    }

    public enum RequestContentType
    {
        /// <summary>
        /// Content-Type not specified
        /// </summary>
        NotSpecified,

        /// <summary>
        /// application/x-www-form-urlencoded
        /// </summary>
        UrlEncoded,

        /// <summary>
        /// multipart/form-data
        /// </summary>
        Multipart,

        /// <summary>
        /// application/json
        /// </summary>
        Json,

        /// <summary>
        /// application/xml
        /// </summary>
        Xml
    }

    public static class HttpClient
    {
        static HttpClient()
        {
            //ServicePointManager.DefaultConnectionLimit = int.MaxValue;
        }


        private static HttpWebRequest CreateRequest(string url, HttpMethod method, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = method.Method;

            SetHeaders(request, headers);
            SetCookies(request, cookies);

            if (timeout.HasValue && timeout.Value > 0)
            { request.Timeout = timeout.Value; }

            if (credentials != null)
            { request.Credentials = credentials; }

            if (cachePolicy != null)
            { request.CachePolicy = cachePolicy; }

            if (clientCert != null)
            { request.ClientCertificates.Add(clientCert); }

            if (validateCallback != null)
            { request.ServerCertificateValidationCallback = validateCallback; }

            return request;
        }

        private static void SetHeaders(HttpWebRequest request, WebHeaderCollection headers)
        {
            if (headers == null || headers.Count == 0)
            { return; }

            request.Headers = new WebHeaderCollection();
            foreach (string key in headers.AllKeys)
            {
                switch (key.ToLower())
                {
                    case "host":
                        string host = headers[key];
                        if (!string.IsNullOrWhiteSpace(host))
                        { request.Host = host.Trim(); }
                        break;

                    case "accept":
                        string accept = headers[key];
                        if (!string.IsNullOrWhiteSpace(accept))
                        { request.Accept = accept.Trim(); }
                        break;

                    case "connection":
                        string connection = headers[key];
                        if (!string.IsNullOrWhiteSpace(connection))
                        { request.Connection = connection.Trim(); }
                        break;

                    case "content-type":
                        string contentType = headers[key];
                        if (!string.IsNullOrWhiteSpace(contentType))
                        { request.ContentType = contentType.Trim(); }
                        break;

                    case "expect":
                        string expect = headers[key];
                        if (!string.IsNullOrWhiteSpace(expect))
                        { request.Expect = expect.Trim(); }
                        break;

                    case "referer":
                        string referer = headers[key];
                        if (!string.IsNullOrWhiteSpace(referer))
                        { request.Referer = referer.Trim(); }
                        break;

                    case "user-agent":
                        string userAgent = headers[key];
                        if (!string.IsNullOrWhiteSpace(userAgent))
                        { request.UserAgent = userAgent.Trim(); }
                        break;

                    default:
                        string value = headers[key];
                        if (!string.IsNullOrWhiteSpace(value))
                        { request.Headers[key] = value.Trim(); }
                        break;
                }
            }
        }

        private static void SetCookies(HttpWebRequest request, CookieCollection cookies)
        {
            if (cookies == null || cookies.Count == 0)
            { return; }

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookies);
        }

        private static void SetContentType(HttpWebRequest request, RequestContentType contentType, string boundary = null)
        {
            switch (contentType)
            {
                case RequestContentType.UrlEncoded:
                    request.ContentType = "application/x-www-form-urlencoded";
                    break;

                case RequestContentType.Multipart:
                    if (string.IsNullOrWhiteSpace(boundary))
                    {
                        throw new ArgumentNullException(nameof(boundary), $"Argument '{nameof(boundary)}' cannot be null or empty while argument '{nameof(contentType)}' value is '{RequestContentType.Multipart}'.");
                    }

                    request.ContentType = $"multipart/form-data; boundary={boundary}";
                    break;

                case RequestContentType.Json:
                    request.ContentType = "application/json";
                    break;

                case RequestContentType.Xml:
                    request.ContentType = "application/xml";
                    break;
            }
        }


        private static HttpWebResponse GetResponse(HttpWebRequest request)
        {
            return (HttpWebResponse)request.GetResponse();
        }

        private static SmartStream GetResponseStream(HttpWebResponse response)
        {
            if (response.ContentEncoding == null)
            {
                return new SmartStream(response.GetResponseStream(), true);
            }
            else if (response.ContentEncoding.ToLower().Contains("gzip"))
            {
                return new SmartStream(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress), true);
            }
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
            {
                return new SmartStream(new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress), true);
            }
            else
            {
                return new SmartStream(response.GetResponseStream(), true);
            }
        }


        private static HttpResponse CreateResponse(HttpWebResponse response)
        {
            var result = new HttpResponse();
            result.StatusCode = response.StatusCode;
            result.Headers = response.Headers;
            result.Cookies = response.Cookies;

            result.Stream = new SmartStream(new HybridStream(), true);
            StreamUtil.StreamCopy(GetResponseStream(response), result.Stream);

            result.Stream.Position = 0;

            return result;
        }


        public static HttpResponse Get(string url, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            try
            {
                var request = CreateRequest(url, HttpMethod.Get, headers, cookies, timeout, credentials, cachePolicy, clientCert, validateCallback);

                return CreateResponse(GetResponse(request));
            }
            catch (ThreadAbortException)
            { throw; }
            catch (StackOverflowException)
            { throw; }
            catch (OutOfMemoryException)
            { throw; }
            catch (WebException ex)
            {
                if (ex.Response != null)
                { return CreateResponse((HttpWebResponse)ex.Response); }

                switch (ex.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                        // DNS resolve error.
                        break;
                }

                throw ex;
            }
            catch (Exception)
            { throw; }
        }

        public static HttpResponse Post(string url, Stream content = null, RequestContentType contentType = RequestContentType.NotSpecified, string boundary = null, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            try
            {
                var request = CreateRequest(url, HttpMethod.Post, headers, cookies, timeout, credentials, cachePolicy, clientCert, validateCallback);

                SetContentType(request, contentType, boundary);

                if (content == null)
                { request.ContentLength = 0; }
                else
                {
                    request.ContentLength = content.Length;
                    if (content.CanSeek)
                    { request.ContentLength = content.Length - content.Position; }

                    StreamUtil.StreamCopy(content, request.GetRequestStream());
                }

                return CreateResponse(GetResponse(request));
            }
            catch (ThreadAbortException)
            { throw; }
            catch (StackOverflowException)
            { throw; }
            catch (OutOfMemoryException)
            { throw; }
            catch (WebException ex)
            {
                if (ex.Response != null)
                { return CreateResponse((HttpWebResponse)ex.Response); }

                switch (ex.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                        // DNS resolve error.
                        break;
                }

                throw ex;
            }
            catch (Exception)
            { throw; }
        }

        public static HttpResponse Put(string url, Stream content = null, RequestContentType contentType = RequestContentType.NotSpecified, string boundary = null, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            try
            {
                var request = CreateRequest(url, HttpMethod.Put, headers, cookies, timeout, credentials, cachePolicy, clientCert, validateCallback);

                SetContentType(request, contentType, boundary);

                if (content == null)
                { request.ContentLength = 0; }
                else
                {
                    request.ContentLength = content.Length;
                    if (content.CanSeek)
                    { request.ContentLength = content.Length - content.Position; }

                    StreamUtil.StreamCopy(content, request.GetRequestStream());
                }

                return CreateResponse(GetResponse(request));
            }
            catch (ThreadAbortException)
            { throw; }
            catch (StackOverflowException)
            { throw; }
            catch (OutOfMemoryException)
            { throw; }
            catch (WebException ex)
            {
                if (ex.Response != null)
                { return CreateResponse((HttpWebResponse)ex.Response); }

                switch (ex.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                        // DNS resolve error.
                        break;
                }

                throw ex;
            }
            catch (Exception)
            { throw; }
        }

        public static HttpResponse Delete(string url, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            try
            {
                var request = CreateRequest(url, HttpMethod.Delete, headers, cookies, timeout, credentials, cachePolicy, clientCert, validateCallback);

                return CreateResponse(GetResponse(request));
            }
            catch (ThreadAbortException)
            { throw; }
            catch (StackOverflowException)
            { throw; }
            catch (OutOfMemoryException)
            { throw; }
            catch (WebException ex)
            {
                if (ex.Response != null)
                { return CreateResponse((HttpWebResponse)ex.Response); }

                switch (ex.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                        // DNS resolve error.
                        break;
                }

                throw ex;
            }
            catch (Exception)
            { throw; }
        }


        public static Task<HttpResponse> GetAsync(string url, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            return Task.Run(() => Get(url, headers, cookies, timeout, credentials, cachePolicy, clientCert, validateCallback));
        }

        public static Task<HttpResponse> PostAsync(string url, Stream content = null, RequestContentType contentType = RequestContentType.NotSpecified, string boundary = null, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            return Task.Run(() => Post(url, content, contentType, boundary, headers, cookies, timeout, credentials, cachePolicy, clientCert, validateCallback));
        }

        public static Task<HttpResponse> PutAsync(string url, Stream content = null, RequestContentType contentType = RequestContentType.NotSpecified, string boundary = null, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            return Task.Run(() => Put(url, content, contentType, boundary, headers, cookies, timeout, credentials, cachePolicy, clientCert, validateCallback));
        }

        public static Task<HttpResponse> DeleteAsync(string url, WebHeaderCollection headers = null, CookieCollection cookies = null, int? timeout = null, ICredentials credentials = null, RequestCachePolicy cachePolicy = null, X509Certificate2 clientCert = null, RemoteCertificateValidationCallback validateCallback = null)
        {
            return Task.Run(() => Delete(url, headers, cookies, timeout, credentials, cachePolicy, clientCert, validateCallback));
        }


        public static Stream BuildUrlEncodedData(IDictionary formData, Encoding encoding = null)
        {
            AssertUtil.ArgumentNotNull(formData, nameof(formData));

            encoding = encoding == null ? Encoding.UTF8 : encoding;

            var stream = new MemoryStream();
            using (var writer = new SmartStream(stream, false))
            {
                writer.Encoding = encoding;

                bool firstKeyValue = true;
                foreach (var key in formData.Keys)
                {
                    if (!(key is string))
                    { throw new ArgumentException("Key isnot typeof string."); }

                    if (string.IsNullOrWhiteSpace((string)key))
                    { throw new ArgumentException("Key cannot be null or empty."); }

                    if (formData[key] is IEnumerable)
                    {
                        foreach (var value in (IEnumerable)formData[key])
                        {
                            if (!firstKeyValue)
                            { writer.Write("&"); }

                            firstKeyValue = false;
                            writer.Write($"{HttpUtility.UrlEncode(((string)key).Trim())}={HttpUtility.UrlEncode(value?.ToString())}");
                        }
                    }
                    else
                    {
                        if (!firstKeyValue)
                        { writer.Write("&"); }

                        firstKeyValue = false;
                        writer.Write($"{HttpUtility.UrlEncode(((string)key).Trim())}={HttpUtility.UrlEncode(formData[key]?.ToString())}");
                    }
                }

                writer.Flush();
            }

            stream.Position = 0;
            return stream;
        }

        public static Stream BuildMultipartData(IDictionary formData, string boundary, Encoding encoding = null)
        {
            AssertUtil.ArgumentNotNull(formData, nameof(formData));
            AssertUtil.ArgumentNotEmpty(boundary, nameof(boundary));

            encoding = encoding == null ? Encoding.UTF8 : encoding;

            var stream = new MultiStream();

            string keyValTemplate = $"--{boundary}\r\n"
                                   + "Content-Disposition: form-data; name=\"{0}\"\r\n"
                                   + "\r\n";

            string fileValTemplate = $"--{boundary}\r\n"
                                    + "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n"
                                    + "Content-Type: application/octet-stream\r\n"
                                    + "\r\n";

            foreach (var key in formData.Keys)
            {
                if (!(key is string))
                { throw new ArgumentException("Key isnot typeof string."); }

                if (string.IsNullOrWhiteSpace((string)key))
                { throw new ArgumentException("Key cannot be null or empty."); }

                if (formData[key] is IEnumerable)
                {
                    foreach (var value in (IEnumerable)formData[key])
                    {
                        if (value is FileStream)
                        {
                            var fileValStrm = new MemoryStream();
                            using (var writer = new SmartStream(fileValStrm, false))
                            {
                                writer.Encoding = encoding;

                                writer.Write(string.Format(fileValTemplate, ((string)key).Trim(), Path.GetFileName(((FileStream)value).Name)));
                                writer.Flush();
                            }

                            fileValStrm.Position = 0;
                            stream.AppendStream(fileValStrm);

                            stream.AppendStream((FileStream)value);
                            stream.AppendStream(new MemoryStream(new byte[] { (byte)'\r', (byte)'\n' }));
                        }
                        else
                        {
                            var keyValStrm = new MemoryStream();
                            using (var writer = new SmartStream(keyValStrm, false))
                            {
                                writer.Encoding = encoding;

                                writer.Write(string.Format(keyValTemplate, ((string)key).Trim()));
                                writer.Write(value.ToString());
                                writer.Write("\r\n");
                                writer.Flush();
                            }

                            keyValStrm.Position = 0;
                            stream.AppendStream(keyValStrm);
                        }
                    }
                }
                else
                {
                    if (formData[key] is FileStream)
                    {
                        var fileValStrm = new MemoryStream();
                        using (var writer = new SmartStream(fileValStrm, false))
                        {
                            writer.Encoding = encoding;

                            writer.Write(string.Format(fileValTemplate, ((string)key).Trim(), Path.GetFileName(((FileStream)formData[key]).Name)));
                            writer.Flush();
                        }

                        fileValStrm.Position = 0;
                        stream.AppendStream(fileValStrm);

                        stream.AppendStream((FileStream)formData[key]);
                        stream.AppendStream(new MemoryStream(new byte[] { (byte)'\r', (byte)'\n' }));
                    }
                    else
                    {
                        var keyValStrm = new MemoryStream();
                        using (var writer = new SmartStream(keyValStrm, false))
                        {
                            writer.Encoding = encoding;

                            writer.Write(string.Format(keyValTemplate, ((string)key).Trim()));
                            writer.Write(formData[key]?.ToString());
                            writer.Write("\r\n");
                            writer.Flush();
                        }

                        keyValStrm.Position = 0;
                        stream.AppendStream(keyValStrm);
                    }
                }
            }

            // add end boundary string line.
            stream.AppendStream(new MemoryStream(encoding.GetBytes($"--{boundary}--\r\n")));

            return stream;
        }
    }
}