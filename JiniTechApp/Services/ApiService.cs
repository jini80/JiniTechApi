using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JiniTechApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7292/") // Your API URL
            };
        }

        public async Task<string> CreateImageAsync(string prompt)
        {
            var response = await _httpClient.PostAsJsonAsync("api/image/create", new { Prompt = prompt });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> CreateVideoAsync(string prompt)
        {
            var response = await _httpClient.PostAsJsonAsync("api/video/create", new { Prompt = prompt });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
