using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimConnectWebService.Clients
{
    public class SimConnectClient
    {
        /// User-defined win32 event
        public const int WM_USER_SIMCONNECT = 0x0402;

        /// Window handle
        //private IntPtr hWnd = new IntPtr(0);

        /// SimConnect object
        private SimConnect simConnect = null;

        public bool IsFSXcompatible { get; private set; }
        public bool IsConnected { get; private set; }


        public void Connect()
        {
            Console.WriteLine("Connect");

            /// The constructor is similar to SimConnect_Open in the native API
            simConnect = new SimConnect("Simconnect - Simvar test", new IntPtr(0), WM_USER_SIMCONNECT, null, IsFSXcompatible ? (uint)1 : 0);

            IsConnected = true;

            /// Listen to connect and quit msgs
            simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
            simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

            /// Listen to exceptions
            simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

            /// Catch a simobject data request
            simConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);

            simConnect.OnRecvSimobjectData += SimConnect_OnRecvSimobjectData;

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

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            //Console.WriteLine("SimConnect_OnRecvSimobjectDataBytype");

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
        private void SimConnect_OnRecvSimobjectData(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA data)
        {
            throw new NotImplementedException();
        }
    }
}
