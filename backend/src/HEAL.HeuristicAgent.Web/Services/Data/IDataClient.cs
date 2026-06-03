namespace HEAL.HeuristicAgent.Web.Services.Data;

public interface IDataClient
{
    event EventHandler<double[]> DataReceived;
}
