using System.Text.Json.Serialization;

namespace OuraRing
{
    [JsonConverter(typeof(JsonStringEnumConverter<EventType>))]
    public enum EventType
    {
        create,
        update,
        delete
    }

    [JsonConverter(typeof(JsonStringEnumConverter<DataType>))]
    public enum DataType
    {
        tag,
        enhanced_tag,
        workout,
        session,
        sleep,
        daily_sleep,
        daily_readiness,
        daily_activity,
        daily_spo2,
        sleep_time,
        rest_mode_period,
        ring_configuration,
        daily_stress,
        daily_cycle_phases
    }

    //[JsonSourceGenerationOptions(UseStringEnumConverter = true, PropertyNamingPolicy =JsonKnownNamingPolicy.SnakeCaseLower)] does not work
    public class Callback
    {
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }

        [JsonPropertyName("callback_url")]
        public required Uri CallbackUrl { get; set; }

        [JsonPropertyName("event_type")]
        public EventType EventType { get; set; }

        [JsonPropertyName("data_type")]
        public DataType DataType { get; set; }

        [JsonPropertyName("expiration_time")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTime? ExpirationTime { get; set; }

        [JsonPropertyName("verification_token")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VerificationToken { get; set; }

        public override string ToString() => $"{Id}: {CallbackUrl} {DataType}/{EventType} -> {ExpirationTime}";
    }
}
