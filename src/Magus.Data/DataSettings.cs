namespace Magus.Data;

public class DataSettings
{
    public const string Data = "DataSettings";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;

}
