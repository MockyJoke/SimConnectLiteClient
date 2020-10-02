using System;

namespace SimConnectWebService.Clients.SimVar.Model
{
    public class SimVarField
    {
        public string VarName { get; set; }
        public string Units { get; set; }
        public SimVarType VarType { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SimVarField field &&
                   VarName == field.VarName &&
                   Units == field.Units &&
                   VarType == field.VarType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VarName, Units, VarType);
        }
    }
}