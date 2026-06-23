namespace AIsra.HeuristicLibWeb.Server.Rest.Util;

public static class AssemblyUtil
{
    public static string XmlPath
        => Path.Combine(AppContext.BaseDirectory, $"{typeof(AssemblyUtil).Assembly.GetName().Name}.xml");
}
