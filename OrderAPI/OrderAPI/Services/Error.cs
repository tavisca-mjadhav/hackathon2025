using System.Text.Json;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class Error : Exception
{
    [JsonProperty("type")]
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonProperty("title")]
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonProperty("errors")]
    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>> Errors { get; set; }

    [JsonProperty("traceId")]
    [JsonPropertyName("traceId")]
    public string TraceId { get; set; }
}
