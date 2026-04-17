using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace HEAL.HeuristicGrpc.Server;

public static class Extensions
{
    extension(IEndpointRouteBuilder builder)
    {
        public IEndpointRouteBuilder AddHeuristicGrpc()
        {
            builder.MapGrpcService<HeuristicService>();

            return builder;
        }
    }
}
