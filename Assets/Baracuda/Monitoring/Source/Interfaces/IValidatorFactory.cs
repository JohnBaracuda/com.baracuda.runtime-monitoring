// Copyright (c) 2022 Jonathan Lang
 
using System;
using System.Reflection;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Utilities;

namespace Baracuda.Monitoring.Source.Interfaces
{
    public interface IValidatorFactory : IMonitoringSystem<IValidatorFactory>
    {
        Func<bool> CreateStaticValidator(MConditionalAttribute attribute, MemberInfo memberInfo);
        Func<TTarget, bool> CreateInstanceValidator<TTarget>(MConditionalAttribute attribute, MemberInfo memberInfo);
        Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MConditionalAttribute attribute, MemberInfo memberInfo);
        ValidationEvent CreateEventValidator(MConditionalAttribute attribute, MemberInfo memberInfo);
    }
}