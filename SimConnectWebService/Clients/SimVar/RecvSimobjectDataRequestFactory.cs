using System;

namespace SimConnectWebService.Clients.SimVar
{
    public class RecvSimobjectDataRequestFactory
    {
        private SimConnectClient simConnectClient;
        private RecvSimobjectDataRequestDispatcher recvSimobjectDataRequestDispatcher;
        private static uint requestId = 0;
        private static uint definitionId = 0;

        public RecvSimobjectDataRequestFactory(
            SimConnectClient simConnectClient,
            RecvSimobjectDataRequestDispatcher recvSimobjectDataRequestDispatcher)
        {
            this.simConnectClient = simConnectClient;
            this.recvSimobjectDataRequestDispatcher = recvSimobjectDataRequestDispatcher;
        }
        public RecvSimobjectDataRequest<T> CreateRecvSimobjectDataRequest<T>(string name, string units)
        {
            return new RecvSimobjectDataRequest<T>(simConnectClient, recvSimobjectDataRequestDispatcher)
            {
                Name = name,
                Units = units,
                RequestId = ++requestId,
                DefinitionId = ++definitionId
            };
        }

        public RequestDataOnSimObjectRequest CreateRequestDataOnSimObjectRequest(string name, string units, Type type)
        {
            return new RequestDataOnSimObjectRequest(simConnectClient, recvSimobjectDataRequestDispatcher, type)
            {
                Name = name,
                Units = units,
                RequestId = ++requestId,
                DefinitionId = ++definitionId
            };
        }
    }
}