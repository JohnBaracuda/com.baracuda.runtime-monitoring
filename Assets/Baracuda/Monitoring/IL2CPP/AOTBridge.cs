#if ENABLE_IL2CPP || UNITY_EDITOR
using System.Runtime.CompilerServices;
using Baracuda.Monitoring.Internal.Profiling;
using UnityEngine.Scripting;

namespace Baracuda.Monitoring.IL2CPP
{
    public static class AOTBridge
    {
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void AOTValueTypeArray<T>() where T : unmanaged
        {
            ValueProcessorFactory.AOTValueTypeArray<T>();
        }
        
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void AOTReferenceTypeArray<T>()
        {
            ValueProcessorFactory.AOTReferenceTypeArray<T>();
        }
        
        [Preserve]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void AOTDictionary<TKey, TValue>()
        {
            ValueProcessorFactory.AOTDictionary<TKey, TValue>();
        }
    }
}
#endif
