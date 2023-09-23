using System.Text.Json.Serialization;

namespace ZhiPeiZaiXian;

public class Param
{
    [JsonPropertyName("video_id")] public int VideoId { get; set; }
    [JsonPropertyName("u")] public int? UId { get; set; }

    [JsonPropertyName("time")] public int Time { get; set; }
    [JsonPropertyName("unit_id")] public int UnitId { get; set; }
    [JsonPropertyName("class_id")] public int? ClassId { get; set; }
}