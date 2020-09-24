using System;
using System.Reflection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    public class SimVarRequestDefinitionRegistry
    {
        private static uint internalDefinitionId = 0;
        private SimVarMappingUtil mappingUtil = new SimVarMappingUtil();
        private SimConnectClient simConnectClient;

        public SimVarRequestDefinitionRegistry(SimConnectClient simConnectClient)
        {
            this.simConnectClient = simConnectClient;
        }



        public RecvSimobjectDataRequestDefinitionIdEnum RegisterType(SimVarFieldGroup simVarFieldGroup)
        {
            RecvSimobjectDataRequestDefinitionIdEnum definitionId = (RecvSimobjectDataRequestDefinitionIdEnum)GetDefinitionId(simVarFieldGroup);
            foreach (SimVarField simVarField in simVarFieldGroup.SimVarFieldList)
            {
                simConnectClient.SimConnect.AddToDataDefinition(
                    definitionId,
                    simVarField.VarName,
                    simVarField.Units,
                    mappingUtil.GetSimConnectDataTypeFromSimVarType(simVarField.VarType),
                    0.0f,
                    SimConnect.SIMCONNECT_UNUSED);
            }
            Type structType = GetSimVarStructType(simVarFieldGroup);
            MethodInfo method = typeof(SimConnect).GetMethod(nameof(SimConnect.RegisterDataDefineStruct));
            MethodInfo registerDataDefineStruct_Method = method.MakeGenericMethod(structType);
            registerDataDefineStruct_Method.Invoke(simConnectClient.SimConnect, new object[] { definitionId });
            return definitionId;
        }

        private uint GetDefinitionId(SimVarFieldGroup simVarFieldGroup)
        {
            return internalDefinitionId++;
        }
        private Type GetSimVarStructType(SimVarFieldGroup simVarGroup)
        {
            SimVarStructBuilder structBuilder = new SimVarStructBuilder(simVarGroup.Name);
            foreach (SimVarField simVarField in simVarGroup.SimVarFieldList)
            {
                structBuilder.AddProperty(simVarField.VarType, mappingUtil.GetSimVarStructFieldNameFromVarName(simVarField.VarName));
            }
            return structBuilder.Build();
        }
    }
}