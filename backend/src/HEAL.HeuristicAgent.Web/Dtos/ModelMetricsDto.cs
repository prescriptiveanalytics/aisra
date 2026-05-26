namespace HEAL.HeuristicAgent.Web.Dtos;

public sealed record ModelMetricsDto(double Quality, FeatureImportanceDto[] FeatureImportances);

public sealed record FeatureImportanceDto(string Feature, double Importance);
