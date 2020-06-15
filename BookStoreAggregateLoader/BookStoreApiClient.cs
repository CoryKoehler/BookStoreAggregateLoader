using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text;

namespace BookStoreAggregateLoader
{
    public interface IBookStoreApiClient
    {
        Task<HttpResponseMessage> TryPostAsync(string route, object request);
    }

    public class BookStoreApiClient : IBookStoreApiClient
    {
        private readonly HttpClient _httpClient;
        //TODO remove this once ya know the azure url and hardcode for funs
        private readonly IConfiguration _configurationSettings;
        public BookStoreApiClient(HttpClient httpClient, IConfiguration configurationSettings)
        {
            _httpClient = httpClient;
            _configurationSettings = configurationSettings;
        }

        public async Task<HttpResponseMessage> TryPostAsync(string route, object request)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, $"{_configurationSettings.GetValue<string>("BookStoreApiUrl")}{route}")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(message, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return response;
        }
    }
}
