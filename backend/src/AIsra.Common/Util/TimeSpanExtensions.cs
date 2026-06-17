using System.Runtime.CompilerServices;

namespace AIsra.Common.Util;

public static class TimeSpanExtensions
{
    extension(TimeSpan ts)
    {
        public TaskAwaiter GetAwaiter() => Task.Delay(ts).GetAwaiter();
        public Task WithCancellationToken(CancellationToken ct)
            => Task.Delay(ts, ct);
    }
}
