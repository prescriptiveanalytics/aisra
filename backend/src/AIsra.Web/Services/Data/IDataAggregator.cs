using AIsra.Web.Dtos;

namespace AIsra.Web.Services.Data;

public interface IDataAggregator
{
    void Push(DataPointDto dataPoint);

    event EventHandler<double[]>? DataAggregated;
}
