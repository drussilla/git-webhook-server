using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using git_webhook_server.PayloadModels;
using git_webhook_server.Repositories;
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
        private readonly IEventLogRepository _eventLogRepository;
        private readonly ILogger<WebHookController> _log;

        public WebHookController(
            IPushEventProcessor pushEventProcessor, 
            IEventLogRepository eventLogRepository,
            IOptions<SecretOptions> secrets, 
            ILogger<WebHookController> log)
        {
            _secrets = secrets.Value;
            _pushEventProcessor = pushEventProcessor;
            _eventLogRepository = eventLogRepository;
            _log = log;
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var version = GetType().Assembly.GetName().Version?.ToString();
            return Ok(version);
        }

        // GET api/webhook
        [HttpPost]
        public async ValueTask<IActionResult> Post(CancellationToken token)
        {
            string body;
            using (var reader = new StreamReader(Request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            var headers = string.Join('\n', Request.Headers.Select(x => $"{x.Key}: {x.Value}"));

            var eventLog = await _eventLogRepository.CreateFromPayloadAsync(body, headers, token);
            
            if (!string.IsNullOrEmpty(_secrets.WebHookSecret))
            {
                if (!Request.Headers.ContainsKey("X-Hub-Signature"))
                {
                    eventLog.Error("Signature is not provided. X-Hub-Signature header is missing.");
                    await _eventLogRepository.UpdateAsync(eventLog);
                    _log.LogError("Signature is not provided. X-Hub-Signature header is missing.");
                    return BadRequest("Please sign payload. X-Hub-Signature header is missing.");
                }

                if (!IsValidSignature(Request.Headers["X-Hub-Signature"], body))
                {
                    eventLog.Error("Invalid signature.");
                    await _eventLogRepository.UpdateAsync(eventLog);
                    _log.LogError("Invalid signature");
                    return BadRequest("Invalid signature");
                }
            }
                        
            var payload = DeserializePayload(body);

            if (payload?.Ref == null)
            {
                eventLog.Error("Unsupported payload. Ref property is empty.");
                await _eventLogRepository.UpdateAsync(eventLog);
                return BadRequest("Unsupported payload. Only push events are supported for now.");
            }

            var result = await _pushEventProcessor.Process(eventLog.Id, payload, token);
            
            if (result.Success)
            {
                return Ok("Rule matched. Work queued");
            }

            eventLog.Succeeded = false;
            eventLog.StatusMessage = "No matching rule found";
            await _eventLogRepository.UpdateAsync(eventLog);
            
            _log.LogInformation("No matching rule found");
            return Ok("No matching rule found");
        }

        private static PushEventPayload DeserializePayload(string body)
        {
            return JsonSerializer.Deserialize<PushEventPayload>(body,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        private bool IsValidSignature(string githubSignature, string body)
        {
            using HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_secrets.WebHookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var hashString = HashEncode(hash);
            return githubSignature.Equals($"sha1={hashString}", StringComparison.OrdinalIgnoreCase);
        }

        private static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
