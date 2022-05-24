using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Supabase.Gotrue.Responses;
using System.Threading;
using Supabase.Gotrue.Attributes;
using System.Linq;

namespace Supabase.Gotrue
{
    internal static class Helpers
    {
        public static T GetPropertyValue<T>(object obj, string propName) => (T)obj.GetType().GetProperty(propName).GetValue(obj, null);
        public static T GetCustomAttribute<T>(object obj) where T : Attribute => (T)Attribute.GetCustomAttribute(obj.GetType(), typeof(T));
        public static T GetCustomAttribute<T>(Type type) where T : Attribute => (T)Attribute.GetCustomAttribute(type, typeof(T));

        public static MapToAttribute GetMappedToAttr(Enum obj)
        {
            var type = obj.GetType();
            var name = Enum.GetName(type, obj);

            return type.GetField(name).GetCustomAttributes(false).OfType<MapToAttribute>().SingleOrDefault();
        }

        /// <summary>
        /// Adds query params to a given Url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static Uri AddQueryParams(string url, Dictionary<string, string> data)
        {
            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (var param in data)
                query[param.Key] = param.Value;

            builder.Query = query.ToString();

            return builder.Uri;
        }

        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Helper to make a request using the defined parameters to an API Endpoint and coerce into a model. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="reqParams"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<T> MakeRequest<T>(HttpMethod method, string url, object data = null, Dictionary<string, string> headers = null)
        {
            var baseResponse = await MakeRequest(method, url, data, headers);
            return JsonConvert.DeserializeObject<T>(baseResponse.Content);
        }

        /// <summary>
        /// Helper to make a request using the defined parameters to an API Endpoint.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="reqParams"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<BaseResponse> MakeRequest(HttpMethod method, string url, object data = null, Dictionary<string, string> headers = null)
        {
            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);

            if (data != null && method == HttpMethod.Get)
            {
                // Case if it's a Get request the data object is a dictionary<string,string>
                if (data is Dictionary<string, string> reqParams)
                {
                    foreach (var param in reqParams)
                        query[param.Key] = param.Value;
                }

            }

            builder.Query = query.ToString();

            using (var requestMessage = new HttpRequestMessage(method, builder.Uri))
            {

                if (data != null && method != HttpMethod.Get)
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                }

                if (headers != null)
                {
                    foreach (var kvp in headers)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                    }
                }

                var response = await client.SendAsync(requestMessage);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var obj = new ErrorResponse
                    {
                        Content = content,
                        Message = content
                    };
                    throw new RequestException(response, obj);
                }
                else
                {
                    return new BaseResponse { Content = content, ResponseMessage = response };
                }
            }
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
}
