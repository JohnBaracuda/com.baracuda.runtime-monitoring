using System;
using System.Collections.Generic;
using Baracuda.Pooling.Concretions;
using Baracuda.Pooling.Interfaces;
using Baracuda.Pooling.Utils;
using JetBrains.Annotations;

namespace Baracuda.Pooling.Abstractions
{
    public abstract class ObjectPool<T> : ObjectPool, IDisposable, IObjectPool<T>
    {
        #region --- [PROPERTIES] ---

        public int CountAll { get; private protected set; }

        public int CountActive => CountAll - CountInactive;
        public int CountInactive => Stack.Count;

        protected Stack<T> Stack { get; }
        protected Func<T> CreateFunc { get; }
        protected Action<T> ActionOnGet { get; }
        protected Action<T> ActionOnRelease { get; }
        protected Action<T> ActionOnDestroy { get; }
        protected int MAXSize { get; }
        protected bool CollectionCheck { get; }

#if MONITOR_POOLS
        private protected int m_accessed;
#endif

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CTOR] ---

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
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
            Stack = new Stack<T>(defaultCapacity);

            CreateFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            MAXSize = maxSize;
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

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        public abstract T Get();

        public PooledObject<T> GetDisposable()
        {
            return new PooledObject<T>(Get(), this);
        }

        public abstract void Release(T element);

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CLEAR] ---

        public abstract void Clear();

        public void Dispose()
        {
            Clear();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [TOSTRING] ---

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
#if MONITOR_POOLS
            sb.Append(" Accesses: ");
            sb.Append(m_accessed);
#endif
            sb.Append(" | Concurrent:");
            sb.Append(this is ConcurrentObjectPool<T>);
            return StringBuilderPool.Release(sb);
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, string> _typeCache = new Dictionary<Type, string>();

        private static string EvaluateType(Type type)
        {
            if (_typeCache.TryGetValue(type, out var value))
                return value;

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
                        sbArgs.AppendFormat(", {0}", arg);
                    else
                        sbArgs.Append(arg);
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

                _typeCache.Add(type, retType);
                return retType;
            }

            _typeCache.Add(type, ToTypeKeyWord(type.Name));
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

    public abstract class ObjectPool
    {
#if ENFORCETHREADSAFETY
        protected static readonly System.Threading.Thread MainThread = System.Threading.Thread.CurrentThread;
#endif

#if MONITOR_POOLS
        [MonitorValue] private static readonly List<ObjectPool> _registeredPools = new List<ObjectPool>();

        protected ObjectPool()
        {
            _registeredPools.Add(this);
        }

        ~ObjectPool()
        {
            _registeredPools.Remove(this);
        }
#endif //MONITOR_POOLS
    }
}