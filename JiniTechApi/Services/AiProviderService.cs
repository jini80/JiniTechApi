using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace JiniTechApi.Services
{
    public class AiProviderService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _videoEndpoint;

        public AiProviderService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _cfg = cfg;
            _endpoint = _cfg["AIProvider:EndpointUrl"] ?? throw new InvalidOperationException("AIProvider:EndpointUrl missing");
            _apiKey = _cfg["AIProvider:ApiKey"] ?? "";
            _videoEndpoint = _cfg["AIProvider:VideoEndpointUrl"] ?? _endpoint;
        }

        public async Task<string> GenerateImageBase64Async(string prompt, int width = 1024, int height = 1024, CancellationToken ct = default)
        {
            var reqObj = new { prompt, width, height };
            var json = JsonSerializer.Serialize(reqObj);
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(_apiKey)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var resp = await _http.SendAsync(request, ct);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("base64", out var b64)) return b64.GetString()!;
            if (doc.RootElement.TryGetProperty("url", out var url))
            {
                var urlStr = url.GetString()!;
                var bytes = await _http.GetByteArrayAsync(urlStr);
                return Convert.ToBase64String(bytes);
            }

            throw new InvalidOperationException("AI provider response did not contain base64 or url.");
        }

        public async Task<string> GenerateVideoUrlAsync(string prompt, string? voiceBase64 = null, CancellationToken ct = default)
        {
            var reqObj = new { prompt, voiceBase64 };
            var json = JsonSerializer.Serialize(reqObj);
            var request = new HttpRequestMessage(HttpMethod.Post, _videoEndpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(_apiKey)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var resp = await _http.SendAsync(request, ct);
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("url", out var url)) return url.GetString()!;
            if (doc.RootElement.TryGetProperty("videoBase64", out var vb)) return "data:video/mp4;base64," + vb.GetString()!;
            throw new InvalidOperationException("AI provider response did not contain url or videoBase64.");
        }
    }
}
