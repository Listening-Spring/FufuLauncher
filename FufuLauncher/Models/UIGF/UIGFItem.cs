using System.Text.Json.Serialization;

namespace FufuLauncher.Models.UIGF;

public class UIGFItem
{
    [JsonPropertyName("uigf_gacha_type")]
    public string UigfGachaType { get; set; }

    [JsonPropertyName("gacha_type")]
    public string GachaType { get; set; }

    [JsonPropertyName("item_id")]
    public string ItemId { get; set; }

    [JsonPropertyName("count")]
    public string Count { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("item_type")]
    public string ItemType { get; set; }

    [JsonPropertyName("rank_type")]
    public string RankType { get; set; }
}
