using System.Text.Json.Serialization;

namespace OuraRing
{
    public class ApiResultRoot
    {
        [JsonPropertyName("data")]
        public required List<HeartRate> Data { get; set; }

        [JsonPropertyName("next_token")]
        public required string NextToken { get; set; }
    }
}
