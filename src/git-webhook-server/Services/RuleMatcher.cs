using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace git_webhook_server.Services
{
    public class RuleMatcher : IRuleMatcher
    {
        private readonly ILogger<RuleMatcher> _log;
        private readonly WebHookOptions _options;
        public RuleMatcher(IOptions<WebHookOptions> options, ILogger<RuleMatcher> log)
        {
            _log = log;
            _options = options.Value;
        }

        public WebHookRule Match(string @ref, string repositoryUrl)
        {
            foreach (var rule in _options.Rules)
            {
                _log.LogDebug($"Try rule {rule.Name}");
                // rule.Match for backward compatibility
                if (!@ref.Equals(rule.Match ?? rule.Ref, StringComparison.OrdinalIgnoreCase))
                {
                    // skip this rule because ref doesn't match the one specified in the rule
                    continue;
                }
                    
                if (!string.IsNullOrWhiteSpace(rule.RepositoryUrl) &&
                    !@repositoryUrl.Equals(rule.RepositoryUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // skip this rule because repository url specified in the rule doesn't match the one in the payload
                    continue;
                }

                _log.LogInformation($"Rule \"{rule.Name}\" matches");
                return rule;
            }

            return null;
        }
    }
}