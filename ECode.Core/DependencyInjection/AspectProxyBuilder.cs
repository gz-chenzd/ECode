using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ECode.DependencyInjection
{
    public interface IInvokeBeforeAdvice
    {
        void BeforeInvoke(object target, MethodInfo method, object[] args);
    }

    public interface IAfterReturningAdvice
    {
        void AfterReturning(object target, MethodInfo method, object[] args, object returnValue);
    }

    public interface IThrowsAdvice
    {
        void AfterThrowing(Exception exception);
    }

    public static class AspectProxyHandler
    {
        public static void BeforeInvoke(object target, MethodInfo method, object[] args)
        {

        }

        public static void AfterReturning(object target, MethodInfo method, object[] args, object returnValue)
        {

        }

        public static void AfterThrowing(Exception exception)
        {

        }
    }

    public static class AspectProxyBuilder
    {
        static AssemblyBuilder      assemblyBuilder     = null;
        static ModuleBuilder        moduleBuilder       = null;


        static AspectProxyBuilder()
        {
            var assName = new AssemblyName();
            assName.Name = $"DynamicAssembly_{RandomString()}";

            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assName, AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(assName.Name);
        }


        static string RandomString()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }


        static void EmitRefParameterType(ILGenerator il, Type type)
        {
            if (type.IsValueType)
            {
                if (type == typeof(Boolean))
                {
                    il.Emit(OpCodes.Ldind_U1);
                }
                else if (type == typeof(Byte))
                {
                    il.Emit(OpCodes.Ldind_U1);
                }
                else if (type == typeof(SByte))
                {
                    il.Emit(OpCodes.Ldind_I1);
                }
                else if (type == typeof(Char))
                {
                    il.Emit(OpCodes.Ldind_U2);
                }
                else if (type == typeof(Int16))
                {
                    il.Emit(OpCodes.Ldind_I2);
                }
                else if (type == typeof(UInt16))
                {
                    il.Emit(OpCodes.Ldind_U2);
                }
                else if (type == typeof(Int32))
                {
                    il.Emit(OpCodes.Ldind_I4);
                }
                else if (type == typeof(UInt32))
                {
                    il.Emit(OpCodes.Ldind_U4);
                }
                else if (type == typeof(Int64))
                {
                    il.Emit(OpCodes.Ldind_I8);
                }
                else if (type == typeof(UInt64))
                {
                    il.Emit(OpCodes.Ldind_I8);
                }
                else if (type == typeof(Single))
                {
                    il.Emit(OpCodes.Ldind_R4);
                }
                else if (type == typeof(Double))
                {
                    il.Emit(OpCodes.Ldind_R8);
                }
                else if (type == typeof(Decimal))
                {
                    il.Emit(OpCodes.Ldobj, typeof(Decimal));
                }
                else if (type == typeof(Enum))
                {
                    il.Emit(OpCodes.Ldind_I4);
                }
                else if (type == typeof(Guid))
                {
                    il.Emit(OpCodes.Ldobj, typeof(DateTime));
                }
                else if (type == typeof(DateTime))
                {
                    il.Emit(OpCodes.Ldobj, typeof(DateTime));
                }
                else if (type == typeof(TimeSpan))
                {
                    il.Emit(OpCodes.Ldobj, typeof(TimeSpan));
                }
                else
                {
                    il.Emit(OpCodes.Ldobj, type);
                }

                il.Emit(OpCodes.Box, type);
            }
            else
            {
                il.Emit(OpCodes.Ldind_Ref);
            }
        }


        public static Type CreateType(Type type, ConstructorInfo constructor, MethodInfo[] methods)
        {
            var typeBuilder = moduleBuilder.DefineType($"{type.Name}_{RandomString()}", TypeAttributes.Public, type);

            if (constructor != null)
            {
                BuildConstructor(typeBuilder, constructor);
            }

            if (methods != null && methods.Length > 0)
            {
                foreach (MethodInfo method in methods)
                {
                    BuildMethod(typeBuilder, method);
                }
            }

            return typeBuilder.CreateType();
        }

        private static void BuildConstructor(TypeBuilder typeBuilder, ConstructorInfo constructor)
        {
            var parameters = constructor.GetParameters();
            var parameterTypes = parameters.Select(u => u.ParameterType).ToArray();
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);

            for (int i = 0; i < parameters.Length; i++)
            {
                ctorBuilder.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
            }


            var il = ctorBuilder.GetILGenerator();

            for (int i = 0; i <= parameterTypes.Length; ++i)
            {
                il.Emit(OpCodes.Ldarg, i);
            }

            il.Emit(OpCodes.Call, constructor);
            il.Emit(OpCodes.Ret);
        }

        private static void BuildMethod(TypeBuilder typeBuilder, MethodInfo method)
        {
            var parameters = method.GetParameters();
            var parameterTypes = parameters.Select(u => u.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(method.Name, method.Attributes, method.ReturnType, parameterTypes);

            for (int i = 0; i < parameters.Length; i++)
            {
                methodBuilder.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
            }


            var il = methodBuilder.GetILGenerator();
            var proxyMethod = il.DeclareLocal(typeof(MethodInfo));
            var methodArgs = il.DeclareLocal(typeof(object[]));
            var executeError = il.DeclareLocal(typeof(Exception));
            if (method.ReturnType != typeof(void))
            {
                var returnValue = il.DeclareLocal(method.ReturnType);
            }


            il.BeginExceptionBlock();

            // init proxy args: proxyMethod
            il.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod"));
            il.Emit(OpCodes.Stloc_0);


            // init proxy method's out parameters
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsOut)
                {
                    il.Emit(OpCodes.Ldarg, i + 1);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stind_Ref);
                }
            }


            // init proxy args: methodArgs
            il.Emit(OpCodes.Ldc_I4, parameters.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc_1);


            // exec proxy handler before method executing
            for (int i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i + 1);

                if (parameters[i].ParameterType.IsByRef)
                {
                    string paramTypeStr = parameters[i].ParameterType.FullName.TrimEnd('&');
                    Type oriParamType =  Type.GetType(paramTypeStr);

                    EmitRefParameterType(il, oriParamType);
                }
                else if (parameters[i].ParameterType.IsValueType)
                {
                    il.Emit(OpCodes.Box, parameters[i].ParameterType);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldarg_0);  // target
            il.Emit(OpCodes.Ldloc_0);  // method
            il.Emit(OpCodes.Ldloc_1);  // args
            il.Emit(OpCodes.Call, typeof(AspectProxyHandler).GetMethod("BeforeExecuting"));


            // exec proxy method
            for (int i = 0; i <= parameters.Length; ++i)
            {
                il.Emit(OpCodes.Ldarg, i);
            }

            il.Emit(OpCodes.Call, method);
            if (method.ReturnType != typeof(void))
            {
                // init proxy args: returnValue
                il.Emit(OpCodes.Stloc_3);
            }


            // exec proxy handler after method returning
            for (int i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i + 1);

                if (parameters[i].ParameterType.IsByRef)
                {
                    string paramTypeStr = parameters[i].ParameterType.FullName.TrimEnd('&');
                    Type oriParamType =  Type.GetType(paramTypeStr);

                    EmitRefParameterType(il, oriParamType);
                }
                else if (parameters[i].ParameterType.IsValueType)
                {
                    il.Emit(OpCodes.Box, parameters[i].ParameterType);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldarg_0);  // target
            il.Emit(OpCodes.Ldloc_0);  // method
            il.Emit(OpCodes.Ldloc_1);  // args
            if (method.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Ldloc_3);  // returnValue
            }
            else
            {
                il.Emit(OpCodes.Ldnull);  // returnValue
            }

            il.Emit(OpCodes.Call, typeof(AspectProxyHandler).GetMethod("AfterReturning"));


            il.BeginCatchBlock(typeof(Exception));

            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Call, typeof(AspectProxyHandler).GetMethod("AfterThrowing"));
            il.Emit(OpCodes.Rethrow);

            il.EndExceptionBlock();


            if (method.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Ldloc_3);
            }
            il.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(methodBuilder, method);
        }
    }
}