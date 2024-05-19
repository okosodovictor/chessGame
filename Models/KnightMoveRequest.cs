using Newtonsoft.Json;

namespace Company.Function
{
    public class KnightMoveRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("RequestId")]
        public string RequestId { get; set; }

        [JsonProperty("startPosition")]
        public string StartPosition { get; set; }

        [JsonProperty("endPosition")]
        public string EndPosition { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}