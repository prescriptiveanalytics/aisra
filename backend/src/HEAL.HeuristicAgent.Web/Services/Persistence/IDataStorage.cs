using HEAL.HeuristicAgent.Web.Services.Data;

namespace HEAL.HeuristicAgent.Web.Services.Persistence;

/// <summary>
/// Stores the raw data received by the <see cref="IDataClient"/>.
/// </summary>
public interface IDataStorage
{
    Task InsertAsync(double[] data);
    IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(DateTimeOffset minTime);
    IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(int count);
}
