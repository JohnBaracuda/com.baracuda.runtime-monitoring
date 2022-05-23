// Copyright (c) 2022 Jonathan Lang
using System;
using System.Collections.Generic;
using Baracuda.Pooling.Concretions;
using Baracuda.Pooling.Interface;
using Baracuda.Pooling.Utils;
using JetBrains.Annotations;

namespace Baracuda.Pooling.Abstractions
{
    public abstract class ObjectPool<T> : ObjectPool, IObjectPool<T>
    {
        /*
         *  Properties   
         */

        public int CountAll { get; private protected set; }

        public int CountActive => CountAll - CountInactive;
        public int CountInactive => Stack.Count;

        protected Stack<T> Stack { get; }
        protected Func<T> CreateFunc { get; }
        protected Action<T> ActionOnGet { get; }
        protected Action<T> ActionOnRelease { get; }
        protected Action<T> ActionOnDestroy { get; }
        protected int MaxSize { get; }
        protected bool CollectionCheck { get; }

        /*
         *  Ctor   
         */

        public ObjectPool(
            [NotNull] Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 1,
            int maxSize = 10000)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            }

            Stack = new Stack<T>(defaultCapacity);

            CreateFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            MaxSize = maxSize;
            ActionOnGet = actionOnGet;
            ActionOnRelease = actionOnRelease;
            ActionOnDestroy = actionOnDestroy;
            CollectionCheck = collectionCheck;

            for (var i = 0; i < defaultCapacity; i++)
            {
                Stack.Push(CreateFunc());
                ++CountAll;
            }
        }

        /*
         *  Abstract   
         */

        public abstract T Get();

        public abstract void Release(T element);

        /*
         *  IDisposable   
         */
        
        public PooledObject<T> GetDisposable()
        {
            return new PooledObject<T>(Get(), this);
        }
        
        #region --- ToString ---

        public override string ToString()
        {
            var sb = StringBuilderPool.Get();

            sb.Append("Pool<");
            sb.Append(EvaluateType(typeof(T)));
            sb.Append("> | Inactive: ");
            if (CountInactive >= CountAll)
            {
                sb.Append("<color=#cyan>");
            }

            sb.Append(CountInactive);
            sb.Append('/');
            sb.Append(CountAll);
            if (CountInactive >= CountAll)
            {
                sb.Append("</color>");
            }
            sb.Append(" | Concurrent:");
            sb.Append(this is ConcurrentObjectPool<T>);
            return StringBuilderPool.Release(sb);
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, string> typeCache = new Dictionary<Type, string>();

        private static string EvaluateType(Type type)
        {
            if (typeCache.TryGetValue(type, out var value))
            {
                return value;
            }

            if (type.IsGenericType)
            {
                var sb = StringBuilderPool.Get();
                var sbArgs = StringBuilderPool.Get();

                var arguments = type.GetGenericArguments();

                foreach (var t in arguments)
                {
                    // Let's make sure we get the argument list.
                    var arg = EvaluateType(t);

                    if (sbArgs.Length > 0)
                    {
                        sbArgs.AppendFormat(", {0}", arg);
                    }
                    else
                    {
                        sbArgs.Append(arg);
                    }
                }

                if (sbArgs.Length > 0)
                {
                    sb.AppendFormat("{0}<{1}>", type.Name.Split('`')[0], StringBuilderPool.Release(sbArgs));
                }
                else
                {
                    StringBuilderPool.ReleaseStringBuilder(sbArgs);
                }
                
                var retType = StringBuilderPool.Release(sb);

                typeCache.Add(type, retType);
                return retType;
            }

            typeCache.Add(type, ToTypeKeyWord(type.Name));
            return type.Name;
        }

        private static string ToTypeKeyWord(string typeName)
        {
            switch (typeName)
            {
                case "String":
                    return "string";
                case "Int32":
                    return "int";
                case "Single":
                    return "float";
                case "Boolean":
                    return "bool";
                case "Byte":
                    return "byte";
                case "SByte":
                    return "sbyte";
                case "Char":
                    return "char";
                case "Decimal":
                    return "decimal";
                case "Double":
                    return "double";
                case "UInt32":
                    return "uint";
                case "Int64":
                    return "long";
                case "UInt64":
                    return "ulong";
                case "Int16":
                    return "short";
                case "UInt16":
                    return "ushort";
                case "Object":
                    return "object";
                default:
                    return typeName;
            }
        }

        #endregion
    }

    public abstract class ObjectPool : IDisposable
    {
        public static IReadOnlyCollection<ObjectPool> ActivePools => activePools;

        private static readonly HashSet<ObjectPool> activePools = new HashSet<ObjectPool>();

        protected ObjectPool()
        {
            activePools.Add(this);
        }
        
        public abstract void Clear();
        
#if ENFORCETHREADSAFETY
        protected static System.Threading.Thread MainThread { get; } = System.Threading.Thread.CurrentThread;
#endif
        private void ReleaseUnmanagedResources()
        {
            Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ObjectPool()
        {
            Dispose(false);
        }
    }
}