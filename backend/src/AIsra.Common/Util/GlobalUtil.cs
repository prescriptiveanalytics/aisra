namespace AIsra.Common.Util;

public static class GlobalUtil
{
    public static (T, T) MakeTwo<T>() where T : new()
        =>  (new T(), new T());
}
