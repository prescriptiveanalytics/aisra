using System.ClientModel;
using System.Diagnostics;
using System.IO.Pipelines;
using HEAL.HeuristicAgent.Client.Chat;
using HEAL.HeuristicAgent.Client.Data;
using HEAL.HeuristicAgent.Client.Services;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibAdapter.Grpc;
using HEAL.HeuristicLibWrapper;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using OpenAI;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    Configuration = new ConfigurationManager().With(cfg =>
    {
        cfg
            .AddJsonFile("appsettings.jsonc")
            .AddUserSecrets<Program>();
    })
});
