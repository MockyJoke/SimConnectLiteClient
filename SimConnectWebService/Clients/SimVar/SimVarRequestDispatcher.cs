using System;
using System.Collections.Concurrent;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectWebService.Clients.SimVar.Exchange;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    public class SimVarRequestDispatcher : IDisposable
    {
        private SimConnectClient simConnectClient;
        private ConcurrentDictionary<uint, IRequestDataOnSimObjectRequest> RequestPool;
        private SimConnect.RecvSimobjectDataEventHandler recvSimobjectDataEventHandler;

        private ConcurrentDictionary<uint, RequestDataOnSimObjectTypeRequest> requestDataOnSimObjectRequestPool;
        private SimConnect.RecvSimobjectDataBytypeEventHandler recvSimobjectDataBytypeEventHandler;

        public SimVarRequestDispatcher(SimConnectClient simConnectClient)
        {
            this.simConnectClient = simConnectClient;
            RequestPool = new ConcurrentDictionary<uint, IRequestDataOnSimObjectRequest>();
            recvSimobjectDataEventHandler = new SimConnect.RecvSimobjectDataEventHandler(SimConnect_OnRecvSimobjectData);
            simConnectClient.SimConnect.OnRecvSimobjectData += recvSimobjectDataEventHandler;


            requestDataOnSimObjectRequestPool = new ConcurrentDictionary<uint, RequestDataOnSimObjectTypeRequest>();
            recvSimobjectDataBytypeEventHandler = new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
            simConnectClient.SimConnect.OnRecvSimobjectDataBytype += recvSimobjectDataBytypeEventHandler;
        }

        public void Dispose()
        {
            simConnectClient.SimConnect.OnRecvSimobjectData -= recvSimobjectDataEventHandler;
            simConnectClient.SimConnect.OnRecvSimobjectDataBytype -= recvSimobjectDataBytypeEventHandler;
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


        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (requestDataOnSimObjectRequestPool.ContainsKey(data.dwRequestID))
            {
                requestDataOnSimObjectRequestPool[data.dwRequestID].DeliverResult(this, data);
            }
        }
        internal void RegisterRequest(RequestDataOnSimObjectTypeRequest request)
        {
            requestDataOnSimObjectRequestPool.TryAdd(request.RequestId, request);
        }
        internal void UnRegisterRequest(RequestDataOnSimObjectTypeRequest request)
        {
            RequestDataOnSimObjectTypeRequest outValue;
            requestDataOnSimObjectRequestPool.TryRemove(request.RequestId, out outValue);
        }
    }
}