using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectWebService.Clients.SimVar.Exchange
{
    interface IRequestDataOnSimObjectRequest
    {
        uint RequestId { get; }
        void DeliverResult(SimVarRequestDispatcher dispatcher, SIMCONNECT_RECV_SIMOBJECT_DATA data);
    }
}