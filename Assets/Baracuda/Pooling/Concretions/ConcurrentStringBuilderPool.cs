// Copyright (c) 2022 Jonathan Lang
using System;
using System.Runtime.CompilerServices;
using System.Text;
using Baracuda.Pooling.Abstractions;
using Baracuda.Pooling.Interface;
using UnityEngine;

namespace Baracuda.Pooling.Concretions
{
    /// <summary>
    /// Thread safe version of a <see cref="StringBuilderPool"/>
    /// </summary>
    public static class ConcurrentStringBuilderPool
    {
        // Hack used to guarantee the initialization of the StringBuilder Pool
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
        }
        
        private static readonly ConcurrentObjectPool<StringBuilder> pool = 
            new ConcurrentObjectPool<StringBuilder>(() => new StringBuilder(100), actionOnRelease: builder => builder.Clear());

        public static StringBuilder Get()
        {
            return pool.Get();
        }
        
        public static void ReleaseStringBuilder(StringBuilder toRelease)
        {
            pool.Release(toRelease);
        }
        
        public static string Release(StringBuilder toRelease)
        {
            var str = toRelease.ToString();
            pool.Release(toRelease);
            return str;
        }
        
        public static PooledStringBuilder GetDisposable()
        {
            return new PooledStringBuilder(pool);
        }
    }
    
    public readonly struct PooledStringBuilder : IDisposable
    {
        public readonly StringBuilder Value;
        private readonly IObjectPool<StringBuilder> _pool;

        internal PooledStringBuilder(IObjectPool<StringBuilder> pool)
        {
            Value = pool.Get();
            _pool = pool;
        }

        void IDisposable.Dispose()
        {
            _pool.Release(Value);
        }
        
        public static implicit operator StringBuilder (PooledStringBuilder pooledObject)
        {
            return pooledObject.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator string(PooledStringBuilder current)
        {
            return current.ToString();
        }

        //--------------------------------------------------------------------------------------------------------------

        #region --- Append ---

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(char value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(char value, int repeatCount)
        {
            return Value.Append(value, repeatCount);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(char[] value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(char[] value, int startIndex, int charCount)
        {
            return Value.Append(value, startIndex, charCount);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(string value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(int value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(long value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(short value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(byte value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(float value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(double value)
        {
            return Value.Append(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Append(decimal value)
        {
            return Value.Append(value);
        }

        #endregion
    }
}