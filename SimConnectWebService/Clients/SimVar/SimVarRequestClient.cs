using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimConnectWebService.Clients.SimVar.Exchange;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    public class SimVarRequestClient
    {
        private static uint requestId = 0;
        private SimConnectClient simConnectClient;
        private SimVarRequestDefinitionRegistry requestDefinitionRegistry;

        public SimVarRequestClient(SimConnectClient simConnectClient)
        {
            this.simConnectClient = simConnectClient;
            this.requestDefinitionRegistry = new SimVarRequestDefinitionRegistry(simConnectClient);
        }

        public async Task<object> RequestDataOnSimObjectAsync(SimVarFieldGroup simVarFieldGroup, uint targetObjectId)
        {
            if (!simConnectClient.IsConnected)
            {
                throw new InvalidOperationException("SimConnect is not connected yet");
            }
            SimVarRequestDefinition requestDefinition = simConnectClient.RequestDefinitionRegistry.RegisterType(simVarFieldGroup);
            RequestDataOnSimObjectRequest request = new RequestDataOnSimObjectRequest(simConnectClient, requestDefinition, targetObjectId, ++requestId);
            simConnectClient.RequestDispatcher.RegisterRequest(request);
            object result = await request.RequestAsync();
            simConnectClient.RequestDispatcher.UnRegisterRequest(request);
            return result;
        }

        public async Task<List<object>> RequestDataOnSimObjectTypeAsync(SimVarFieldGroup simVarFieldGroup)
        {
            if (!simConnectClient.IsConnected)
            {
                throw new InvalidOperationException("SimConnect is not connected yet");
            }
            SimVarRequestDefinition requestDefinition = simConnectClient.RequestDefinitionRegistry.RegisterType(simVarFieldGroup);
            RequestDataOnSimObjectTypeRequest request = new RequestDataOnSimObjectTypeRequest(simConnectClient, requestDefinition, ++requestId);
            simConnectClient.RequestDispatcher.RegisterRequest(request);
            List<object> result = await request.RequestAsync(Microsoft.FlightSimulator.SimConnect.SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT);
            simConnectClient.RequestDispatcher.UnRegisterRequest(request);
            return result;
        }
    }
}