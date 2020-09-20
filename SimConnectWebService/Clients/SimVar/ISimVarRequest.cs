using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectWebService.Clients.SimVar
{
    internal interface ISimVarRequest
    {
        uint RequestId { get; }
        void DeliverDataValue(SIMCONNECT_RECV_SIMOBJECT_DATA data);
    }
}