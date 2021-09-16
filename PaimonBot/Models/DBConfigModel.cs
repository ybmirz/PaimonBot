using Newtonsoft.Json;

namespace PaimonBot.Models
{
    public class DbConfigModel
    {
        [JsonProperty("Username")]
        public string Username { set; get; }
        [JsonProperty("Password")]
        public string Password { set; get; }
        [JsonProperty("Host")]
        public string Host { set; get; }
        [JsonProperty("database")]
        public string database { set; get; }
    }
}
