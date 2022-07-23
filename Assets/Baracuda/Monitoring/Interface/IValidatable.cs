namespace Baracuda.Monitoring.Interface
{
    public interface IValidatable
    {
        bool NeedsValidation { get; }
        void Validate();
    }
}