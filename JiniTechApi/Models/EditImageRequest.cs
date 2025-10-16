using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JiniTechApi.Models
{
    public class EditImageRequest
    {
        [Required]
        public IFormFile File { get; set; } = default!;

        [Required]
        public string Prompt { get; set; } = string.Empty;

        public int Width { get; set; } = 1024;

        public int Height { get; set; } = 1024;
    }
}
