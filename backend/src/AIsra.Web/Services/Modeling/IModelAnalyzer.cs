using HEAL.HeuristicLib.Genotypes.Trees;

namespace AIsra.Web.Services.Modeling;

public interface IModelAnalyzer
{
    double EvaluateQuality(SymbolicExpressionTree model, double[][] data);

    /// <summary>
    /// Evaluates the model quality over time using a sliding window.
    /// </summary>
    IReadOnlyList<double> EvaluateQualityOverTime(SymbolicExpressionTree model, double[][] data, int windowSize);

    void CalculatePermutationFeatureImportances(
        Span<double> featureImportances,
        SymbolicExpressionTree model,
        double[][] data,
        int numPermutations = 5
    );
}
