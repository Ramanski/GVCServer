using ModelsLibrary;
using System;
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
        Task<T> Post<T>(string uri, object valueToPost);
        Task<T> Delete<T>(string uri, object valueToDelete);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContext;
        private HttpResponseMessage response;
        private readonly BlazorServerAuthStateCache _cache;
        private readonly NotificationService _notificationService;

        public HttpService(
            HttpClient httpClient,
            IHttpContextAccessor httpContext,
            BlazorServerAuthStateCache cache,
            NotificationService notificationService
        )
        {
            _httpClient = httpClient;
            _httpContext = httpContext;
            _cache = cache;
            _notificationService = notificationService;
        }

        public async Task<T> Get<T>(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return await SendRequest<T>(request);
        }

        public async Task<T> Post<T>(string uri, object valueToPost)
        {
            HttpRequestMessage request = new (HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(valueToPost), Encoding.UTF8, "application/json")
            };
            return await SendRequest<T>(request);
        }

        public async Task<T> Delete<T>(string uri, object valueToDelete)
        {
            HttpRequestMessage request = new (HttpMethod.Delete, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(valueToDelete), Encoding.UTF8, "application/json")
            };
            return await SendRequest<T>(request);
        }

        private async Task<T> SendRequest<T>(HttpRequestMessage request)
        {
            var sessionId = _httpContext.HttpContext.User.Claims
                                                  .Where(c => c.Type.Equals("sid"))
                                                  .Select(c => c.Value)
                                                  .FirstOrDefault();
            // Attach access token to header if exists
            if (sessionId != null && _cache.HasSubjectId(sessionId))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cache.Get(sessionId)?.AccessToken);
            }
            
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                _notificationService.SetMessage(TypeNotification.Error, "Нет связи с сервером ГВЦ");
                throw new Exception();
            }

            switch (response.StatusCode){
                case HttpStatusCode.BadRequest:
                {
                    var reason = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
                    throw new RailProcessException(reason.Detail);
                }
                case HttpStatusCode.NoContent:
                    return default;
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedAccessException("Нет доступа");
                case HttpStatusCode.OK:
                    return (response.Content.Headers.ContentLength > 0) ? await response.Content.ReadFromJsonAsync<T>(): default;
                default:
                {
                    throw new Exception("Unexpected error");
                }
            }
        }
    }
}