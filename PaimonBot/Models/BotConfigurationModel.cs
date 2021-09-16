using Newtonsoft.Json;

namespace PaimonBot.Models
{
    public class BotConfigurationModel
    {
        [JsonProperty("BotName")]
        public string BotName { set; get; }
        [JsonProperty("LogoURL")]
        public string LogoURL { set; get; }
        [JsonProperty("Token")]
        public string token { set; get; }
        [JsonProperty("Prefixes")]
        public string[] prefixes { set; get; } // For now simply, one prefix to be used entirely
    }
}
