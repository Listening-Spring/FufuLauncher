using System.Text.Json.Serialization;

namespace FufuLauncher.Models.UIGF;

public class UIGFJson
{
    [JsonPropertyName("info")]
    public UIGFInfo Info { get; set; } = new();

    [JsonPropertyName("hk4e")]
    public List<UIGFEntry> Hk4e { get; set; } = new();
}
