using Baracuda.Monitoring;

namespace Baracuda
{
    public struct TestStruct
    {
        [Monitor]
        public static int Value { get; set; } = 1337;
    }
}
