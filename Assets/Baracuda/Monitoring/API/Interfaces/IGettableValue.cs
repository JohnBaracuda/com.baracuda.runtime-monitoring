// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring
{
    public interface IGettableValue<out TValue> : IGettableValue
    {
        TValue GetValue();
    }

    public interface IGettableValue
    {
        TValue GetValueAs<TValue>();
        object GetValueAsObject();
    }
}
