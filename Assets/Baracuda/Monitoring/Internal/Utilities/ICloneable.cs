namespace Baracuda.Monitoring.Internal.Utilities
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}