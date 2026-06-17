using AIsra.Web.Services.Data;

namespace AIsra.Web.Services.Persistence;

/// <summary>
/// Stores the raw data received by the <see cref="IDataClient"/>.
/// </summary>
public interface IDataStorage
{
    Task InsertAsync(double[] data);
    IAsyncEnumerable<(DateTimeOffset, double[])> GetLastDataAsync(DateTimeOffset minTime);
    IAsyncEnumerable<(DateTimeOffset, double[])> GetLastDataAsync(int count);
}
