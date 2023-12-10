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

    public string Name { get; set; }
    public string Value { get; set; }
    public bool IsInline { get; set; }
}
