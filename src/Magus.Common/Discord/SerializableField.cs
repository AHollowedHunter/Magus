using System.Text.Json.Serialization;

namespace Magus.Common.Discord;

public sealed class SerializableField
{
    public SerializableField(string name, string value, bool isInline = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Name = name;
        Value = value;
        IsInline = isInline;
    }

    [JsonPropertyName(nameof(Name))]
    public string Name { get; set; }

    [JsonPropertyName(nameof(Value))]
    public string Value { get; set; }

    [JsonPropertyName(nameof(IsInline))]
    public bool IsInline { get; set; }
}
