using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OneLoginSampleAuthApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace OneLoginSampleAuthApi.Auth
{
    /// <summary>
    /// custom authentication handler for validating the access token
    /// from OneLogin, gathering the users claims and caching the result.
    /// </summary>
    public class OneLoginOpenIdConnectAuthenticationHandler : AuthenticationHandler<OneLoginOpenIdConnectAuthenticationOptions>
    {
        private const int MAX_CACHE_TIME_IN_MINUTES = 15;

        private readonly IMemoryCache cache;
        private readonly OneLoginOpenIdConnectApiService api;

        public OneLoginOpenIdConnectAuthenticationHandler(
            IOptionsMonitor<OneLoginOpenIdConnectAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMemoryCache cache,
            OneLoginOpenIdConnectApiService api) : base(options, logger, encoder, clock)
        {
            this.cache = cache;
            this.api = api;
        }

        protected async override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(new ApiErrorResponse() { StatusCode = 401, Message = "Unauthorized - Invalid Access Token" }), Encoding.UTF8);
        }

        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(new ApiErrorResponse() { StatusCode = 401, Message = "Forbidden - You do not have access to this resource" }), Encoding.UTF8);
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeaders))
            {
                const string cachePrefix = "onelogin:oidc:at:";
                const string tokenPrefix = "bearer ";
                var authHeader = authHeaders.FirstOrDefault(e => e.StartsWith(tokenPrefix, StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    //have token, trim off prefix
                    var accessToken = authHeader.Substring(tokenPrefix.Length);

                    //check for cached value
                    KeyValuePair<string, string>[] cachedClaims;
                    if (cache.TryGetValue($"{cachePrefix}{accessToken}", out cachedClaims) && cachedClaims != null && cachedClaims.Length > 0)
                    {
                        //found matching values in cache, use them and return now.
                        return GenerateSuccessResult(cachedClaims);
                    }

                    //no values in cache, call API to validate token
                    var introspectResult = await api.IntrospectTokenAsync(accessToken);
                    if (introspectResult != null && introspectResult.Active)
                    {
                        //token is active. do second query to get users details.
                        var userDetails = await api.GetUserDetails(accessToken);

                        //generate claims
                        var claimsToCache = new List<KeyValuePair<string, string>>();
                        claimsToCache.Add(new KeyValuePair<string, string>(Constants.ClaimTypes.CLIENT_ID, introspectResult.ClientId));
                        if (introspectResult.Scopes != null && introspectResult.Scopes.Count > 0)
                        {
                            introspectResult.Scopes.ForEach((e) => {
                                claimsToCache.Add(new KeyValuePair<string, string>(Constants.ClaimTypes.CLIENT_SCOPE, e));
                            });
                        }

                        claimsToCache.Add(new KeyValuePair<string, string>(Constants.ClaimTypes.USER_ID, introspectResult.Subject));
                        claimsToCache.Add(new KeyValuePair<string, string>(Constants.ClaimTypes.USER_NAME, userDetails?.Name == null ? "" : userDetails.Name));
                        claimsToCache.Add(new KeyValuePair<string, string>(Constants.ClaimTypes.USER_EMAIL, userDetails?.Email == null ? "" : userDetails.Email));
                        claimsToCache.Add(new KeyValuePair<string, string>(Constants.ClaimTypes.USER_COMPANY, userDetails?.Company == null ? "" : userDetails.Company));
                        claimsToCache.Add(new KeyValuePair<string, string>(Constants.ClaimTypes.USER_DEPARTMENT, userDetails?.Department == null ? "" : userDetails.Department));
                        if (userDetails?.Groups != null && userDetails.Groups.Count > 0)
                        {
                            userDetails.Groups.ForEach(e =>
                            {
                                claimsToCache.Add(new KeyValuePair<string, string>(Constants.ClaimTypes.USER_GROUP, e));
                            });
                        }

                        //add to cache to save network call of every request. set expiry to max or actual session expiry, whatever is the soonest.
                        //setting a short cache time so if token is revoked then this will be the max time until
                        //all local caches are cleared and access is guaranteed to be blocked.
                        var claimsArray = claimsToCache.ToArray();
                        var cacheExpiry = DateTimeOffset.UtcNow.AddMinutes(MAX_CACHE_TIME_IN_MINUTES);
                        if(cacheExpiry > introspectResult.ExpiresAtUtc)
                        {
                            cacheExpiry = introspectResult.ExpiresAtUtc;
                        }
                        cache.Set($"{cachePrefix}{accessToken}", claimsArray, cacheExpiry);
                        return GenerateSuccessResult(claimsArray);
                    }
                }
            }
            //if control gets here did not authenticate successfully
            //return NoResult so if any other authentication handlers are registered they can try.
            return AuthenticateResult.NoResult();
        }

        private AuthenticateResult GenerateSuccessResult(KeyValuePair<string, string>[] inputClaims)
        {
            var claims = new List<Claim>();
            for (int i = 0, l = inputClaims.Length; i < l; i++)
            {
                claims.Add(new Claim(inputClaims[i].Key, inputClaims[i].Value));
            }

            //NOTE: using OneLogin's groups as the role claim.
            //this will allow you to easily do role based authorisation for users in groups.
            var identities = new List<ClaimsIdentity> { new ClaimsIdentity(claims, "OneLoginOpenIdConnectAccessToken", Constants.ClaimTypes.USER_NAME, Constants.ClaimTypes.USER_GROUP) };
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identities), Scheme.Name));
        }
    }
}
