using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using SimConnectWebService.Clients.SimVar.Model;

namespace SimConnectWebService.Clients.SimVar
{
    class SimVarStructBuilder
    {
        private const string MODULE_NAME = "SimVarCustomStructsModule";
        private const int STRING_SIZE = 256;
        private SimVarMappingUtil mappingUtil;
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
            mappingUtil = new SimVarMappingUtil();
            structTypeBuilder = getTypeBuilder(structName);
            //ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        }

        public SimVarStructBuilder AddProperty(SimVarType simVarType, string propertyName)
        {
            defineProperty(simVarType, propertyName);
            return this;
        }
        public SimVarStructBuilder AddField(SimVarType simVarType, string fieldName)
        {
            defineField(simVarType, fieldName);
            return this;
        }
        public Type Build()
        {
            return structTypeBuilder.CreateType();
        }

        private PropertyBuilder defineProperty(SimVarType simVarType, string propertyName)
        {
            FieldBuilder fieldBuilder = defineField(simVarType, "_" + propertyName);
            Type propertyType = mappingUtil.GetStructTypeFromSimVarType(simVarType);
            PropertyBuilder propertyBuilder = structTypeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = structTypeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                structTypeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
            return propertyBuilder;
        }

        private FieldBuilder defineField(SimVarType simVarType, string fieldName)
        {
            // https://www.developerfusion.com/article/84519/mastering-structs-in-c
            Type fieldType = mappingUtil.GetStructTypeFromSimVarType(simVarType);
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
            return fieldBuilder;
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
    }
}