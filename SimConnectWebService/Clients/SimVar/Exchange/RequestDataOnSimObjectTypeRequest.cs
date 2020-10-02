using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar.Exchange
{
    public class RequestDataOnSimObjectTypeRequest
    {
        private SimConnectClient simConnectClient;
        private TaskCompletionSource<List<object>> taskCompletionSource;
        public SimVarRequestDefinition RequestDefinition { get; private set; }
        public bool IsInUse { get; private set; }
        public uint RequestId { get; private set; }
        private List<object> resultList;
        public RequestDataOnSimObjectTypeRequest(SimConnectClient simConnectClient, SimVarRequestDefinition requestDefinition, uint requestId)
        {
            this.simConnectClient = simConnectClient;
            this.RequestDefinition = requestDefinition;
            this.RequestId = requestId;
        }

        public Task<List<object>> RequestAsync(SIMCONNECT_SIMOBJECT_TYPE simObjectType)
        {
            if (taskCompletionSource != null && !taskCompletionSource.Task.IsCompleted)
            {
                throw new InvalidOperationException("This request is pending");
            }
            IsInUse = true;
            taskCompletionSource = new TaskCompletionSource<List<object>>();
            resultList = new List<object>();

            simConnectClient.SimConnect.RequestDataOnSimObjectType(
                (SimObjectDataRequestIdEnum)RequestId,
                (SimObjectDataRequestDefinitionIdEnum)RequestDefinition.DefinitionId,
                200000, simObjectType);

            return taskCompletionSource.Task;
        }

        public void DeliverResult(SimVarRequestDispatcher sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            resultList.Add(data.dwData[0]);
            if (resultList.Count == data.dwoutof)
            {
                taskCompletionSource.SetResult(resultList);
            }
        }
    }
}