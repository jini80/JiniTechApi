using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace JiniTechApi.Services
{
    public class FirebaseService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public FirebaseService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _baseUrl = cfg["Firebase:RealtimeDbUrl"]?.TrimEnd('/')
                       ?? throw new InvalidOperationException("Firebase RealtimeDbUrl missing");
        }

        public async Task<HttpResponseMessage> SaveJobAsync(string jobId, object payload, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/jobs/{jobId}.json";
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _http.PutAsync(url, content, ct);
        }

        public async Task<HttpResponseMessage> SaveResultAsync(string jobId, object payload, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/results/{jobId}.json";
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _http.PutAsync(url, content, ct);
        }

        public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
        {
            var url = $"{_baseUrl}/{path}.json";
            var res = await _http.GetAsync(url, ct);
            if (!res.IsSuccessStatusCode) return default;
            var body = await res.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<T>(body);
        }
    }
}
