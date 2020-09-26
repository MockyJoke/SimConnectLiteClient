using System;
using System.Threading.Tasks;
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

        public async Task<object> RequestDataOnSimObjectAsync(SimVarFieldGroup simVarFieldGroup)
        {
            SimVarRequestDefinition requestDefinition = simConnectClient.RequestDefinitionRegistry.RegisterType(simVarFieldGroup);
            RequestDataOnSimObjectRequest request = new RequestDataOnSimObjectRequest(simConnectClient, requestDefinition, ++requestId);
            simConnectClient.RequestDispatcher.RegisterRequest(request);
            object result = await request.RequestAsync();
            simConnectClient.RequestDispatcher.UnRegisterRequest(request);
            return result;
        }
    }
}