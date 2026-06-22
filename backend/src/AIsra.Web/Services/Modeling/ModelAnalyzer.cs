using System.Buffers;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression.Evaluators;
using HEAL.HeuristicLib.Problems.DataAnalysis.Symbolic;
using AIsra.Common.Random;

namespace AIsra.Web.Services.Modeling;

public sealed class ModelAnalyzer(IRng rng) : IModelAnalyzer
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

        try
        {
            return Evaluator.Evaluate(predictions, trueValues);
        }
        catch (InvalidOperationException)
        {
            return 0;
        }
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

    public void CalculatePermutationFeatureImportances(
        Span<double> featureImportances,
        SymbolicExpressionTree tree,
        double[][] data,
        int numPermutations = 5
    )
    {
        if (data.Length == 0)
        {
            throw new ArgumentException("Data cannot be empty.");
        }

        if (numPermutations <= 0)
        {
            throw new ArgumentException("Permutations must be greater than zero.");
        }

        var baselineQuality = EvaluateQuality(tree, data);
        var dataLength = data.Length;
        var numFeatures = data[0].Length - 1;

        var pool = ArrayPool<double>.Shared;
        var originalColumnArray = pool.Rent(dataLength);
        var shuffleBufferArray = pool.Rent(dataLength);

        try
        {
            var originalColumn = originalColumnArray.AsSpan(0, dataLength);
            var shuffleBuffer = shuffleBufferArray.AsSpan(0, dataLength);

            for (var f = 0; f < numFeatures; f++)
            {
                var featureImportanceSum = 0.0;

                for (var i = 0; i < dataLength; i++)
                {
                    originalColumn[i] = data[i][f];
                }

                for (var p = 0; p < numPermutations; p++)
                {
                    originalColumn.CopyTo(shuffleBuffer);

                    for (var i = dataLength - 1; i > 0; i--)
                    {
                        var j = rng.Next(i + 1);
                        (shuffleBuffer[i], shuffleBuffer[j]) = (shuffleBuffer[j], shuffleBuffer[i]);
                    }

                    for (var i = 0; i < dataLength; i++)
                    {
                        data[i][f] = shuffleBuffer[i];
                    }

                    var permutedQuality = EvaluateQuality(tree, data);
                    featureImportanceSum += baselineQuality - permutedQuality;
                }

                for (var i = 0; i < dataLength; i++)
                {
                    data[i][f] = originalColumn[i];
                }

                featureImportances[f] = featureImportanceSum / numPermutations;
            }
        }
        finally
        {
            pool.Return(originalColumnArray);
            pool.Return(shuffleBufferArray);
        }
    }
}
