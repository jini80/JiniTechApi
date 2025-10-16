using Microsoft.AspNetCore.Http;

namespace JiniTechApi.Models
{
    public class EditVideoRequest
    {
        public IFormFile? File { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? VoiceBase64 { get; set; }
    }
}
