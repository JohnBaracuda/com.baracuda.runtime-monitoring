// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Reflection
{
    public static class InstanceFactory
    {
        /*
         *  Data   
         */

        private delegate object CreateDelegate(Type type, object arg1, object arg2, object arg3);

        private static readonly ConcurrentDictionary<Tuple<Type, Type, Type, Type>, CreateDelegate> cachedFunctions =
            new ConcurrentDictionary<Tuple<Type, Type, Type, Type>, CreateDelegate>();

        /*
         *  Factory   
         */

        public static object CreateInstance(Type type)
        {
            return GenericFactory<Ignore, Ignore, Ignore>.CreateInstance(type, null, null, null);
        }

        public static object CreateInstance<TArg1>(Type type, TArg1 arg1)
        {
            return GenericFactory<TArg1, Ignore, Ignore>.CreateInstance(type, arg1, null, null);
        }

        public static object CreateInstance<TArg1, TArg2>(Type type, TArg1 arg1, TArg2 arg2)
        {
            return GenericFactory<TArg1, TArg2, Ignore>.CreateInstance(type, arg1, arg2, null);
        }

        public static object CreateInstance<TArg1, TArg2, TArg3>(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            return GenericFactory<TArg1, TArg2, TArg3>.CreateInstance(type, arg1, arg2, arg3);
        }

        public static object CreateInstance(Type type, params object[] args)
        {
            if (args == null)
            {
                return CreateInstance(type);
            }

            if (args.Length > 3 ||
                (args.Length > 0 && args[0] == null) ||
                (args.Length > 1 && args[1] == null) ||
                (args.Length > 2 && args[2] == null))
            {
                return Activator.CreateInstance(type, args);
            }

            var arg0 = args.Length > 0 ? args[0] : null;
            var arg1 = args.Length > 1 ? args[1] : null;
            var arg2 = args.Length > 2 ? args[2] : null;

            var key = Tuple.Create(
                type,
                arg0?.GetType() ?? typeof(Ignore),
                arg1?.GetType() ?? typeof(Ignore),
                arg2?.GetType() ?? typeof(Ignore));

            if (cachedFunctions.TryGetValue(key, out CreateDelegate func))
            {
                return func(type, arg0, arg1, arg2);
            }
            else
            {
                return CacheFunc(key)(type, arg0, arg1, arg2);
            }
        }

        /*
         *  Caching   
         */

        private static CreateDelegate CacheFunc(Tuple<Type, Type, Type, Type> key)
        {
            var types = new[] {key.Item1, key.Item2, key.Item3, key.Item4};

            var method = typeof(InstanceFactory)
                .GetMethods()
                .Where(m => m.Name == "CreateInstance").Single(m => m.GetParameters().Length == 4);

            var generic = method.MakeGenericMethod(key.Item2, key.Item3, key.Item4);

            var paramExpr = ConcurrentListPool<ParameterExpression>.Get();

            paramExpr.Add(Expression.Parameter(typeof(Type)));

            for (var i = 0; i < 3; i++)
            {
                paramExpr.Add(Expression.Parameter(typeof(object)));
            }

            var callParamExpr = ConcurrentListPool<Expression>.Get();
            callParamExpr.Add(paramExpr[0]);

            for (var i = 1; i < 4; i++)
            {
                callParamExpr.Add(Expression.Convert(paramExpr[i], types[i]));
            }

            var callExpr = Expression.Call(generic, callParamExpr);
            var lambdaExpr = Expression.Lambda<CreateDelegate>(callExpr, paramExpr);
            var func = lambdaExpr.Compile();
            cachedFunctions.TryAdd(key, func);

            ConcurrentListPool<ParameterExpression>.Release(paramExpr);
            ConcurrentListPool<Expression>.Release(callParamExpr);

            return func;
        }

        private static class GenericFactory<TArg1, TArg2, TArg3>
        {
            /*
             *  Data   
             */

            // ReSharper disable once MemberHidesStaticFromOuterClass
            private static readonly ConcurrentDictionary<Type, Func<TArg1, TArg2, TArg3, object>> cachedFunctions =
                new ConcurrentDictionary<Type, Func<TArg1, TArg2, TArg3, object>>();

            /*
             *  Factory   
             */

            public static object CreateInstance(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            {
                return cachedFunctions.TryGetValue(type, out Func<TArg1, TArg2, TArg3, object> func)
                    ? func(arg1, arg2, arg3)
                    : CacheFunc(type, arg1, arg2, arg3)(arg1, arg2, arg3);
            }

            private static Func<TArg1, TArg2, TArg3, object> CacheFunc(Type type, TArg1 arg1, TArg2 arg2, TArg3 arg3)
            {
                var constructorTypes = ConcurrentListPool<Type>.Get();

                if (typeof(TArg1) != typeof(Ignore))
                {
                    constructorTypes.Add(typeof(TArg1));
                }

                if (typeof(TArg2) != typeof(Ignore))
                {
                    constructorTypes.Add(typeof(TArg2));
                }

                if (typeof(TArg3) != typeof(Ignore))
                {
                    constructorTypes.Add(typeof(TArg3));
                }

                var parameters = ConcurrentListPool<ParameterExpression>.Get();

                parameters.Add(Expression.Parameter(typeof(TArg1)));
                parameters.Add(Expression.Parameter(typeof(TArg2)));
                parameters.Add(Expression.Parameter(typeof(TArg3)));

                var constructor =
                    type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        (Binder) null, constructorTypes.ToArray(), (ParameterModifier[]) null);
                var constructorParameters = parameters.Take(constructorTypes.Count).ToList();

                Debug.Assert(constructor != null, nameof(constructor) + " != null");
                var body = Expression.New(constructor, constructorParameters);

                var lambda = Expression.Lambda<Func<TArg1, TArg2, TArg3, object>>(body, parameters);
                var compiledMethod = lambda.Compile();

                cachedFunctions.TryAdd(type, compiledMethod);

                ConcurrentListPool<ParameterExpression>.Release(parameters);
                ConcurrentListPool<Type>.Release(constructorTypes);
                return compiledMethod;
            }
        }

        private class Ignore
        {
        }
    }
}