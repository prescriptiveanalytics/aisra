using AIsra.Web.Dtos;

namespace AIsra.Web.Services.Data;

public interface IDataClient
{
    event EventHandler<DataPointDto> DataReceived;
}
