using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace StrangeSpace
{
   /* public class HttpRequestResult<T>
    {
        public string Response { get; set; }
        public int StatusCode { get; set; }
        public T Data { get; set; }
    }

    public interface IHttpService
    {
        void Get<T>(string url, Action<HttpRequestResult<T>> callback);
        Task<HttpRequestResult<T>> GetAsync<T>(string url);

        void Post<T>(string url, object data, Action<HttpRequestResult<T>> callback);
        void Post<T>(string url, Dictionary<string, string> headers, object data, Action<HttpRequestResult<T>> callback);

        Task<HttpRequestResult<T>> PostAsync<T>(string url, object data);
        Task<HttpRequestResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, object data);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        
        // Get / Set
        public TimeSpan Timeout
        {
            get => _httpClient.Timeout;
            set => _httpClient.Timeout = value;
        }
        
        public void Get<T>(string url, Action<HttpRequestResult<T>> callback)
        {
            Task.Run(async () =>
            {
                var result = await GetAsync<T>(url);
                callback?.Invoke(result);
            });
        }

        public async Task<HttpRequestResult<T>> GetAsync<T>(string url)
        {
            var result = new HttpRequestResult<T>();
            try
            {
                var response = await _httpClient.GetAsync(url);
                result.StatusCode = (int)response.StatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    result.Data = JsonSerializer.Deserialize<T>(result.Response);
                }
            }
            catch (Exception ex)
            {
                result.Response = ex.Message;
                result.StatusCode = 500;
            }

            return result;
        }

        public void Post<T>(string url, object data, Action<HttpRequestResult<T>> callback)
        {
            Post<T>(url, new Dictionary<string, string>(), data, callback);
        }

        public void Post<T>(string url, Dictionary<string, string> headers, object data, Action<HttpRequestResult<T>> callback)
        {
            Task.Run(async () =>
            {
                var result = await PostAsync<T>(url, headers, data);
                callback?.Invoke(result);
            });
        }

        public async Task<HttpRequestResult<T>> PostAsync<T>(string url, object data)
        {
            return await PostAsync<T>(url, new Dictionary<string, string>(), data);
        }

        public async Task<HttpRequestResult<T>> PostAsync<T>(string url, Dictionary<string, string> headers, object data)
        {
            var result = new HttpRequestResult<T>();
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                foreach (var kv in headers)
                {
                    requestMessage.Headers.Add(kv.Key, kv.Value);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                result.StatusCode = (int)response.StatusCode;
                result.Response = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    result.Data = JsonSerializer.Deserialize<T>(result.Response);
                }
            }
            catch (Exception ex)
            {
                result.Response = ex.Message;
                result.StatusCode = 500;
            }

            return result;
        }
    }*/
}