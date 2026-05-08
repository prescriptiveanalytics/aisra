using HEAL.HeuristicLib.Problems.DataAnalysis;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression.Evaluators;
using HEAL.HeuristicLib.Problems.DataAnalysis.Symbolic;
using HEAL.HeuristicAgent.Web.Mcp.Server.Tools;

namespace HEAL.HeuristicAgent.Web.Services;

public class DataReceivedEventArgs : EventArgs
{
    public double[] Data { get; init; } = [];
    public double? ModelQuality { get; init; }
}

public interface IDataClient
{
    IReadOnlyCollection<(DateTimeOffset, double[])> Data { get; }

    event EventHandler<DataReceivedEventArgs> DataReceived;
}

public sealed class DataClient : IDataClient
{
    private const int MinValue = -100;
    private const int MaxValue = 100;
    private const int NumValuesToUse = 20;

    private readonly Queue<(DateTimeOffset, double[])> _data = new();
    private static readonly PearsonR2Evaluator Evaluator = new();

    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    private static readonly Func<double, double, double> F1 = (x1, x2) => x1 * x1 * (1 + x1 / 2000) + x2 / 1.99 + 7.01;
    private static readonly Func<double, double, double> F2 = (x1, x2) => x1 * x1 * (1 + x1 / 300) + x2 / 1.88 + 7.11;

    private bool _useF2;

    public DataClient(ICancellationTokenProvider ctp)
    {
        var ct = ctp.Token;
        var startTime = DateTimeOffset.UtcNow;

        Task.Run(async () =>
        {
            var rand = new Random();
            while (!ct.IsCancellationRequested)
            {
                var x1 = rand.NextDouble() * (MaxValue - MinValue) + MinValue;
                var x2 = rand.NextDouble() * (MaxValue - MinValue) + MinValue;

                var data = new[]
                {
                    x1, x2, _useF2 ? F2(x1, x2) : F1(x1, x2)
                };

                _data.Enqueue((DateTimeOffset.UtcNow, data));

                double? quality = null;

                if (_data.Count >= NumValuesToUse)
                {
                    var model = new SymbolicRegressionModel(
                        HeuristicTools.CombinedModel,
                        new SymbolicDataAnalysisExpressionTreeInterpreter()
                    );

                    var dataset = Dataset.FromRowData(
                        Enumerable.Range(0, data.Length - 1).Select(i => $"x{i}").Append("y").ToArray(),
                        _data.TakeLast(NumValuesToUse).Select(x => x.Item2)
                    );
                    var predictedValues = model.Predict(dataset, Enumerable.Range(0, dataset.Rows));
                    var trueValues = _data.Select(d => d.Item2.Last()).TakeLast(NumValuesToUse);
                    quality = Evaluator.Evaluate(predictedValues, trueValues);
                }

                DataReceived?.Invoke(this, new DataReceivedEventArgs
                {
                    Data = data,
                    ModelQuality = quality,
                });

                await 0.5.Seconds.WithCancellationToken(ct);

                if (startTime + 20.Seconds < DateTimeOffset.UtcNow)
                {
                    _useF2 = true;
                }
            }
        }, ct);
    }

    public IReadOnlyCollection<(DateTimeOffset, double[])> Data => _data;
}
