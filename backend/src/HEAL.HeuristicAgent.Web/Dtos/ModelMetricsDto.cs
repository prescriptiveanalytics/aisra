namespace HEAL.HeuristicAgent.Web.Dtos;

public readonly record struct ModelMetricsDto(double Quality, FeatureImportanceDto[]? FeatureImportances = null);

public readonly record struct FeatureImportanceDto(string Feature, double Importance);
