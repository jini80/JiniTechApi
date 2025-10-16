using JiniTechApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace JiniTechApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly ApiKeyService _svc;

        public ApiKeyController(ApiKeyService svc)
        {
            _svc = svc;
        }

        public class GenerateKeyRequest
        {
            public string? Owner { get; set; }
        }

        [HttpPost("generate")]
        public IActionResult Generate([FromBody] GenerateKeyRequest? req)
        {
            var owner = req?.Owner ?? "unknown";
            var key = _svc.CreateApiKey(owner);
            // **Important**: you should show/store the key securely client-side. This is the one-time plain key.
            return Ok(new { apiKey = key, note = "Store this key now — it will not be shown again via this endpoint." });
        }

        [HttpGet("check")]
        public IActionResult Check()
        {
            var header = Request.Headers["x-api-key"].ToString();
            var ok = _svc.ValidateApiKey(header);
            if (!ok) return Unauthorized(new { status = "invalid" });
            return Ok(new { status = "valid" });
        }

        [HttpGet("list")]
        public IActionResult List()
        {
            // WARNING: this is an admin-style endpoint. Protect in production.
            return Ok(_svc.ListKeys());
        }

        public class RevokeRequest
        {
            public string? Id { get; set; }
            public string? Key { get; set; } // plain key optional
        }

        [HttpPost("revoke")]
        public IActionResult Revoke([FromBody] RevokeRequest req)
        {
            if (!string.IsNullOrEmpty(req?.Id))
            {
                var r = _svc.RevokeById(req.Id);
                return r ? Ok(new { revoked = true }) : NotFound(new { revoked = false });
            }
            if (!string.IsNullOrEmpty(req?.Key))
            {
                var r = _svc.RevokeByPlainKey(req.Key);
                return r ? Ok(new { revoked = true }) : NotFound(new { revoked = false });
            }
            return BadRequest(new { error = "Provide Id or Key to revoke." });
        }
    }
}
