// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Interface is not yet in use.
    /// </summary>
    public interface ISettableValue<in TValue> : ISettableValue
    {
        /// <summary>
        /// Interface is not yet in use.
        /// </summary>
        void SetValue(TValue value);
    }

    /// <summary>
    /// Interface is not yet in use.
    /// </summary>
    public interface ISettableValue
    {
        /// <summary>
        /// Interface is not yet in use.
        /// </summary>
        void SetValue(object value);

        /// <summary>
        /// Interface is not yet in use.
        /// </summary>
        void SetValueStruct<TStruct>(TStruct value) where TStruct : struct;
    }
}