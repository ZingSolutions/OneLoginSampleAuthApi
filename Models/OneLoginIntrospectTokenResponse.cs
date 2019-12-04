using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OneLoginSampleAuthApi.Models
{
    public class OneLoginIntrospectTokenResponse
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("sub")]
        public string Subject { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("exp")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset ExpiresAtUtc { get; set; }

        [JsonProperty("iat")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTimeOffset IssuedAtUtc { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonIgnore]
        public List<string> Scopes => string.IsNullOrWhiteSpace(Scope) ? new List<string>() : Scope.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
