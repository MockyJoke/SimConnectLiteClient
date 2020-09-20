using System;
using System.Collections.Concurrent;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectWebService.Clients.SimVar
{
    public class RecvSimobjectDataRequestDispatcher : IDisposable
    {

        private SimConnect simConnect;
        private ConcurrentDictionary<uint, ISimVarRequest> RequestPool;
        private SimConnect.RecvSimobjectDataEventHandler eventHandler;
        public RecvSimobjectDataRequestDispatcher(SimConnect simConnect)
        {
            this.simConnect = simConnect;
            RequestPool = new ConcurrentDictionary<uint, ISimVarRequest>();
            eventHandler = new SimConnect.RecvSimobjectDataEventHandler(SimConnect_OnRecvSimobjectData);
            simConnect.OnRecvSimobjectData += eventHandler;
        }

        public void Dispose()
        {
            simConnect.OnRecvSimobjectData -= eventHandler;
        }

        private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (RequestPool.ContainsKey(data.dwRequestID))
            {
                RequestPool[data.dwRequestID].DeliverDataValue(data);
            }
        }

        internal void RegisterRequest(ISimVarRequest request)
        {
            RequestPool.TryAdd(request.RequestId, request);
        }
        internal void UnRegisterRequest(ISimVarRequest request)
        {
            ISimVarRequest outValue;
            RequestPool.TryRemove(request.RequestId, out outValue);
        }
    }
}