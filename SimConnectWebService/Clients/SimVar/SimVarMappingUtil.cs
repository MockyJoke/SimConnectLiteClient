using System;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    public class SimVarMappingUtil
    {
        public SimVarType GetSimVarTypeFromStringType(string varType)
        {
            varType = varType.Trim().ToUpper();
            return Enum.Parse<SimVarType>(varType);
        }

        public string GetSimVarStructFieldNameFromVarName(string varName)
        {
            return varName.ToLower().Replace(' ', '-');
        }

        public SIMCONNECT_DATATYPE GetSimConnectDataTypeFromSimVarType(SimVarType simVarType)
        {
            switch (simVarType)
            {
                case SimVarType.BOOLEAN:
                    return SIMCONNECT_DATATYPE.INT64;
                case SimVarType.FLOAT:
                    return SIMCONNECT_DATATYPE.FLOAT64;
                case SimVarType.INTEGER:
                    return SIMCONNECT_DATATYPE.INT64;
                case SimVarType.STRING:
                    return SIMCONNECT_DATATYPE.STRING256;
                default:
                    return SIMCONNECT_DATATYPE.FLOAT64;

            }
        }

        public Type GetStructTypeFromSimVarType(SimVarType simVarType)
        {
            switch (simVarType)
            {
                case SimVarType.BOOLEAN:
                    return typeof(bool);
                case SimVarType.FLOAT:
                    return typeof(double);
                case SimVarType.INTEGER:
                    return typeof(long);
                case SimVarType.STRING:
                    return typeof(string);
                default:
                    return typeof(double);
            }
        }
    }
}