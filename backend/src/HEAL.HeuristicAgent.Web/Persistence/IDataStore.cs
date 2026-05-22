namespace HEAL.HeuristicAgent.Web.Persistence;

public interface IDataStore
{
    Task InsertAsync(double[] data);
    IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(DateTimeOffset minTime);
    IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(int count);
}
