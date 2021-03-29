namespace Core.Entities
{
    using System.Text.Json.Serialization;

    public sealed class QueueMessage<T>
        where T : class
    {
        [JsonPropertyName("trace_id")]
        public string TraceId { get; set; }

        [JsonPropertyName("entity")]
        public T Entity { get; set; }
    }
}