using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using Bucket = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

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
                var parameters = method.GetParameters();
                typeBuilder.DefineMethodOverride(methodBuilder, method);
                var classifyAsyncMethod = typeof(TBase).GetMethod("CreateClassificationAdaptor", BindingFlags.NonPublic | BindingFlags.Instance);
                var returnType = classifyAsyncMethod.ReturnType;
                var sendAsyncMethod = typeof(TBase).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(m => m.Name == "SendAsync" && m.GetParameters().Length == 2);
                var il = methodBuilder.GetILGenerator();
                var arr = il.DeclareLocal(returnType.MakeArrayType());
                il.Emit(OpCodes.Nop);

                il.Emit(OpCodes.Ldc_I4, parameters.Length);
                il.Emit(OpCodes.Newarr, returnType);
                il.Emit(OpCodes.Stloc_0);

                // classify each parameter
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, method.Name);
                    il.Emit(OpCodes.Ldstr, parameter.Name);
                    il.Emit(OpCodes.Ldarg, i + 1);
                    il.Emit(OpCodes.Call, classifyAsyncMethod.MakeGenericMethod(parameter.ParameterType));
                    il.Emit(OpCodes.Stelem, returnType);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, method.Name);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Call, sendAsyncMethod);
                il.Emit(OpCodes.Ret);
            }
            var type = typeBuilder.CreateTypeInfo();
            return (T)Activator.CreateInstance(type, arguments);
        }
    }
}
