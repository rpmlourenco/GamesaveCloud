using Microsoft.Kiota.Abstractions.Authentication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GamesaveCloudLib
{
    public class TokenProvider : IAccessTokenProvider
    {
        private readonly Func<string[], Task<string>> getTokenDelegate;
        private readonly string[] scopes;

        public TokenProvider(Func<string[], Task<string>> getTokenDelegate, string[] scopes)
        {
            this.getTokenDelegate = getTokenDelegate;
            this.scopes = scopes;
            this.AllowedHostsValidator = new AllowedHostsValidator();
        }

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = default,
            CancellationToken cancellationToken = default)
        {
            return getTokenDelegate(scopes);
        }

        public AllowedHostsValidator AllowedHostsValidator { get; }
    }
}