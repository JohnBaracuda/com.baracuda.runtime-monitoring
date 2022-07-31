// Copyright (c) 2022 Jonathan Lang
 
using System;
using System.Reflection;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Types;

namespace Baracuda.Monitoring.Source.Systems
{
    internal partial class ValidatorFactory : IValidatorFactory
    {
        #region --- API ---
        
        public Func<bool> CreateStaticValidator(MShowIfAttribute attribute, MemberInfo memberInfo)
        {
            return CreateStaticValidatorInternal(attribute, memberInfo);
        }

        public Func<TTarget, bool> CreateInstanceValidator<TTarget>(MShowIfAttribute attribute, MemberInfo memberInfo)
        {
            return CreateInstanceValidatorInternal<TTarget>(attribute);
        }
        
        public Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MShowIfAttribute attribute, MemberInfo memberInfo)
        {
            return CreateStaticValidatorCondition<TValue>(attribute, memberInfo);
        }

        public ValidationEvent CreateEventValidator(MShowIfAttribute attribute, MemberInfo memberInfo)
        {
            return CreateEventValidatorInternal(attribute, memberInfo);
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Reflection Fields ---

        private const BindingFlags STATIC_FLAGS
            = BindingFlags.Default |
              BindingFlags.Static |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly;

        private const BindingFlags INSTANCE_FLAGS
            = BindingFlags.Default |
              BindingFlags.Public |
              BindingFlags.NonPublic |
              BindingFlags.DeclaredOnly |
              BindingFlags.Instance;

        #endregion
    }
}