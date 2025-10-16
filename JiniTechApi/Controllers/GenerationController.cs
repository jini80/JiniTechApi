using JiniTechApi.Models;
using JiniTechApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text.Json;

namespace JiniTechApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenerateController : ControllerBase
    {
        private readonly ApiKeyService _apiKeyService;
        private readonly FirebaseService _firebase;
        private readonly AiProviderService _ai;
        private readonly ILogger<GenerateController> _logger;

        public GenerateController(
            ApiKeyService apiKeyService,
            FirebaseService firebase,
            AiProviderService ai,
            ILogger<GenerateController> logger)
        {
            _apiKeyService = apiKeyService;
            _firebase = firebase;
            _ai = ai;
            _logger = logger;
        }

        // ------------------ API KEY CHECK ------------------
        private bool CheckApiKey()
        {
            var header = Request.Headers[ApiKeyService.HeaderName].FirstOrDefault();
            return _apiKeyService.ValidateApiKey(header);
        }

        // ------------------ JOB ID GENERATOR ------------------
        private static string MakeJobId()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[12];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 16);
        }

        // ------------------ CREATE IMAGE ------------------
        [HttpPost("image")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateImage([FromBody] ImageRequest req, CancellationToken ct)
        {
            if (!CheckApiKey()) return Unauthorized();

            var jobId = MakeJobId();
            var jobMeta = new { jobId, type = "image_create", prompt = req.Prompt, createdAt = DateTime.UtcNow };
            await _firebase.SaveJobAsync(jobId, jobMeta, ct);

            try
            {
                var base64 = await _ai.GenerateImageBase64Async(req.Prompt, req.Width, req.Height, ct);
                var resultObj = new { jobId, status = "done", base64, createdAt = DateTime.UtcNow };
                await _firebase.SaveResultAsync(jobId, resultObj, ct);
                return Ok(new { jobId, status = "done", base64 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image generation failed for job {job}", jobId);
                var failObj = new { jobId, status = "failed", error = ex.Message, createdAt = DateTime.UtcNow };
                await _firebase.SaveResultAsync(jobId, failObj, ct);
                return StatusCode(500, new { jobId, status = "failed", error = ex.Message });
            }
        }

        // ------------------ EDIT IMAGE ------------------
        [HttpPost("image/edit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EditImage([FromForm] EditImageRequest req, CancellationToken ct)
        {
            if (!CheckApiKey()) return Unauthorized();
            if (req.File == null || req.File.Length == 0)
                return BadRequest("File required");

            var jobId = MakeJobId();
            var jobMeta = new
            {
                jobId,
                type = "image_edit",
                prompt = req.Prompt,
                filename = req.File.FileName,
                createdAt = DateTime.UtcNow
            };
            await _firebase.SaveJobAsync(jobId, jobMeta, ct);

            using var ms = new MemoryStream();
            await req.File.CopyToAsync(ms, ct);

            try
            {
                var editedBase64 = await _ai.GenerateImageBase64Async(
                    $"Edit uploaded image with prompt: {req.Prompt}",
                    req.Width,
                    req.Height,
                    ct
                );

                var resultObj = new { jobId, status = "done", base64 = editedBase64, createdAt = DateTime.UtcNow };
                await _firebase.SaveResultAsync(jobId, resultObj, ct);
                return Ok(new { jobId, status = "done", base64 = editedBase64 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image edit failed for job {job}", jobId);
                var failObj = new { jobId, status = "failed", error = ex.Message, createdAt = DateTime.UtcNow };
                await _firebase.SaveResultAsync(jobId, failObj, ct);
                return StatusCode(500, new { jobId, status = "failed", error = ex.Message });
            }
        }

        // ------------------ CREATE VIDEO ------------------
        [HttpPost("video")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateVideo([FromBody] VideoRequest req, CancellationToken ct)
        {
            if (!CheckApiKey()) return Unauthorized();

            var jobId = MakeJobId();
            var jobMeta = new { jobId, type = "video_create", prompt = req.Prompt, createdAt = DateTime.UtcNow };
            await _firebase.SaveJobAsync(jobId, jobMeta, ct);

            try
            {
                var videoUrl = await _ai.GenerateVideoUrlAsync(req.Prompt, req.VoiceBase64, ct);
                var resultObj = new { jobId, status = "done", url = videoUrl, createdAt = DateTime.UtcNow };
                await _firebase.SaveResultAsync(jobId, resultObj, ct);
                return Ok(new { jobId, status = "done", url = videoUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video generation failed for job {job}", jobId);
                var failObj = new { jobId, status = "failed", error = ex.Message, createdAt = DateTime.UtcNow };
                await _firebase.SaveResultAsync(jobId, failObj, ct);
                return StatusCode(500, new { jobId, status = "failed", error = ex.Message });
            }
        }

        // ------------------ EDIT VIDEO ------------------
        // ------------------ EDIT VIDEO ------------------
        [HttpPost("video/edit")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> EditVideo([FromForm] EditVideoRequest req, CancellationToken ct)
        {
            if (!CheckApiKey()) return Unauthorized();

            var jobId = MakeJobId();
            var jobMeta = new
            {
                jobId,
                type = "video_edit",
                prompt = req.Prompt,
                filename = req.File?.FileName,
                createdAt = DateTime.UtcNow
            };
            await _firebase.SaveJobAsync(jobId, jobMeta, ct);

            string? uploadedVideoBase64 = null;
            if (req.File != null)
            {
                using var ms = new MemoryStream();
                await req.File.CopyToAsync(ms, ct);
                uploadedVideoBase64 = Convert.ToBase64String(ms.ToArray());
            }

            try
            {
                var videoUrl = await _ai.GenerateVideoUrlAsync(req.Prompt, req.VoiceBase64 ?? uploadedVideoBase64, ct);
                var resultObj = new { jobId, status = "done", url = videoUrl, createdAt = DateTime.UtcNow };
                await _firebase.SaveResultAsync(jobId, resultObj, ct);
                return Ok(new { jobId, status = "done", url = videoUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video edit failed for job {job}", jobId);
                var failObj = new { jobId, status = "failed", error = ex.Message, createdAt = DateTime.UtcNow };
                await _firebase.SaveResultAsync(jobId, failObj, ct);
                return StatusCode(500, new { jobId, status = "failed", error = ex.Message });
            }
        }


        // ------------------ JOB STATUS ------------------
        [HttpGet("status/{jobId}")]
        public async Task<IActionResult> JobStatus(string jobId)
        {
            if (!CheckApiKey()) return Unauthorized();
            var obj = await _firebase.GetAsync<JsonElement?>($"results/{jobId}");
            if (obj == null) return NotFound();
            return Ok(obj);
        }
    }

    // ------------------ REQUEST MODELS ------------------
    public record ImageRequest(string Prompt, int Width = 1024, int Height = 1024);
    public record VideoRequest(string Prompt, string? VoiceBase64);
}
