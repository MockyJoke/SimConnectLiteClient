using System;
using System.Collections.Concurrent;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    public class RequestDataOnSimObjectRequestDispatcher : IDisposable
    {
        private SimConnectClient simConnectClient;
        private ConcurrentDictionary<uint, IRequestDataOnSimObjectRequest> RequestPool;
        private SimConnect.RecvSimobjectDataEventHandler eventHandler;
        public RequestDataOnSimObjectRequestDispatcher(SimConnectClient simConnectClient)
        {
            this.simConnectClient = simConnectClient;
            RequestPool = new ConcurrentDictionary<uint, IRequestDataOnSimObjectRequest>();
            eventHandler = new SimConnect.RecvSimobjectDataEventHandler(SimConnect_OnRecvSimobjectData);
            simConnectClient.SimConnect.OnRecvSimobjectData += eventHandler;
        }

        public void Dispose()
        {
            simConnectClient.SimConnect.OnRecvSimobjectData -= eventHandler;
        }

        private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (RequestPool.ContainsKey(data.dwRequestID))
            {
                RequestPool[data.dwRequestID].DeliverResult(this, data);
            }
        }

        internal void RegisterRequest(IRequestDataOnSimObjectRequest request)
        {
            RequestPool.TryAdd(request.RequestId, request);
        }
        internal void UnRegisterRequest(IRequestDataOnSimObjectRequest request)
        {
            IRequestDataOnSimObjectRequest outValue;
            RequestPool.TryRemove(request.RequestId, out outValue);
        }
    }
}