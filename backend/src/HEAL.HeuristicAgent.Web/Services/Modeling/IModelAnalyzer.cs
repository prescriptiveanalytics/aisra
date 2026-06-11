using HEAL.HeuristicLib.Genotypes.Trees;

namespace HEAL.HeuristicAgent.Web.Services.Modeling;

public interface IModelAnalyzer
{
    double EvaluateQuality(SymbolicExpressionTree model, double[][] data);

    /// <summary>
    /// Evaluates the model quality over time using a sliding window.
    /// </summary>
    IReadOnlyList<double> EvaluateQualityOverTime(SymbolicExpressionTree model, double[][] data, int windowSize);

    void CalculatePermutationFeatureImportance(
        Span<double> featureImportances,
        SymbolicExpressionTree model,
        double[][] data,
        int permutations = 5
    );
}
