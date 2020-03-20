using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using git_webhook_server.PayloadModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace git_webhook_server.Controllers
{
    [Route("api/[controller]")]
    public class WebHookController : Controller
    {
        private readonly WebHookOptions _options;
        private readonly SecretOptions _secrets;
        private readonly ILogger<WebHookController> _log;

        public WebHookController(IOptions<WebHookOptions> options, IOptions<SecretOptions> secrets, ILogger<WebHookController> log)
        {
            _options = options.Value;
            _secrets = secrets.Value;
            _log = log;
        }

        // GET api/webhook
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            // read body to a memory stream so we can reuse it multiple times
            await using var body = new MemoryStream();
            await Request.Body.CopyToAsync(body);

            if (!string.IsNullOrEmpty(_secrets.WebHookSecret) && Request.Headers.ContainsKey("X-Hub-Signature"))
            {
                if (!IsValidSignature(Request.Headers["X-Hub-Signature"], body))
                {
                    _log.LogError("Invalid signature");
                    return BadRequest("Invalid signature");
                }
            }
                        
            var payload = DeserializePayload(body);

            if (payload?.Ref == null)
            {
                return BadRequest("Unsupported payload. Only push events are supported for now.");
            }

            try
            {
                foreach (var rule in _options.Rules)
                {
                    _log.LogDebug($"Try rule {rule.Name}");
                    if (payload.Ref.Equals(rule.Match, StringComparison.OrdinalIgnoreCase))
                    {
                        _log.LogInformation($"Rule {rule.Name} matches by {rule.Match}.");
                        _log.LogInformation($"Start {rule.Execute}");

                        var startInfo = new ProcessStartInfo(rule.Execute)
                        {
                            UseShellExecute = false, 
                            CreateNoWindow = true
                        };

                        Process.Start(startInfo);
                        return Ok($"Rule {rule.Name} executed");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unexpected error while trying to execute script");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
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
