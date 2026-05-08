namespace HEAL.HeuristicLibContracts.Util;

public static class StringExtensions
{
    extension(string? s)
    {
        public string NotBlankOrThrow(Exception ex)
            => string.IsNullOrWhiteSpace(s) ? throw ex : s;
    }
}
