namespace HEAL.HeuristicLibContracts.Util;

public static class NumberExtensions
{
    extension(int val)
    {
        public double Percent => val / 100.0;
        public TimeSpan Seconds => TimeSpan.FromSeconds(val);
        public TimeSpan Minutes => TimeSpan.FromMinutes(val);
    }

    extension(double val)
    {
        public double Percent => val / 100;
        public TimeSpan Seconds => TimeSpan.FromSeconds(val);
        public TimeSpan Minutes => TimeSpan.FromMinutes(val);
    }
}
