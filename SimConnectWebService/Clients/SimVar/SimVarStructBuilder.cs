using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SimConnectWebService.Clients.SimVar
{
    class SimVarStructBuilder
    {
        private const string MODULE_NAME = "SimVarCustomStructsModule";
        private const int STRING_SIZE = 256;
        private TypeBuilder structTypeBuilder;
        public SimVarStructBuilder() :
            this(null)
        {
        }
        public SimVarStructBuilder(string structName)
        {
            if (string.IsNullOrEmpty(structName))
            {
                structName = "SomeName";
            }

            structTypeBuilder = getTypeBuilder(structName);
            //ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        }
        public SimVarStructBuilder AddField(SimVarType simVarType, string fieldName)
        {
            // https://www.developerfusion.com/article/84519/mastering-structs-in-c
            Type fieldType = getStructType(simVarType);
            FieldBuilder fieldBuilder = structTypeBuilder.DefineField(fieldName, fieldType, FieldAttributes.Public);
            if (simVarType == SimVarType.STRING)
            {
                // Need to mark string as unmanaged fixed length string
                // Find MarshalAsAttribute's constructor by signature, then invoke
                var ctorParameters = new Type[] { typeof(UnmanagedType) };
                var ctorInfo = typeof(MarshalAsAttribute).GetConstructor(ctorParameters);
                var fields = typeof(MarshalAsAttribute).GetFields(BindingFlags.Public | BindingFlags.Instance);
                var sizeConst = (from f in fields
                                 where f.Name == "SizeConst"
                                 select f).ToArray();
                var marshalAsAttr = new CustomAttributeBuilder(ctorInfo,
                    new object[] { UnmanagedType.ByValTStr }, sizeConst, new object[] { STRING_SIZE });
                fieldBuilder.SetCustomAttribute(marshalAsAttr);
            }
            return this;
        }
        public Type Build()
        {
            return structTypeBuilder.CreateType();
        }

        private TypeBuilder getTypeBuilder(string typeName)
        {
            string typeSignature = typeName;
            AssemblyName assemblyName = new AssemblyName(typeSignature);

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run); //AppDomain.CurrentDomain..DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(MODULE_NAME);
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeSignature,
                TypeAttributes.Public |
                TypeAttributes.Sealed |
                TypeAttributes.SequentialLayout |
                TypeAttributes.Serializable |
                TypeAttributes.AnsiClass,
                typeof(ValueType),
                PackingSize.Size1);
            return typeBuilder;
        }

        private Type getStructType(SimVarType simVarType)
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