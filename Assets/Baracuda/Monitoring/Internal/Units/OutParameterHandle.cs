using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Baracuda.Monitoring.Internal.Profiling;
using Baracuda.Monitoring.Internal.Utilities;
using Baracuda.Reflection;
using UnityEngine;

namespace Baracuda.Monitoring.Internal.Units
{
    public abstract class OutParameterHandle
    {
        public abstract string GetValueAsString(object value);

        internal static OutParameterHandle CreateForType(Type type, IFormatData formatData)
        {
            var underlyingType = type.IsByRef ? type.GetElementType() : type;
            
#if ENABLE_IL2CPP
            if (underlyingType.IsReadonlyRefStruct())
            {
                return new OutParameterHandleIL2CPP(formatData);
            }
#endif
            var concreteType = typeof(OutParameterHandle<>).MakeGenericType(underlyingType);
            var ctor = concreteType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
            return (OutParameterHandle)ctor.Invoke(new object[] {formatData});
        }
    }

    //TODO: if def type 
    public class OutParameterHandleIL2CPP : OutParameterHandle
    {
        private readonly IFormatData _formatData;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        
        public override string GetValueAsString(object value)
        {
            //TODO: Apply IFormatData
            _stringBuilder.Clear();
            _stringBuilder.Append(_formatData.Label);
            _stringBuilder.Append(' ');
            _stringBuilder.Append(value);
            return _stringBuilder.ToString();
        }

        public OutParameterHandleIL2CPP(IFormatData formatData)
        {
            _formatData = formatData;
        }
    }

    public class OutParameterHandle<TValue> : OutParameterHandle
    {
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