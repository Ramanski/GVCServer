using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace StationAssistant.Services
{
    public interface IHttpService
    {
        Task<T> Get<T>(string uri);
        Task<T> Post<T>(string uri, object value);
    }

    public class HttpService : IHttpService
    {
        private HttpClient _httpClient;
        private readonly IHttpContextAccessor httpContext;
        private HttpResponseMessage response;
        private readonly BlazorServerAuthStateCache _cache;

        public HttpService(
            HttpClient httpClient,
            IHttpContextAccessor httpContext,
            BlazorServerAuthStateCache cache
        )
        {
            _httpClient = httpClient;
            this.httpContext = httpContext;
            _cache = cache;
        }

        public async Task<T> Get<T>(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return await sendRequest<T>(request);
        }

        public async Task<T> Post<T>(string uri, object value)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            return await sendRequest<T>(request);
        }

        private async Task<T> sendRequest<T>(HttpRequestMessage request)
        {
            var sid = httpContext.HttpContext.User.Claims
                                                  .Where(c => c.Type.Equals("sid"))
                                                  .Select(c => c.Value)
                                                  .FirstOrDefault();
            // Attach access token to header if exists
            if (sid != null && _cache.HasSubjectId(sid))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cache.Get(sid)?.AccessToken);
            }
            
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                throw new HttpRequestException("Произошла ошибка при обращении к серверу ГВЦ");
            }

            switch (response.StatusCode){
                case HttpStatusCode.NotFound:
                case HttpStatusCode.NoContent:
                    return default(T);
                case HttpStatusCode.Unauthorized:
                    throw new Exception("Нет доступа");
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<T>();
                default:
                {
                    var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    throw new Exception(error["message"]);
                }
            }
        }
    }
}