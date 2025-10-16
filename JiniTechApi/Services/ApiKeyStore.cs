using JiniTechApi.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JiniTechApi.Services
{
    public class ApiKeyStore
    {
        private readonly string _dbPath;
        private readonly object _fileLock = new();
        private List<ApiKeyRecord> _keys;

        public ApiKeyStore(IWebHostEnvironment env)
        {
            _dbPath = Path.Combine(env.ContentRootPath, "database.json");
            try
            {
                if (!File.Exists(_dbPath))
                {
                    _keys = new List<ApiKeyRecord>();
                    Save();
                }
                else
                {
                    _keys = Load();
                }
            }
            catch
            {
                _keys = new List<ApiKeyRecord>();
                Save();
            }
        }

        private List<ApiKeyRecord> Load()
        {
            lock (_fileLock)
            {
                var json = File.ReadAllText(_dbPath);
                if (string.IsNullOrWhiteSpace(json)) return new List<ApiKeyRecord>();
                return JsonSerializer.Deserialize<List<ApiKeyRecord>>(json) ?? new List<ApiKeyRecord>();
            }
        }

        private void Save()
        {
            lock (_fileLock)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var dir = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(_dbPath, JsonSerializer.Serialize(_keys, options));
            }
        }

        public ApiKeyRecord AddKey(string plainKey, string owner)
        {
            var hash = ComputeHash(plainKey);
            var rec = new ApiKeyRecord
            {
                KeyHash = hash,
                Owner = owner,
                CreatedAt = DateTime.UtcNow,
                Active = true
            };
            _keys.Add(rec);
            Save();
            return rec;
        }

        public bool ValidateKey(string plainKey, out ApiKeyRecord? record)
        {
            record = null;
            if (string.IsNullOrEmpty(plainKey)) return false;
            var hash = ComputeHash(plainKey);
            record = _keys.FirstOrDefault(k => k.KeyHash == hash && k.Active);
            return record != null;
        }

        public List<ApiKeyRecord> ListKeys()
        {
            return _keys.Select(k => new ApiKeyRecord
            {
                Id = k.Id,
                Owner = k.Owner,
                CreatedAt = k.CreatedAt,
                Active = k.Active,
                KeyHash = "***REDACTED***"
            }).ToList();
        }

        public bool RevokeById(string id)
        {
            var found = _keys.FirstOrDefault(k => k.Id == id);
            if (found == null) return false;
            found.Active = false;
            Save();
            return true;
        }

        public bool RevokeByPlainKey(string plainKey)
        {
            var hash = ComputeHash(plainKey);
            var found = _keys.FirstOrDefault(k => k.KeyHash == hash);
            if (found == null) return false;
            found.Active = false;
            Save();
            return true;
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
