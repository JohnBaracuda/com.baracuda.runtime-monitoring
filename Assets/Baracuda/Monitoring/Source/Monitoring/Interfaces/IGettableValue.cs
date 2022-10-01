// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Interface to get the value of a monitored field, property or method.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IGettableValue<out TValue> : IGettableValue
    {
        /// <summary>
        /// Get the value of a monitored field, property or method.
        /// </summary>
        TValue GetValue();
    }

    /// <summary>
    /// Interface to get the value of a monitored field, property or method.
    /// </summary>
    public interface IGettableValue
    {
        /// <summary>
        /// Get the value of a monitored field, property or method.
        /// </summary>
        TValue GetValueAs<TValue>();

        /// <summary>
        /// Get the value of a monitored field, property or method.
        /// </summary>
        object GetValueAsObject();
    }
}
