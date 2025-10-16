using JiniTechApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace JiniTechApi.Services
{
    public class ApiKeyService
    {
        public const string HeaderName = "X-API-KEY";
        private readonly ApiKeyStore _store;

        public ApiKeyService(ApiKeyStore store)
        {
            _store = store;
        }

        public string CreateApiKey(string owner = "unknown")
        {
            var key = GenerateRandomHex(32);
            _store.AddKey(key, owner);
            return key;
        }

        public bool ValidateApiKey(string? key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            return _store.ValidateKey(key, out _);
        }

        public IEnumerable<object> ListKeys()
        {
            return _store.ListKeys().Select(k => new { k.Id, k.Owner, k.CreatedAt, k.Active });
        }

        public bool RevokeById(string id) => _store.RevokeById(id);
        public bool RevokeByPlainKey(string key) => _store.RevokeByPlainKey(key);

        private static string GenerateRandomHex(int bytesLength)
        {
            var bytes = new byte[bytesLength];
            RandomNumberGenerator.Fill(bytes);
            var sb = new StringBuilder(bytesLength * 2);
            foreach (var b in bytes) sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }
    }
}
