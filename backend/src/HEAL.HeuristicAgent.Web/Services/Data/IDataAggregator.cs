using HEAL.HeuristicAgent.Web.Dtos;

namespace HEAL.HeuristicAgent.Web.Services.Data;

public interface IDataAggregator
{
    void Push(DataPointDto dataPoint);

    event EventHandler<double[]>? DataAggregated;
}
