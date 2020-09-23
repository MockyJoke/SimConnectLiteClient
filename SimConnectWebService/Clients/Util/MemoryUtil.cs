using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SimConnectWebService.Util
{
    public class MemoryUtil
    {
        public string GetMemoryLayoutString(Type type)
        {
            string output = "Beginning of Layout.";
            output += $"Total size of type: {type.ToString()} is : {Marshal.SizeOf(type)} bytes \n";
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                output += $"Field name: {fieldInfo.Name}, offset: {Marshal.OffsetOf(type, fieldInfo.Name)} bytes \n";
            }
            output += $"End of Layout. \n";
            return output;

        }
    }
}