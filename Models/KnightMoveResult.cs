using Newtonsoft.Json;

namespace Company.Function
{
    public class KnightMoveResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("RequestId")]
        public string RequestId { get; set; }
        public List<string> ShortestPath { get; set; }
        public int NumberOfMoves { get; set; }

        public string StartPosition { get; set; }

        [JsonProperty("endPosition")]
        public string EndPosition { get; set; }

    }

    public class KnightPathResponse
    {
        [JsonProperty("id")]
        public string OperationId { get; set; }
        public List<string> ShortestPath { get; set; }
        public int NumberOfMoves { get; set; }

        public string StartPosition { get; set; }

        [JsonProperty("endPosition")]
        public string EndPosition { get; set; }

    }
}
