// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Attributes;
using Baracuda.Monitoring.Core.Types;
using Baracuda.Monitoring.Interfaces;
using System;
using System.Reflection;

namespace Baracuda.Monitoring.Core.Interfaces
{
    internal interface IValidatorFactory : IMonitoringSubsystem<IValidatorFactory>
    {
        Func<bool> CreateStaticValidator(MShowIfAttribute attribute, MemberInfo memberInfo);
        Func<TTarget, bool> CreateInstanceValidator<TTarget>(MShowIfAttribute attribute, MemberInfo memberInfo);
        Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MShowIfAttribute attribute, MemberInfo memberInfo);
        ValidationEvent CreateEventValidator(MShowIfAttribute attribute, MemberInfo memberInfo);
    }
}