using System.Text.Json.Serialization;

namespace PetCenterClient.DTOs
{
    public class OdataResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string Context { get; set; }

        [JsonPropertyName("@odata.count")]
        public int? Count { get; set; }

        [JsonPropertyName("value")]
        public List<T> Values { get; set; }
    }
}
