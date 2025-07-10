
namespace NorthWind.Sales.Backend.DataContexts.Dapper.Options;

public class DBOptions
{
    public const string SectionKey = nameof(DBOptions);
    public string ConnectionString { get; set; } = string.Empty;
}
