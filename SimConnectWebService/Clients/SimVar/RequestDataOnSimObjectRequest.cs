using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectWebService.Clients.SimVar
{
    // Represent a request to be registered with SimConnect
    public class RequestDataOnSimObjectRequest : IDisposable, ISimVarRequest, IDataRequest
    {
        private SimConnectClient simConnectClient;
        private RecvSimobjectDataRequestDispatcher dispatcher;
        private Type type;

        private TaskCompletionSource<object> taskCompletionSource;
        public RequestDataOnSimObjectRequest(
            SimConnectClient simConnectClient,
            RecvSimobjectDataRequestDispatcher recvSimobjectDataRequestDispatcher,
            Type type
        )
        {
            this.simConnectClient = simConnectClient;
            this.dispatcher = recvSimobjectDataRequestDispatcher;
            this.type = type;
        }

        // Whether this SimVarRequest has been registered with SimConnect
        public bool IsInUse { get; set; }
        public uint DefinitionId { get; set; }
        public uint RequestId { get; set; }

        public object DataValue { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public uint ObjectId { get; set; } = SimConnect.SIMCONNECT_OBJECT_ID_USER;


        public void Dispose()
        {
            if (IsInUse && simConnectClient != null)
            {
                simConnectClient.SimConnect.RequestDataOnSimObject(
                    (RecvSimobjectDataRequestIdEnum)RequestId,
                    (RecvSimobjectDataRequestDefinitionIdEnum)DefinitionId,
                    ObjectId,
                    SIMCONNECT_PERIOD.NEVER, 0, 0, 0, 0);
                simConnectClient.SimConnect.ClearDataDefinition((RecvSimobjectDataRequestDefinitionIdEnum)DefinitionId);
                IsInUse = false;
            }
        }

        public Task<object> RequestValueAsync()
        {
            if (taskCompletionSource != null && !taskCompletionSource.Task.IsCompleted)
            {
                throw new InvalidOperationException("This request is pending");
            }
            dispatcher.RegisterRequest(this);
            taskCompletionSource = new TaskCompletionSource<object>();
            IsInUse = true;
            simConnectClient.SimConnect.AddToDataDefinition(
                (RecvSimobjectDataRequestDefinitionIdEnum)DefinitionId,
                Name,
                Units,
                getSimConnectDataType(),
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);

            MethodInfo method = typeof(SimConnect).GetMethod(nameof(SimConnect.RegisterDataDefineStruct));
            MethodInfo generic = method.MakeGenericMethod(type);
            generic.Invoke(simConnectClient.SimConnect, new object[] { (RecvSimobjectDataRequestIdEnum)DefinitionId });
            //simConnectClient.SimConnect.RegisterDataDefineStruct<object>((RecvSimobjectDataRequestIdEnum)DefinitionId);
            simConnectClient.SimConnect.RequestDataOnSimObject(
                    (RecvSimobjectDataRequestIdEnum)RequestId,
                    (RecvSimobjectDataRequestDefinitionIdEnum)DefinitionId,
                    ObjectId,
                    SIMCONNECT_PERIOD.ONCE, 0, 0, 0, 0);

            return taskCompletionSource.Task;
        }

        void ISimVarRequest.DeliverDataValue(SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            DataValue = data.dwData[0];
            taskCompletionSource.SetResult(DataValue);
            dispatcher.UnRegisterRequest(this);
        }

        private SIMCONNECT_DATATYPE getSimConnectDataType()
        {
            //https://www.prepar3d.com/SDKv4/sdk/simconnect_api/references/structures_and_enumerations.html
            return SIMCONNECT_DATATYPE.STRING256;
        }

    }
}