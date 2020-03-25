using System.Collections.Generic;
using FluentAssertions;
using git_webhook_server;
using git_webhook_server.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace git_webhook_server_tests
{
    public class RuleMatcherShould
    {
        [Fact]
        public void ReturnFirstMatchedRuleByRef_IfRepositoryUrlIsNotSpecified()
        {
            var rules = new WebHookOptions
            {
                Rules = new List<WebHookRule>
                {
                    new WebHookRule
                    {
                        Name = "no match",
                        Ref = "/test/test1"
                    },
                    new WebHookRule
                    {
                        Name = "match",
                        Ref = "/test/test"
                    },
                    new WebHookRule
                    {
                        Name = "match2",
                        Ref = "/test/test"
                    }
                }
            };

            var sup = new RuleMatcher(new OptionsWrapper<WebHookOptions>(rules), new NullLogger<RuleMatcher>());

            var actual = sup.Match("/test/test", null);

            actual.Name.Should().Be("match");
        }

        [Fact]
        public void ReturnFirstMatchedRuleByMatch_IfRepositoryUrlIsNotSpecified()
        {
            var rules = new WebHookOptions
            {
                Rules = new List<WebHookRule>
                {
                    new WebHookRule
                    {
                        Name = "no match",
                        Match = "/test/test1"
                    },
                    new WebHookRule
                    {
                        Name = "match",
                        Match = "/test/test"
                    },
                    new WebHookRule
                    {
                        Name = "match2",
                        Match = "/test/test"
                    }
                }
            };

            var sup = new RuleMatcher(new OptionsWrapper<WebHookOptions>(rules), new NullLogger<RuleMatcher>());

            var actual = sup.Match("/test/test", null);

            actual.Name.Should().Be("match");
        }

        [Fact]
        public void ReturnFirstMatchedRuleByRefAndRepositoryUrl()
        {
            var rules = new WebHookOptions
            {
                Rules = new List<WebHookRule>
                {
                    new WebHookRule
                    {
                        Name = "no match",
                        Ref = "/test/test",
                        RepositoryUrl = "repo1"
                    },
                    new WebHookRule
                    {
                        Name = "match",
                        Ref = "/test/test",
                        RepositoryUrl = "repo2"
                    }
                }
            };

            var sup = new RuleMatcher(new OptionsWrapper<WebHookOptions>(rules), new NullLogger<RuleMatcher>());

            var actual = sup.Match("/test/test", "repo2");

            actual.Name.Should().Be("match");
        }
    }
}
