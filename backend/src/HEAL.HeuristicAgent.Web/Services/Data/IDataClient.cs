using HEAL.HeuristicAgent.Web.Dtos;

namespace HEAL.HeuristicAgent.Web.Services.Data;

public interface IDataClient
{
    event EventHandler<DataPointDto> DataReceived;
}
