namespace HEAL.HeuristicLibContracts.Util;

public static class NumberExtensions
{
    extension(int val)
    {
        public double Percent => val / 100.0;
        public TimeSpan Milliseconds => TimeSpan.FromMilliseconds(val);
    }
}
