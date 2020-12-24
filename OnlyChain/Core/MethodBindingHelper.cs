#nullable enable

using OnlyChain.Network;
using OnlyChain.Network.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Linq.Expressions;

namespace OnlyChain.Core {
    public static class MethodBindingHelper {
        public static void Bind<TDelegate>(object client, IDictionary<string, TDelegate> handlers) where TDelegate : Delegate {
            if (handlers.IsReadOnly) throw new InvalidOperationException($"{nameof(handlers)}是只读的");

            Type? targetReturnType = null;
            Type[] targetParamTypes;
            {
                var method = typeof(TDelegate).GetMethod("Invoke")!;
                targetReturnType = method.ReturnType == typeof(Task) ? typeof(void) : null;
                if (targetReturnType is null) {
                    if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)) {
                        targetReturnType = method.ReturnType.GenericTypeArguments[0];
                    }
                }
                if (targetReturnType is null) throw new InvalidOperationException($"{typeof(TDelegate)}的返回类型必须是Task或Task<T>");
                targetParamTypes = Array.ConvertAll(method.GetParameters(), p => p.ParameterType);
            }

            Type type = client.GetType();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (method.GetCustomAttribute<CommandHandlerAttribute>() is not CommandHandlerAttribute { Command: string cmd }) continue;

                var @params = method.GetParameters();
                if (@params.Length != targetParamTypes.Length) continue;
                for (int i = 0; i < targetParamTypes.Length; i++) {
                    if (@params[i].ParameterType != targetParamTypes[i]) continue;
                }

                var paramExprs = @params.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();


                if (method.ReturnType == targetReturnType) {
                    var fromResult = typeof(Task).GetMethod("FromResult")!.MakeGenericMethod(targetReturnType);
                    var lambda = Expression.Lambda(Expression.Call(fromResult, Expression.Call(Expression.Constant(client), method, paramExprs)), paramExprs);
                    handlers.Add(cmd, (TDelegate)lambda.Compile());

                    //var handler = (Func<RemoteRequest, BDict?>)method.CreateDelegate(typeof(Func<RemoteRequest, BDict?>), client);
                    //requestHandlers.Add(attr.Command, dict => Task.FromResult(handler(dict)));
                } else if (method.ReturnType == typeof(ValueTask<BDict>)) {



                    var handler = (Func<RemoteRequest, ValueTask<BDict?>>)method.CreateDelegate(typeof(Func<RemoteRequest, ValueTask<BDict?>>), client);
                    requestHandlers.Add(attr.Command, dict => handler(dict).AsTask());
                } else if (method.ReturnType == typeof(Task<BDict>)) {
                    var handler = (Func<RemoteRequest, Task<BDict?>>)method.CreateDelegate(typeof(Func<RemoteRequest, Task<BDict?>>), client);
                    requestHandlers.Add(attr.Command, dict => handler(dict));
                } else if (method.ReturnType == typeof(void)) {
                    var handler = (Action<RemoteRequest>)method.CreateDelegate(typeof(Action<RemoteRequest>), client);
                    requestHandlers.Add(attr.Command, dict => {
                        handler(dict);
                        return Task.CompletedTask;
                    });
                } else if (method.ReturnType == typeof(ValueTask)) {
                    var handler = (Func<RemoteRequest, ValueTask>)method.CreateDelegate(typeof(Func<RemoteRequest, ValueTask>), client);
                    requestHandlers.Add(attr.Command, async dict => await handler(dict));
                } else if (method.ReturnType == typeof(Task)) {
                    var handler = (Func<RemoteRequest, Task>)method.CreateDelegate(typeof(Func<RemoteRequest, Task>), client);
                    requestHandlers.Add(attr.Command, handler);
                } else {
                    throw new TypeLoadException($"{type}的{method.Name}方法签名错误");
                }
            }
        }
    }
}
