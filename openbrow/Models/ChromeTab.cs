using System.Text.Json.Serialization;

namespace openbrow.Models
{
    internal class ChromeTab
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("devtoolsFrontendUrl")]
        public string? DevToolsFrontendUrl { get; set; }
        
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("webSocketDebuggerUrl")]
        public string? WebSocketDebuggerUrl { get; set; }
    }
}
