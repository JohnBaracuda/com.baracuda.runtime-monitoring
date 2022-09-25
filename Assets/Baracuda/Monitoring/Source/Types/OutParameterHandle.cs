// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.API;
using System;
using System.Linq;
using System.Reflection;
#if ENABLE_IL2CPP
using Baracuda.Utilities.Extensions;
#endif

namespace Baracuda.Monitoring.Types
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
                return new OutParameterHandleRefStruct(type, formatData);
            }
#endif
            var concreteType = typeof(OutParameterHandleT<>).MakeGenericType(underlyingType);
            var ctor = concreteType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First();
            return (OutParameterHandle)ctor.Invoke(new object[] {formatData});
        }
    }
}