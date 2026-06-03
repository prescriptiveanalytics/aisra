using ModelContextProtocol.Protocol;

namespace HEAL.HeuristicAgent.Web.Services.Mcp.Server;

public partial class HeuristicTools
{
    private static CallToolResult Do(Func<CallToolResult> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return CreateErrorResult(ex);
        }
    }

    private static async Task<CallToolResult> DoAsync(Func<Task<CallToolResult>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            return CreateErrorResult(ex);
        }
    }

    private static CallToolResult CreateErrorResult(Exception ex)
    {
        Console.WriteLine($"Error in tool execution: {ex}");

        return new CallToolResult
        {
            Content =
            [
                new TextContentBlock
                {
                    Text = $"ERROR: {ex.Message}"
                },
            ],
            IsError = true,
        };
    }

    private static CallToolResult TextResult(string text) => new()
    {
        Content =
        [
            new TextContentBlock
            {
                Text = text,
            },
        ],
    };
}
