using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.FlightSimulator.SimConnect;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    public class SimVarRequestDefinitionRegistry
    {
        private static uint internalDefinitionId = 0;
        private ConcurrentDictionary<SimVarFieldGroup, SimVarRequestDefinition> simVarRequestDefinitionCache;
        private SimConnectClient simConnectClient;

        public SimVarRequestDefinitionRegistry(SimConnectClient simConnectClient)
        {
            this.simConnectClient = simConnectClient;
            simVarRequestDefinitionCache = new ConcurrentDictionary<SimVarFieldGroup, SimVarRequestDefinition>();
        }

        public SimVarRequestDefinition RegisterType(SimVarFieldGroup simVarFieldGroup)
        {
            SimVarRequestDefinition result;
            if (simVarRequestDefinitionCache.TryGetValue(simVarFieldGroup, out result))
            {
                return result;
            }
            result = DoRegisterType(simVarFieldGroup);
            simVarRequestDefinitionCache.TryAdd(simVarFieldGroup, result);
            return result;
        }

        private SimVarRequestDefinition DoRegisterType(SimVarFieldGroup simVarFieldGroup)
        {
            uint definitionId = GetDefinitionId(simVarFieldGroup);
            foreach (SimVarField simVarField in simVarFieldGroup.SimVarFieldList)
            {
                simConnectClient.SimConnect.AddToDataDefinition(
                    (SimObjectDataRequestDefinitionIdEnum)definitionId,
                    simVarField.VarName,
                    simVarField.Units,
                    simConnectClient.MappingUtil.GetSimConnectDataTypeFromSimVarType(simVarField.VarType),
                    0.0f,
                    SimConnect.SIMCONNECT_UNUSED);
            }
            Type structType = GetSimVarStructType(simVarFieldGroup);
            MethodInfo method = typeof(SimConnect).GetMethod(nameof(SimConnect.RegisterDataDefineStruct));
            MethodInfo registerDataDefineStruct_Method = method.MakeGenericMethod(structType);
            registerDataDefineStruct_Method.Invoke(simConnectClient.SimConnect, new object[] { (SimObjectDataRequestDefinitionIdEnum)definitionId });

            return new SimVarRequestDefinition()
            {
                DefinitionId = definitionId,
                FieldGroup = simVarFieldGroup,
                FieldGroupStructType = structType
            };
        }

        private uint GetDefinitionId(SimVarFieldGroup simVarFieldGroup)
        {
            return ++internalDefinitionId;
        }

        private Type GetSimVarStructType(SimVarFieldGroup simVarGroup)
        {
            SimVarStructBuilder structBuilder = new SimVarStructBuilder(simVarGroup.Name);
            foreach (SimVarField simVarField in simVarGroup.SimVarFieldList)
            {
                structBuilder.AddProperty(simVarField.VarType, simConnectClient.MappingUtil.GetSimVarStructFieldNameFromVarName(simVarField.VarName));
            }
            return structBuilder.Build();
        }
    }
}