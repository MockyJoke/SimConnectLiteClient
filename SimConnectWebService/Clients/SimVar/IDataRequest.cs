using System.Threading.Tasks;

namespace SimConnectWebService.Clients.SimVar
{
    public interface IDataRequest
    {
        Task<object> RequestValueAsync();
    }
}