// Copyright (c) 2022 Jonathan Lang

using System;
using System.Reflection;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring.Source.Types;

namespace Baracuda.Monitoring.Source.Interfaces
{
    public interface IValidatorFactory : IMonitoringSubsystem<IValidatorFactory>
    {
        Func<bool> CreateStaticValidator(MShowIfAttribute attribute, MemberInfo memberInfo);
        Func<TTarget, bool> CreateInstanceValidator<TTarget>(MShowIfAttribute attribute, MemberInfo memberInfo);
        Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MShowIfAttribute attribute, MemberInfo memberInfo);
        ValidationEvent CreateEventValidator(MShowIfAttribute attribute, MemberInfo memberInfo);
    }
}