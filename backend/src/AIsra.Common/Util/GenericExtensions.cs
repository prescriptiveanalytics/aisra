namespace AIsra.Common.Util;

public static class GenericExtensions
{
    extension<TIt, TRet>(TIt it)
    {
        public TRet Let(Func<TIt, TRet> action)
            => action(it);
    }
}
