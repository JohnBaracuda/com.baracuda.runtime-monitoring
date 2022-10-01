
using Baracuda.Monitoring;

public struct TestStruct
{
    public TestStruct(int value)
    {
        Value = value;
    }

    [Monitor]
    public int Value { get; }
}
