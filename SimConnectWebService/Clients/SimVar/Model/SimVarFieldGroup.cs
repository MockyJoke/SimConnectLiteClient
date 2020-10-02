using System;
using System.Collections.Generic;
using System.Linq;

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
        public uint TargetObjectId { get; private set; }

        public SimVarFieldGroup() :
            this(null)
        {
        }
        public SimVarFieldGroup(string name) :
            this(name, 0)
        {
        }

        public SimVarFieldGroup(string name, uint targetObject)
        {
            simVarFieldList = new List<SimVarField>();
            Name = name;
            TargetObjectId = targetObject;
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

        public override bool Equals(object obj)
        {
            return obj is SimVarFieldGroup group &&
                   Enumerable.SequenceEqual(simVarFieldList, group.simVarFieldList) &&
                   Name == group.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = Name.GetHashCode();
            foreach (SimVarField simVarField in simVarFieldList)
            {
                hashCode ^= simVarField.GetHashCode();
            }
            return hashCode;
        }
    }
}