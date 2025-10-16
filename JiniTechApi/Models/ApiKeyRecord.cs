using System;

namespace JiniTechApi.Models
{
    public class ApiKeyRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string KeyHash { get; set; } = string.Empty; // SHA256 hex of key
        public string Owner { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Active { get; set; } = true;
    }
}
