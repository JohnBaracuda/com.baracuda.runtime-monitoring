using System;
using System.Linq;
using System.Reflection;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Units
{
    public abstract class OutParameterHandle
    {
        public abstract string GetValueAsString();
        public abstract string GetValueAsString(object value);

        internal static OutParameterHandle CreateForType(Type type, IFormatData formatData)
        {
            var underlyingType = type.IsByRef ? type.GetElementType() : type;
            var concreteType = typeof(OutParameterHandle<>).MakeGenericType(underlyingType);
            var ctor = concreteType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
            return (OutParameterHandle)ctor.Invoke(new object[] {formatData});
        }
    }

    internal class OutParameterHandle<TValue> : OutParameterHandle
    {
        public override string GetValueAsString()
        {
            return _processor(default);
        }

        public override string GetValueAsString(object value)
        {
            return _processor((TValue)value);
        }

        private readonly Func<TValue, string> _processor;
        
        private OutParameterHandle(IFormatData formatData)
        {
            _processor = ValueProcessorFactory.CreateProcessorForType<TValue>(formatData);
        }

        public override string ToString()
        {
            return _processor(default);
        }
    }
}