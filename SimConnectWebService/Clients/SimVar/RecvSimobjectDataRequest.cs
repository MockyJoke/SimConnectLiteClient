using System;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace SimConnectWebService.Clients.SimVar
{
    // Represent a request to be registered with SimConnect
    public class RecvSimobjectDataRequest<T> : IDisposable, ISimVarRequest
    {
        private SimConnectClient simConnectClient;
        private RecvSimobjectDataRequestDispatcher dispatcher;

        private TaskCompletionSource<T> taskCompletionSource;
        public RecvSimobjectDataRequest(
            SimConnectClient simConnectClient,
            RecvSimobjectDataRequestDispatcher recvSimobjectDataRequestDispatcher
        )
        {
            this.simConnectClient = simConnectClient;
            this.dispatcher = recvSimobjectDataRequestDispatcher;
        }

        // Whether this SimVarRequest has been registered with SimConnect
        public bool IsInUse { get; set; }
        public uint DefinitionId { get; set; }
        public uint RequestId { get; set; }

        public T DataValue { get; set; }
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

        public Task<T> RequestValueAsync()
        {
            if (taskCompletionSource != null && !taskCompletionSource.Task.IsCompleted)
            {
                throw new InvalidOperationException("This request is pending");
            }
            dispatcher.RegisterRequest(this);
            taskCompletionSource = new TaskCompletionSource<T>();
            IsInUse = true;
            simConnectClient.SimConnect.AddToDataDefinition(
                (RecvSimobjectDataRequestDefinitionIdEnum)DefinitionId,
                Name,
                Units,
                getSimConnectDataType(),
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simConnectClient.SimConnect.RegisterDataDefineStruct<T>((RecvSimobjectDataRequestIdEnum)DefinitionId);
            simConnectClient.SimConnect.RequestDataOnSimObject(
                    (RecvSimobjectDataRequestIdEnum)RequestId,
                    (RecvSimobjectDataRequestDefinitionIdEnum)DefinitionId,
                    ObjectId,
                    SIMCONNECT_PERIOD.ONCE, 0, 0, 0, 0);

            return taskCompletionSource.Task;
        }

        void ISimVarRequest.DeliverDataValue(SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            DataValue = (T)data.dwData[0];
            taskCompletionSource.SetResult(DataValue);
            dispatcher.UnRegisterRequest(this);
        }

        private SIMCONNECT_DATATYPE getSimConnectDataType()
        {
            Type dataType = typeof(T);
            if (dataType == typeof(int) || dataType == typeof(uint))
            {
                return SIMCONNECT_DATATYPE.INT32;
            }
            else if (dataType == typeof(long) || dataType == typeof(ulong))
            {
                return SIMCONNECT_DATATYPE.INT64;
            }
            else if (dataType == typeof(float))
            {
                return SIMCONNECT_DATATYPE.FLOAT32;
            }
            else if (dataType == typeof(double))
            {
                return SIMCONNECT_DATATYPE.FLOAT64;
            }
            else if (dataType == typeof(string))
            {
                return SIMCONNECT_DATATYPE.STRINGV;
            }
            throw new InvalidOperationException($"Cannot find the corresponding SimConnect DataType for type: {dataType.ToString()}");
        }

    }
    enum RecvSimobjectDataRequestIdEnum
    {
        RequestId
    }

    enum RecvSimobjectDataRequestDefinitionIdEnum
    {
        DefinitionId
    }
}