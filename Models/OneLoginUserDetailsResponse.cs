using Newtonsoft.Json;
using System.Collections.Generic;

namespace OneLoginSampleAuthApi.Models
{
    public class OneLoginUserDetailsResponse
    {
        [JsonProperty("sub")]
        public string Subject { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("groups")]
        public List<string> Groups { get; set; }
    }
}
