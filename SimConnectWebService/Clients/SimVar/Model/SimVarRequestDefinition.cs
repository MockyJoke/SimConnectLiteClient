using System;

namespace SimConnectWebService.Clients.SimVar.Model
{
    public class SimVarRequestDefinition
    {
        public uint DefinitionId { get; set; }
        public SimVarFieldGroup FieldGroup { get; set; }
        public Type FieldGroupStructType { get; set; }
    }
}