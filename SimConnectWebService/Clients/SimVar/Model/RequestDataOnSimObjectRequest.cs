using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectWebService.Clients.SimVar.Model
{
    public class RequestDataOnSimObjectRequest : IRequestDataOnSimObjectRequest, IDisposable
    {
        private static uint requestId = 0;
        private SimConnectClient simConnectClient;
        private RecvSimobjectDataRequestDispatcher dispatcher;
        private SimVarRequestDefinitionRegistry registry;

        public SimVarFieldGroup SimVarFieldGroup { get; private set; }

        private SimVarMappingUtil mappingUtil;
        private TaskCompletionSource<object> taskCompletionSource;
        private RecvSimobjectDataRequestDefinitionIdEnum definitionId;
        public bool IsInUse { get; private set; }
        public uint RequestId { get; private set; }
        public RequestDataOnSimObjectRequest(SimConnectClient simConnectClient, RecvSimobjectDataRequestDispatcher dispatcher,
        SimVarRequestDefinitionRegistry registry,
        SimVarFieldGroup simVarFieldGroup,
        SimVarMappingUtil mappingUtil)
        {
            this.simConnectClient = simConnectClient;
            this.dispatcher = dispatcher;
            this.registry = registry;
            this.SimVarFieldGroup = simVarFieldGroup;
            this.mappingUtil = mappingUtil;
            this.definitionId = registry.RegisterType(SimVarFieldGroup);
            this.RequestId = requestId++;
        }
        public Task<object> RequestAsync()
        {
            if (taskCompletionSource != null && !taskCompletionSource.Task.IsCompleted)
            {
                throw new InvalidOperationException("This request is pending");
            }
            dispatcher.RegisterRequest(this);
            taskCompletionSource = new TaskCompletionSource<object>();
            IsInUse = true;

            simConnectClient.SimConnect.RequestDataOnSimObject(
                (RecvSimobjectDataRequestIdEnum)RequestId,
                definitionId,
                mappingUtil.GetSimConnectObjectId(SimVarFieldGroup.TargetObject),
                SIMCONNECT_PERIOD.ONCE, 0, 0, 0, 0);
            return taskCompletionSource.Task;
        }

        public void DeliverResult(SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            taskCompletionSource.SetResult(data.dwData[0]);
            dispatcher.UnRegisterRequest(this);
        }

        public void Dispose()
        {
            if (IsInUse && simConnectClient != null)
            {
                simConnectClient.SimConnect.RequestDataOnSimObject(
                    (RecvSimobjectDataRequestIdEnum)RequestId,
                    definitionId,
                    mappingUtil.GetSimConnectObjectId(SimVarFieldGroup.TargetObject),
                    SIMCONNECT_PERIOD.NEVER, 0, 0, 0, 0);
                simConnectClient.SimConnect.ClearDataDefinition(definitionId);
                IsInUse = false;
            }
        }
    }
}