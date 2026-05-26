using System.Text.Json.Serialization;

namespace FufuLauncher.Models.UIGF;

public class UIGFEntry
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; }

    [JsonPropertyName("timezone")]
    public int Timezone { get; set; } = 8;

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = "zh-cn";

    [JsonPropertyName("list")]
    public List<UIGFItem> List { get; set; } = new();
}
