using System;
using System.Collections.Concurrent;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    public class RecvSimobjectDataRequestDispatcher : IDisposable
    {

        private SimConnect simConnect;
        private ConcurrentDictionary<uint, ISimVarRequest2> RequestPool2;
        private ConcurrentDictionary<uint, IRequestDataOnSimObjectRequest> RequestPool;
        private SimConnect.RecvSimobjectDataEventHandler eventHandler;
        public RecvSimobjectDataRequestDispatcher(SimConnect simConnect)
        {
            this.simConnect = simConnect;
            RequestPool = new ConcurrentDictionary<uint, IRequestDataOnSimObjectRequest>();
            RequestPool2 = new ConcurrentDictionary<uint, ISimVarRequest2>();
            eventHandler = new SimConnect.RecvSimobjectDataEventHandler(SimConnect_OnRecvSimobjectData);
            simConnect.OnRecvSimobjectData += eventHandler;
        }

        public void Dispose()
        {
            simConnect.OnRecvSimobjectData -= eventHandler;
        }

        private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            if (RequestPool2.ContainsKey(data.dwRequestID))
            {
                RequestPool2[data.dwRequestID].DeliverDataValue(data);
            }
            if (RequestPool.ContainsKey(data.dwRequestID))
            {
                RequestPool[data.dwRequestID].DeliverResult(data);
            }
        }

        internal void RegisterRequest(ISimVarRequest2 request)
        {
            RequestPool2.TryAdd(request.RequestId, request);
        }
        
        
        internal void UnRegisterRequest(ISimVarRequest2 request)
        {
            ISimVarRequest2 outValue;
            RequestPool2.TryRemove(request.RequestId, out outValue);
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