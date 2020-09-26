using System;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectWebService.Clients.SimVar.Model
{
    public class RequestDataOnSimObjectRequest : IRequestDataOnSimObjectRequest, IDisposable
    {
        private SimConnectClient simConnectClient;
        private TaskCompletionSource<object> taskCompletionSource;
        public SimVarRequestDefinition RequestDefinition { get; private set; }
        public bool IsInUse { get; private set; }
        public uint RequestId { get; private set; }
        public RequestDataOnSimObjectRequest(SimConnectClient simConnectClient, SimVarRequestDefinition requestDefinition, uint requestId)
        {
            this.simConnectClient = simConnectClient;
            this.RequestDefinition = requestDefinition;
            this.RequestId = requestId;
        }

        public Task<object> RequestAsync()
        {
            if (taskCompletionSource != null && !taskCompletionSource.Task.IsCompleted)
            {
                throw new InvalidOperationException("This request is pending");
            }
            IsInUse = true;
            taskCompletionSource = new TaskCompletionSource<object>();
            simConnectClient.SimConnect.RequestDataOnSimObject(
                (SimObjectDataRequestIdEnum)RequestId,
                (SimObjectDataRequestDefinitionIdEnum)RequestDefinition.DefinitionId,
                simConnectClient.MappingUtil.GetSimConnectObjectId(RequestDefinition.FieldGroup.TargetObject),
                SIMCONNECT_PERIOD.ONCE, 0, 0, 0, 0);
            return taskCompletionSource.Task;
        }

        public void Dispose()
        {
            if (IsInUse)
            {
                simConnectClient.SimConnect.RequestDataOnSimObject(
                    (SimObjectDataRequestIdEnum)RequestId,
                    (SimObjectDataRequestDefinitionIdEnum)RequestDefinition.DefinitionId,
                    simConnectClient.MappingUtil.GetSimConnectObjectId(RequestDefinition.FieldGroup.TargetObject),
                    SIMCONNECT_PERIOD.NEVER, 0, 0, 0, 0);
                simConnectClient.SimConnect.ClearDataDefinition((SimObjectDataRequestDefinitionIdEnum)RequestDefinition.DefinitionId);
                IsInUse = false;
            }
        }

        public void DeliverResult(RequestDataOnSimObjectRequestDispatcher dispatcher, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            taskCompletionSource.SetResult(data.dwData[0]);
        }
    }
}