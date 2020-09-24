using System;
using System.Threading.Tasks;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    public class SimVarRequestClient
    {
        private SimVarMappingUtil mappingUtil;
        private RecvSimobjectDataRequestDispatcher dispatcher;
        private SimConnectClient simConnectClient;
        private SimVarRequestDefinitionRegistry requestDefinitionRegistry;
        public SimVarRequestClient(SimConnectClient simConnectClient)
        {
            this.simConnectClient = simConnectClient;
            mappingUtil = new SimVarMappingUtil();
            dispatcher = new RecvSimobjectDataRequestDispatcher(simConnectClient.SimConnect);
            this.requestDefinitionRegistry = new SimVarRequestDefinitionRegistry(simConnectClient);
        }
        public void RequestDataOnSimObjectAsync(SimVarField simVar)
        {

        }

        public async Task<object> RequestDataOnSimObjectAsync(SimVarFieldGroup simVarFieldGroup)
        {
            RequestDataOnSimObjectRequest request = new RequestDataOnSimObjectRequest(
                simConnectClient,
                dispatcher,
                requestDefinitionRegistry,
                simVarFieldGroup,
                mappingUtil);
            return await request.RequestAsync();
        }
    }
}