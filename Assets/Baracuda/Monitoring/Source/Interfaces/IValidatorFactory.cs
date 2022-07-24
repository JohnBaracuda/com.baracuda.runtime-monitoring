using System;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Utilities;

namespace Baracuda.Monitoring.Source.Interfaces
{
    public interface IValidatorFactory : IMonitoringSystem<IValidatorFactory>
    {
        Func<bool> CreateStaticValidator(MConditionalAttribute attribute, Type baseType);
        Func<TTarget, bool> CreateInstanceValidator<TTarget>(MConditionalAttribute attribute);
        Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MConditionalAttribute attribute, Type baseType);
        ValidationEvent CreateEventValidator(MConditionalAttribute attribute, Type baseType);
    }
}