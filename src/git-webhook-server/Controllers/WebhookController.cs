using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using git_webhook_server.Models;
using git_webhook_server.PayloadModels;
using git_webhook_server.Services;
using git_webhook_server.Services.EventProcessors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace git_webhook_server.Controllers
{
    [Route("api/[controller]")]
    public class WebHookController : Controller
    {
        private readonly SecretOptions _secrets;
        private readonly IPushEventProcessor _pushEventProcessor;
        private readonly ILogger<WebHookController> _log;

        public WebHookController(IPushEventProcessor pushEventProcessor, IOptions<SecretOptions> secrets, ILogger<WebHookController> log)
        {
            _secrets = secrets.Value;
            _pushEventProcessor = pushEventProcessor;
            _log = log;
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var version = GetType().Assembly.GetName().Version.ToString();
            return Ok(version);
        }

        // GET api/webhook
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var eventLog = new EventLog();
            
            // read body to a memory stream so we can reuse it multiple times
            await using var body = new MemoryStream();
            await Request.Body.CopyToAsync(body);

            await eventLog.ReadPayload(body);

            if (!string.IsNullOrEmpty(_secrets.WebHookSecret))
            {
                if (!Request.Headers.ContainsKey("X-Hub-Signature"))
                {
                    eventLog.Error("Signature is not provided. X-Hub-Signature header is missing.");
                    _log.LogError("Signature is not provided. X-Hub-Signature header is missing.");
                    return BadRequest("Please sign payload. X-Hub-Signature header is missing.");
                }

                if (!IsValidSignature(Request.Headers["X-Hub-Signature"], body))
                {
                    eventLog.Error("Invalid signature.");
                    _log.LogError("Invalid signature");
                    return BadRequest("Invalid signature");
                }
            }
                        
            var payload = DeserializePayload(body);

            if (payload?.Ref == null)
            {
                eventLog.Error("Unsupported payload. Ref property is empty.");
                return BadRequest("Unsupported payload. Only push events are supported for now.");
            }

            var result = await _pushEventProcessor.Process(payload);
            eventLog.ReadResult(result);
            


            if (result.Success)
            {
                return Ok("Rule matched");
            }
            
            _log.LogInformation("No matching rule found");
            return Ok("No matching rule found");
        }

        private static PushEventPayload DeserializePayload(MemoryStream body)
        {
            body.Seek(0, SeekOrigin.Begin);
            return JsonSerializer.Deserialize<PushEventPayload>(new ReadOnlySpan<byte>(body.ToArray()),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        private bool IsValidSignature(string githubSignature, MemoryStream body)
        {
            using HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_secrets.WebHookSecret));
            body.Seek(0, SeekOrigin.Begin);
            var hash = hmac.ComputeHash(body);
            var hashString = HashEncode(hash);
            return githubSignature.Equals($"sha1={hashString}", StringComparison.OrdinalIgnoreCase);
        }

        private static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
