namespace Core.Entities
{
    using System.Text.Json.Serialization;

    public sealed class Carro : ICarro
    {
        [JsonPropertyName("fabricante")]
        public string Fabricante { get; set; }

        [JsonPropertyName("modelo")]
        public string Modelo { get; set; }

        [JsonPropertyName("cor")]
        public string Cor { get; set; }
    }
}