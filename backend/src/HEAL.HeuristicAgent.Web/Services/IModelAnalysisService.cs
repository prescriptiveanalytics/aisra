using HEAL.HeuristicLib.Genotypes.Trees;

namespace HEAL.HeuristicAgent.Web.Services;

public interface IModelAnalysisService
{
    double EvaluateQuality(SymbolicExpressionTree model, double[][] data);
    IReadOnlyList<double> EvaluateQualityOverTime(SymbolicExpressionTree model, double[][] data, int windowSize);
}
