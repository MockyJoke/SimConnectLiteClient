using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectWebService.Clients.SimVar.Model
{
    interface IRequestDataOnSimObjectRequest
    {
        uint RequestId { get; }
        void DeliverResult(SIMCONNECT_RECV_SIMOBJECT_DATA data);
    }
}