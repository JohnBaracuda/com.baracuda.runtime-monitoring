// Copyright (c) 2022 Jonathan Lang
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    /// <summary>
    /// Add this script to a GameObject and see that T is monitored.
    /// </summary>
    public class GenericsExample : MonoBehaviour
    {
        public class ValueObject<T> : MonitoredObject
        {
            [Monitor]
            public T Value { get; protected set; }
        }

        public class IntValueObject : ValueObject<int>
        {
            public IntValueObject(int value)
            {
                Value = value;
            }
        }

        private IntValueObject _valueObject;
        
        private void Awake()
        {
            _valueObject = new IntValueObject(300);
        }
    }
}
