using Microsoft.AspNetCore.Authentication;

namespace OneLoginSampleAuthApi.Auth
{
    public class OneLoginOpenIdConnectAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "OneLoginOpenIdConnect";
    }
}
