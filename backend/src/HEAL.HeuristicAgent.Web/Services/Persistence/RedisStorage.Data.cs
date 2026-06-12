using System.Globalization;
using StackExchange.Redis;

namespace HEAL.HeuristicAgent.Web.Services.Persistence;

partial class RedisStorage
{
    public async Task InsertAsync(double[] data)
    {
        await db.StreamAddAsync("data-records", [
            new NameValueEntry("data", string.Join(",", data)),
        ]);
    }

    public async IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(DateTimeOffset minTime)
    {
        var entries = await db.StreamRangeAsync(
            "data-records",
            messageOrder: Order.Descending,
            minId: $"{Math.Max(minTime.ToUnixTimeMilliseconds(), 0)}-0"
        );

        foreach (var entry in entries)
        {
            yield return (
                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(entry.Id.ToString().Split('-')[0])),
                entry.Values[0].Value.ToString().Split(',').Select(double.Parse).ToArray()
            );
        }
    }

    public async IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(int count)
    {
        var entries = await db.StreamRangeAsync(
            "data-records",
            count: count,
            messageOrder: Order.Descending
        );

        foreach (var entry in entries)
        {
            yield return (
                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(entry.Id.ToString().Split('-')[0])),
                ParseDoubles(entry.Values[0].Value.ToString())
            );
        }
    }

    private static double[] ParseDoubles(string value)
    {
        var span = value.AsSpan();
        var result = new double[span.Count(',') + 1];

        var index = 0;
        foreach (var range in span.Split(','))
        {
            result[index++] = double.Parse(span[range], CultureInfo.InvariantCulture);
        }

        return result;
    }
}
