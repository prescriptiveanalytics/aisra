namespace HEAL.HeuristicAgent.Web.Data;

public interface IDataClient
{
    event EventHandler<double[]> DataReceived;
}
