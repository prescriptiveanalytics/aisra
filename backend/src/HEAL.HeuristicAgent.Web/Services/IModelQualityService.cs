using System.Collections.Generic;
using HEAL.HeuristicLib.Genotypes.Trees;

namespace HEAL.HeuristicAgent.Web.Services;

public interface IModelQualityService
{
    double EvaluateQuality(SymbolicExpressionTree model, IReadOnlyList<double[]> data);
    IReadOnlyList<double> EvaluateQualityOverTime(SymbolicExpressionTree model, IReadOnlyList<double[]> data, int windowSize);
}
