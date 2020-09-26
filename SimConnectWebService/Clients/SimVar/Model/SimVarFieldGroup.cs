using System;
using System.Collections.Generic;

namespace SimConnectWebService.Clients.SimVar.Model
{
    public class SimVarFieldGroup
    {
        private List<SimVarField> simVarFieldList;
        public IReadOnlyList<SimVarField> SimVarFieldList
        {
            get
            {
                return simVarFieldList;
            }
        }
        public string Name { get; private set; }
        public SimVarTargetObject TargetObject { get; private set; } = SimVarTargetObject.USER;

        public SimVarFieldGroup() :
            this(null)
        {
        }
        public SimVarFieldGroup(string name) :
            this(name, SimVarTargetObject.USER)
        {
        }

        public SimVarFieldGroup(string name, SimVarTargetObject targetObject)
        {
            simVarFieldList = new List<SimVarField>();
            Name = name;
            TargetObject = targetObject;
        }

        public void AddSimVarField(SimVarField simVar)
        {
            if (simVarFieldList.Contains(simVar))
            {
                throw new InvalidOperationException($"SimVar Group already contain SimVar: {simVar.ToString()}");
            }
            simVarFieldList.Add(simVar);
        }
        public void DeleteSimVarField(SimVarField simVar)
        {
            if (!simVarFieldList.Contains(simVar))
            {
                throw new InvalidOperationException($"SimVar Group does not contain SimVar: {simVar.ToString()}");
            }
            simVarFieldList.Remove(simVar);
        }
    }
}