using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Weknow.EventSource.Backbone.CodeGeneration
{
    public class CodeGenerator
    {
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly ModuleBuilder _moduleBuilder;

        public CodeGenerator(string assemblyName)
        {
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName);
        }

        public T CreateProducerProxy<T, TBase>(params object[] arguments)
        {
            var typeBuilder = _moduleBuilder.DefineType("Producer" + typeof(T).Name.Substring(1), TypeAttributes.Public | TypeAttributes.Class, typeof(TBase), new[] { typeof(T) });
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, typeof(TBase).GetConstructors()[0].GetParameters().Select(p => p.ParameterType).ToArray());
            var cil = ctorBuilder.GetILGenerator();
            cil.Emit(OpCodes.Ldarg_0);
            cil.Emit(OpCodes.Ldarg_1);
            cil.Emit(OpCodes.Ldarg_2);
            cil.Emit(OpCodes.Call, typeof(TBase).GetConstructors()[0]);
            cil.Emit(OpCodes.Nop);
            cil.Emit(OpCodes.Nop);
            cil.Emit(OpCodes.Ret);
            foreach (var method in typeof(T).GetMethods())
            {
                var methodBuilder = typeBuilder
                    .DefineMethod(method.Name,
                                  MethodAttributes.Public |
                                  MethodAttributes.Virtual |
                                  MethodAttributes.NewSlot |
                                  MethodAttributes.HideBySig |
                                  MethodAttributes.Final,
                                  method.CallingConvention,
                                  method.ReturnType,
                                  method.GetParameters()
                                    .Select(pi => pi.ParameterType)
                                    .ToArray());
                var parameter = method.GetParameters()[0];
                typeBuilder.DefineMethodOverride(methodBuilder, method);
                var emptyField = typeof(ImmutableDictionary<string, ReadOnlyMemory<byte>>).GetField("Empty", BindingFlags.Public | BindingFlags.Static);
                var addMethod = typeof(ImmutableDictionary<string, ReadOnlyMemory<byte>>).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                var serializerField = typeof(TBase).GetField("_serializer", BindingFlags.NonPublic | BindingFlags.Instance);
                var serializeMethod = serializerField.FieldType.GetMethod("Serialize").MakeGenericMethod(parameter.ParameterType);
                var sendAsyncMethod = typeof(TBase).GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);
                var il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldsfld, emptyField);
                il.Emit(OpCodes.Ldstr, parameter.Name);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, serializerField);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt, serializeMethod);
                il.Emit(OpCodes.Callvirt, addMethod);
                il.Emit(OpCodes.Call, sendAsyncMethod);
                il.Emit(OpCodes.Ret);
            }
            var type = typeBuilder.CreateTypeInfo();
            return (T)Activator.CreateInstance(type, arguments);
        }
    }
}
