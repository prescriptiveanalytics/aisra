using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression.Evaluators;
using HEAL.HeuristicLib.Problems.DataAnalysis.Symbolic;

namespace HEAL.HeuristicAgent.Web.Services.Modeling;

public sealed class ModelAnalyzer : IModelAnalyzer
{
    private static readonly PearsonR2Evaluator Evaluator = new();

    public double EvaluateQuality(SymbolicExpressionTree tree, double[][] data)
    {
        if (data.Length == 0)
        {
            throw new ArgumentException("Data cannot be empty.");
        }

        var variableNames = Enumerable.Range(1, data[0].Length - 1)
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

    /// <inheritdoc/>
    public IReadOnlyList<double> EvaluateQualityOverTime(SymbolicExpressionTree tree, double[][] data, int windowSize)
    {
        if (data.Length == 0) throw new ArgumentException("Data cannot be empty.");

        var variableNames = Enumerable.Range(1, data[0].Length - 1)
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
            var windowLen = i - windowStart + 1;

            var windowPredictions = predictions[windowStart..(windowStart + windowLen)];
            var windowTrueValues = trueValues[windowStart..(windowStart + windowLen)];

            var quality = Evaluator.Evaluate(windowPredictions, windowTrueValues);
            qualityOverTime.Add(quality);
        }

        return qualityOverTime;
    }

    public IReadOnlyList<double> CalculatePermutationFeatureImportance(
        SymbolicExpressionTree tree,
        double[][] data,
        int permutations = 5
    )
    {
        if (data.Length == 0)
        {
            throw new ArgumentException("Data cannot be empty.");
        }

        if (permutations <= 0)
        {
            throw new ArgumentException("Permutations must be greater than zero.");
        }

        var baselineQuality = EvaluateQuality(tree, data);
        var numFeatures = data[0].Length - 1;
        var importance = new double[numFeatures];
        var random = new Random(0);

        var permutedData = new double[data.Length][];
        for (var i = 0; i < data.Length; i++)
        {
            permutedData[i] = (double[])data[i].Clone();
        }

        for (var f = 0; f < numFeatures; f++)
        {
            var featureImportanceSum = 0.0;

            for (var p = 0; p < permutations; p++)
            {
                for (var i = permutedData.Length - 1; i > 0; i--)
                {
                    var j = random.Next(i + 1);
                    (permutedData[i][f], permutedData[j][f]) =
                        (permutedData[j][f], permutedData[i][f]);
                }

                var permutedQuality = EvaluateQuality(tree, permutedData);
                featureImportanceSum += baselineQuality - permutedQuality;

                for (var i = 0; i < data.Length; i++)
                {
                    permutedData[i][f] = data[i][f];
                }
            }

            importance[f] = featureImportanceSum / permutations;
        }

        return importance;
    }
}
