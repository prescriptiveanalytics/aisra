using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression.Evaluators;
using HEAL.HeuristicLib.Problems.DataAnalysis.Symbolic;

namespace HEAL.HeuristicAgent.Web.Services;

public sealed class ModelAnalysisService : IModelAnalysisService
{
    private static readonly PearsonR2Evaluator Evaluator = new();

    public double EvaluateQuality(SymbolicExpressionTree tree, double[][] data)
    {
        if (data.Length == 0)
        {
            throw new ArgumentException("Data cannot be empty.");
        }

        var variableNames = Enumerable.Range(0, data[0].Length - 1)
            .Select(i => $"x{i}")
            .Append("y")
            .ToArray();

        var dataset = Dataset.FromRowData(variableNames, data);
        var model = new SymbolicRegressionModel(
            tree,
            new SymbolicDataAnalysisExpressionTreeInterpreter()
        );

        var predictions = model.Predict(dataset, Enumerable.Range(0, dataset.Rows)).ToArray();
        var trueValues = data.Select(d => d.Last()).ToArray();

        return Evaluator.Evaluate(predictions, trueValues);
    }

    public IReadOnlyList<double> EvaluateQualityOverTime(SymbolicExpressionTree tree, double[][] data, int windowSize)
    {
        if (data.Length == 0) throw new ArgumentException("Data cannot be empty.");

        var variableNames = Enumerable.Range(0, data[0].Length - 1)
            .Select(i => $"x{i}")
            .Append("y")
            .ToArray();

        var dataset = Dataset.FromRowData(variableNames, data);
        var model = new SymbolicRegressionModel(
            tree,
            new SymbolicDataAnalysisExpressionTreeInterpreter()
        );

        var predictions = model.Predict(dataset, Enumerable.Range(0, dataset.Rows)).ToArray();
        var trueValues = data.Select(d => d.Last()).ToArray();

        var qualityOverTime = new List<double>(data.Length);

        for (var i = 0; i < data.Length; i++)
        {
            var windowStart = Math.Max(0, i - windowSize + 1);
            var windowPredictions = predictions.Skip(windowStart).Take(i - windowStart + 1).ToArray();
            var windowTrueValues = trueValues.Skip(windowStart).Take(i - windowStart + 1).ToArray();

            var quality = Evaluator.Evaluate(windowPredictions, windowTrueValues);
            qualityOverTime.Add(quality);
        }

        return qualityOverTime;
    }
}
