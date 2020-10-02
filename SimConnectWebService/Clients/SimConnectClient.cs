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

    //https://www.prepar3d.com/SDKv4/sdk/simconnect_api/references/simconnect_references.html#SimObjectFunctions

    public class SimConnectClient
    {
        /// User-defined win32 event
        private const int WM_USER_SIMCONNECT = 0x0402;
        private AutoResetEvent messageSignal = new AutoResetEvent(false);
        /// SimConnect object
        private SimConnect simConnect = null;

        public SimConnect SimConnect => simConnect;
        public bool IsFSXcompatible { get; private set; }
        public bool IsConnected { get; private set; }

        public SimVarMappingUtil MappingUtil { get; private set; }
        public SimVarRequestDefinitionRegistry RequestDefinitionRegistry { get; private set; }
        public SimVarRequestDispatcher RequestDispatcher { get; private set; }
        public SimConnectClient()
        {
            MappingUtil = new SimVarMappingUtil();
        }

        public Task<bool> ConnectAsync()
        {
            Console.WriteLine("Connect");

            /// The constructor is similar to SimConnect_Open in the native API
            simConnect = new SimConnect("Simconnect - Simvar test", new IntPtr(0), WM_USER_SIMCONNECT, messageSignal, IsFSXcompatible ? (uint)1 : 0);

            TaskCompletionSource<bool> connectTaskCompletionSource = new TaskCompletionSource<bool>();
            /// Listen to connect and quit msgs
            simConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler((SimConnect sender, SIMCONNECT_RECV_OPEN data) =>
            {
                RequestDefinitionRegistry = new SimVarRequestDefinitionRegistry(this);
                RequestDispatcher = new SimVarRequestDispatcher(this);
                Console.WriteLine("SimConnect_OnRecvOpen");
                Console.WriteLine("Connected to KH");
                IsConnected = true;
                connectTaskCompletionSource.TrySetResult(true);
            });

            simConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);
            /// Listen to exceptions
            simConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

            Task.Run(() =>
            {
                while (true)
                {
                    //https://docs.microsoft.com/en-us/dotnet/api/system.threading.autoresetevent?view=netcore-3.1
                    if (messageSignal.WaitOne(1000))
                    {
                        simConnect.ReceiveMessage();
                    }
                }
            });
            return connectTaskCompletionSource.Task;
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
        }

        /// The case where the user closes game
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("SimConnect_OnRecvQuit");
            Console.WriteLine("KH has exited");

            Disconnect();
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            Console.WriteLine("SimConnect_OnRecvException: " + eException.ToString());
        }
    }
}
