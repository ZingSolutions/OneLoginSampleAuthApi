using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OneLoginSampleAuthApi.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OneLoginSampleAuthApi.Auth
{
    public class OneLoginOpenIdConnectApiService
    {
        private readonly HttpClient oidcClient;
        private readonly string oneLoginClientId;
        private readonly string oneLoginClientSecret;

        public OneLoginOpenIdConnectApiService(HttpClient oidcClient, IOptionsMonitor<OneLoginOptions> config)
        {
            if (string.IsNullOrWhiteSpace(config?.CurrentValue?.ClientId)) { throw new ArgumentException($"{nameof(config.CurrentValue.ClientId)} property is missing", nameof(config)); }
            if (string.IsNullOrWhiteSpace(config?.CurrentValue?.ClientSecret)) { throw new ArgumentException($"{nameof(config.CurrentValue.ClientSecret)} property is missing", nameof(config)); }
            if (string.IsNullOrWhiteSpace(config?.CurrentValue?.Domain)) { throw new ArgumentException($"{nameof(config.CurrentValue.Domain)} property is missing", nameof(config)); }
     
            Uri baseApiUri;
            if (!Uri.TryCreate($"https://{config.CurrentValue.Domain}/oidc/", UriKind.Absolute, out baseApiUri) || baseApiUri == null)
            {
                throw new ArgumentException($"{nameof(config.CurrentValue.Domain)} value parsed in does not merge to a valid URI", nameof(config));
            }

            this.oidcClient = oidcClient;
            this.oidcClient.BaseAddress = baseApiUri;
            oneLoginClientId = config.CurrentValue.ClientId;
            oneLoginClientSecret = config.CurrentValue.ClientSecret;
        }

        /// <summary>
        /// Introspects the given token and returns the result.
        /// </summary>
        /// <param name="accessToken">access token to introspect.</param>
        /// <returns>Introspect result on success. null on error.</returns>
        public async Task<OneLoginIntrospectTokenResponse> IntrospectTokenAsync(string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                using (var req = new HttpRequestMessage(HttpMethod.Post, "token/introspection"))
                {
                    req.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("token", accessToken),
                    new KeyValuePair<string, string>("token_type_hint", "access_token"),
                    new KeyValuePair<string, string>("client_id", oneLoginClientId),
                    new KeyValuePair<string, string>("client_secret", oneLoginClientSecret)});

                    using (var res = await oidcClient.SendAsync(req))
                    {
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            var resContent = await res.Content.ReadAsStringAsync();
                            var introspectResult = JsonConvert.DeserializeObject<OneLoginIntrospectTokenResponse>(resContent);
                            return introspectResult;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the user details from the me endpoint for the given accessToken.
        /// </summary>
        /// <param name="accessToken">Users token to get the details for</param>
        /// <returns>User details on success. null on error.</returns>
        public async Task<OneLoginUserDetailsResponse> GetUserDetails(string accessToken)
        {
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                using (var req = new HttpRequestMessage(HttpMethod.Get, "me"))
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using (var res = await oidcClient.SendAsync(req))
                    {
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            var resContent = await res.Content.ReadAsStringAsync();
                            var userDetailResponse = JsonConvert.DeserializeObject<OneLoginUserDetailsResponse>(resContent);
                            return userDetailResponse;
                        }
                    }
                }
            }
            return null;
        }
    }
}
