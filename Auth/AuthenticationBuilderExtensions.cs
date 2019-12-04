using Microsoft.AspNetCore.Authentication;
using System;

namespace OneLoginSampleAuthApi.Auth
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddOneLoginOidcAccessToken(
            this AuthenticationBuilder builder, 
            string authenticationScheme, 
            Action<OneLoginOpenIdConnectAuthenticationOptions> configureOptions)
        {
            // Add custom authentication scheme with custom options and custom handler
            return builder.AddScheme<OneLoginOpenIdConnectAuthenticationOptions, OneLoginOpenIdConnectAuthenticationHandler>(authenticationScheme, configureOptions);
        }
    }
}
