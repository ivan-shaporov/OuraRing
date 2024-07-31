using System.Text.Json.Serialization;

namespace OuraRing
{
    public class HeartRate
    {
        [JsonPropertyName("bpm")]
        public int Bpm { get; set; }

        [JsonPropertyName("source")]
        public required string Source { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}
