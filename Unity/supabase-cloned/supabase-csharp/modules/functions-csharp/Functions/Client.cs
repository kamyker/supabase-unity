using Newtonsoft.Json;
using Supabase.Functions.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Supabase.Functions
{
    public class Client
    {
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Returns an <see cref="HttpContent"/> response, allowing for coersion into Streams, Strings, and byte[]
        /// </summary>
        /// <param name="url">Url of function to invoke</param>
        /// <param name="token">Anon Key.</param>
        /// <param name="options">Options</param>
        /// <returns></returns>
        public static async Task<HttpContent> RawInvoke(string url, string token = null, InvokeFunctionOptions options = null) => (await HandleRequest(url, token, options)).Content;

        /// <summary>
        /// Invokes a function and returns the Text content of the response.
        /// </summary>
        /// <param name="url">Url of the function to invoke</param>
        /// <param name="token">Anon Key.</param>
        /// <param name="options">Options</param>
        /// <returns></returns>
        public static async Task<string> Invoke(string url, string token = null, InvokeFunctionOptions options = null)
        {
            var response = await HandleRequest(url, token, options);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Invokes a function and returns a JSON Deserialized object according to the supplied generic Type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">Url of function to invoke</param>
        /// <param name="token">Anon Key.</param>
        /// <param name="options">Options</param>
        /// <returns></returns>
        public static async Task<T> Invoke<T>(string url, string token = null, InvokeFunctionOptions options = null)
        {
            var response = await HandleRequest(url, token, options);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        /// <summary>
        /// Internal request handling
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="RequestException"></exception>
        private static async Task<HttpResponseMessage> HandleRequest(string url, string token = null, InvokeFunctionOptions options = null)
        {
            if (options == null)
            {
                options = new InvokeFunctionOptions();
            }

            if (!string.IsNullOrEmpty(token))
            {
                options.Headers["Authorization"] = $"Bearer {token}";
            }

            options.Headers["X-Client-Info"] = Util.GetAssemblyVersion();

            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);

            builder.Query = query.ToString();

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, builder.Uri))
            {
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(options.Body), Encoding.UTF8, "application/json");

                if (options.Headers != null)
                {
                    foreach (var kvp in options.Headers)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                    }
                }

                var response = await client.SendAsync(requestMessage);

                if (!response.IsSuccessStatusCode || response.Headers.Contains("x-relay-error"))
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var obj = new ErrorResponse
                    {
                        Content = content,
                        Message = content
                    };
                    throw new RequestException(response, obj);
                }

                return response;
            }
        }

        public class RequestException : Exception
        {
            public HttpResponseMessage Response { get; private set; }
            public ErrorResponse Error { get; private set; }

            public RequestException(HttpResponseMessage response, ErrorResponse error) : base(error.Message)
            {
                Response = response;
                Error = error;
            }
        }

        /// <summary>
        /// Options that can be supplied to a function invocation.
        /// 
        /// Note: If Headers.Authorization is set, it can be later overriden if a token is supplied in the method call.
        /// </summary>
        public class InvokeFunctionOptions
        {
            /// <summary>
            /// Headers to be included on the request.
            /// </summary>
            public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();


            /// <summary>
            /// Body of the Request
            /// </summary>
            [JsonProperty("body")]
            public Dictionary<string, object> Body { get; set; } = new Dictionary<string, object>();
        }
    }
}
