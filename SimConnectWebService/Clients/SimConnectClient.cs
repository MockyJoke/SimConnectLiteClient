using Microsoft.FlightSimulator.SimConnect;
using SimConnectWebService.Clients.SimVar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SimConnectWebService.Clients
{
    enum DEFINITIONS
    {
        Struct1,
    }

    enum DATA_REQUESTS
    {
        REQUEST_1,
    };

    // this is how you declare a data structure so that
    // simconnect knows how to fill it/read it.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct Struct1
    {
        // this is how you declare a fixed size string
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public String title;
        public double latitude;
        public double longitude;
        public double altitude;
    };
    public class SimConnectClient
    {
        /// User-defined win32 event
        public const int WM_USER_SIMCONNECT = 0x0402;

        /// Window handle
        //private IntPtr hWnd = new IntPtr(0);

        /// SimConnect object
        private SimConnect simConnect = null;
        private AutoResetEvent messageSignal = new AutoResetEvent(false);

        public bool IsFSXcompatible { get; private set; }
        public bool IsConnected { get; private set; }

        private SimVarRequest simVarRequest;
        public void Connect()
        {
            Console.WriteLine("Connect");

            /// The constructor is similar to SimConnect_Open in the native API
            simConnect = new SimConnect("Simconnect - Simvar test", new IntPtr(0), WM_USER_SIMCONNECT, messageSignal, IsFSXcompatible ? (uint)1 : 0);
            //IsConnected = true;

            /// Listen to connect and quit msgs
            simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
            simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

            /// Listen to exceptions
            simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);




            simVarRequest = new SimVarRequest()
            {
                Name = "Plane Latitude",
                Units = "degrees"
            };
            /// Define a data structure
            // simConnect.AddToDataDefinition(simVarRequest.DefinitionId, simVarRequest.Name, simVarRequest.Units, SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.AddToDataDefinition(DEFINITIONS.Struct1, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            simConnect.RegisterDataDefineStruct<Struct1>(DEFINITIONS.Struct1);
            /// IMPORTANT: Register it with the simconnect managed wrapper marshaller
            /// If you skip this step, you will only receive a uint in the .dwData field.
            // simConnect.RegisterDataDefineStruct<double>(simVarRequest.DefinitionId);
            /// Catch a simobject data request
            // simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
            simConnect.OnRecvSimobjectData += new SimConnect.RecvSimobjectDataEventHandler(SimConnect_OnRecvSimobjectData);
            Console.WriteLine("Registered Data Definition");

            Task.Run(() =>
            {
                while (true)
                {
                    //https://docs.microsoft.com/en-us/dotnet/api/system.threading.autoresetevent?view=netcore-3.1
                    if (messageSignal.WaitOne())
                    {
                        simConnect.ReceiveMessage();
                    }
                }
            });
        }


        public void Disconnect()
        {
            Console.WriteLine("Disconnect");


            if (simConnect != null)
            {
                /// Dispose serves the same purpose as SimConnect_Close()
                simConnect.Dispose();
                simConnect = null;
            }

            IsConnected = false;

            // Set all requests as pending
            //foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            //{
            //    oSimvarRequest.bPending = true;
            //    oSimvarRequest.bStillPending = true;
            //}
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("SimConnect_OnRecvOpen");
            Console.WriteLine("Connected to KH");

            IsConnected = true;

            // Register pending requests
            //foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            //{
            //    if (oSimvarRequest.bPending)
            //    {
            //        oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
            //        oSimvarRequest.bStillPending = oSimvarRequest.bPending;
            //    }
            //}

        }

        /// The case where the user closes game
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("SimConnect_OnRecvQuit");
            Console.WriteLine("KH has exited");

            Disconnect();

        }
        //https://www.prepar3d.com/SDKv4/sdk/simconnect_api/references/simconnect_references.html#SimObjectFunctions
        public void test()
        {
            if (simConnect != null)
            {

                // simConnect.RequestDataOnSimObjectType(simVarRequest.RequestId, simVarRequest.DefinitionId, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                simConnect.RequestDataOnSimObject(
                     DATA_REQUESTS.REQUEST_1,
                     DEFINITIONS.Struct1,
                     SimConnect.SIMCONNECT_OBJECT_ID_USER,
                     SIMCONNECT_PERIOD.SECOND, 0, 0, 5, 0);
                // simConnect.RequestDataOnSimObject(simVarRequest.RequestId,
                //     simVarRequest.DefinitionId,
                //     SimConnect.SIMCONNECT_OBJECT_ID_USER,
                //     SIMCONNECT_PERIOD.SECOND,
                //     SIMCONNECT_DATA_REQUEST_FLAG.DEFAULT,
                //     0, 10, 0);
                // Console.WriteLine("Requested Plane Latitude");
                //simConnect.ReceiveMessage();

            }
        }

        private bool RegisterToSimConnect()//SimvarRequest _oSimvarRequest)
        {
            //if (simConnect != null)
            //{
            //    /// Define a data structure
            //    simConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.sName, _oSimvarRequest.sUnits, SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
            //    /// IMPORTANT: Register it with the simconnect managed wrapper marshaller
            //    /// If you skip this step, you will only receive a uint in the .dwData field.
            //    simConnect.RegisterDataDefineStruct<double>(_oSimvarRequest.eDef);

            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return false;
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            Console.WriteLine("SimConnect_OnRecvException: " + eException.ToString());
        }

        void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            Console.WriteLine("SimConnect_OnRecvSimobjectDataBytype");

            //uint iRequest = data.dwRequestID;
            //uint iObject = data.dwObjectID;
            //if (!lObjectIDs.Contains(iObject))
            //{
            //    lObjectIDs.Add(iObject);
            //}
            //foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            //{
            //    if (iRequest == (uint)oSimvarRequest.eRequest && (!bObjectIDSelectionEnabled || iObject == m_iObjectIdRequest))
            //    {
            //        double dValue = (double)data.dwData[0];
            //        oSimvarRequest.dValue = dValue;
            //        oSimvarRequest.bPending = false;
            //        oSimvarRequest.bStillPending = false;
            //    }
            //}
        }
        void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            Console.WriteLine("SimConnect_OnRecvSimobjectData");
            double output = (double)data.dwData[0];
            Console.WriteLine($"Output value: {output}");
        }
    }
}
