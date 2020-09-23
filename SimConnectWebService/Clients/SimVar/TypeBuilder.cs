using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace SimConnectWebService.Clients.SimVar
{

    // https://stackoverflow.com/questions/41784393/how-to-emit-a-type-in-net-core
    public static class MyTypeBuilder
    {
        public static Type CreateNewType(string name, Type dataType)
        {
            return CompileResultType(name, dataType);
            //var myObject = Activator.CreateInstance(myType);
        }
        public static Type CompileResultType(string name, Type dataType)
        {
            TypeBuilder tb = GetValueTypeBuilder(name);
            ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
            //foreach (var field in yourListOfFields){
            //CreateProperty(tb, field.FieldName, field.FieldType);
            CreateField(tb, "innerDataVal", dataType);
            //}

            Type objectType = tb.CreateType();
            return objectType;
        }

        private static TypeBuilder GetTypeBuilder(string name)
        {
            var typeSignature = name;
            var an = new AssemblyName(typeSignature);

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run); //AppDomain.CurrentDomain..DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);
            return tb;
        }

        //https://gist.github.com/ChadSki/7992383
        private static TypeBuilder GetValueTypeBuilder(string name)
        {
            var typeSignature = name;
            var an = new AssemblyName(typeSignature);

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run); //AppDomain.CurrentDomain..DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                TypeAttributes.Public |
                TypeAttributes.Sealed |
                TypeAttributes.SequentialLayout |
                TypeAttributes.Serializable |
                TypeAttributes.AnsiClass,
                typeof(ValueType),
                PackingSize.Size1);
            return tb;
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
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
        }
        private static void CreateField(TypeBuilder tb, string fieldName, Type fieldType)
        {
            FieldBuilder fieldBuilder = tb.DefineField(fieldName, fieldType, FieldAttributes.Public);


            // Find MarshalAsAttribute's constructor by signature, then invoke
            var ctorParameters = new Type[] { typeof(UnmanagedType) };
            var ctorInfo = typeof(MarshalAsAttribute).GetConstructor(ctorParameters);

            var fields = typeof(MarshalAsAttribute).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var sizeConst = (from f in fields
                             where f.Name == "SizeConst"
                             select f).ToArray();
            var marshalAsAttr = new CustomAttributeBuilder(ctorInfo,
                new object[] { UnmanagedType.ByValTStr }, sizeConst, new object[] { 256 });

            // https://www.developerfusion.com/article/84519/mastering-structs-in-c

            fieldBuilder.SetOffset(0);
            fieldBuilder.SetCustomAttribute(marshalAsAttr);
        }
    }
}