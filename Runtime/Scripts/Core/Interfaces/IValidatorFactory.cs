// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Types;
using System;
using System.Reflection;

namespace Baracuda.Monitoring.Interfaces
{
    internal interface IValidatorFactory : IMonitoringSubsystem<IValidatorFactory>
    {
        Func<bool> CreateStaticValidator(MShowIfAttribute attribute, MemberInfo memberInfo);
        Func<TTarget, bool> CreateInstanceValidator<TTarget>(MShowIfAttribute attribute, MemberInfo memberInfo);
        Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MShowIfAttribute attribute, MemberInfo memberInfo);
        ValidationEvent CreateEventValidator(MShowIfAttribute attribute, MemberInfo memberInfo);
    }
}