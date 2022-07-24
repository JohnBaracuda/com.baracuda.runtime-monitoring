using System;
using System.Reflection;
using Baracuda.Monitoring.Source.Interfaces;
using Baracuda.Monitoring.Source.Utilities;

namespace Baracuda.Monitoring.Source.Systems
{
    internal partial class ValidatorFactory : IValidatorFactory
    {
        #region --- API ---
        
        public Func<bool> CreateStaticValidator(MConditionalAttribute attribute, Type baseType)
        {
            return CreateStaticValidatorInternal(attribute, baseType);
        }

        public Func<TTarget, bool> CreateInstanceValidator<TTarget>(MConditionalAttribute attribute)
        {
            return CreateInstanceValidatorInternal<TTarget>(attribute);
        }
        
        public Func<TValue, bool> CreateStaticConditionalValidator<TValue>(MConditionalAttribute attribute, Type baseType)
        {
            return CreateStaticValidatorCondition<TValue>(attribute, baseType);
        }

        public ValidationEvent CreateEventValidator(MConditionalAttribute attribute, Type baseType)
        {
            return CreateEventValidatorInternal(attribute, baseType);
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