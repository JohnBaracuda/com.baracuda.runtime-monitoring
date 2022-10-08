// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring.Utilities.Extensions;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Baracuda.Monitoring.Systems
{
    internal partial class ValidatorFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<TTarget, bool> CreateInstanceValidatorInternal<TTarget>(MShowIfAttribute attribute)
        {
            return attribute.ValidationMethod == ValidationMethod.ByMember ? CreateInstanceValidatorMethod<TTarget>(attribute.MemberName) : null;
        }

        private Func<TTarget, bool> CreateInstanceValidatorMethod<TTarget>(string name)
        {
            var targetType = typeof(TTarget);
            
            var method = targetType.GetMethod(name, INSTANCE_FLAGS);
            if (method != null)
            {
                return CreateValidatorFromInstanceMethod<TTarget>(method);
            }

            var property = targetType.GetProperty(name, INSTANCE_FLAGS);
            if (property != null)
            {
                return CreateValidatorFromInstanceProperty<TTarget>(property);
            }

            var field = targetType.GetField(name, INSTANCE_FLAGS);
            if (field != null)
            {
                return CreateValidatorFromInstanceField<TTarget>(field);
            }

            return null;
        }

        /*
         * Method   
         */

        private Func<TTarget, bool> CreateValidatorFromInstanceMethod<TTarget>(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            if (methodInfo.IsStatic)
            {
                return null;
            }

            if (methodInfo.ReturnType != typeof(bool))
            {
                return null;
            }

            if (parameters.Length == 0)
            {
                return (Func<TTarget, bool>) methodInfo.CreateDelegate(typeof(Func<TTarget, bool>));
            }

            return null;
        }

        /*
         * Property   
         */

        private Func<TTarget, bool> CreateValidatorFromInstanceProperty<TTarget>(PropertyInfo propertyInfo)
        {
            if (propertyInfo.IsStatic())
            {
                return null;
            }

            if (propertyInfo.PropertyType != typeof(bool))
            {
                return null;
            }

            return (Func<TTarget, bool>) propertyInfo.GetMethod.CreateDelegate(typeof(Func<TTarget, bool>));
        }

        /*
         * Field   
         */

        private Func<TTarget, bool> CreateValidatorFromInstanceField<TTarget>(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                return null;
            }

            if (fieldInfo.FieldType != typeof(bool))
            {
                return null;
            }

            return fieldInfo.CreateGetter<TTarget, bool>();
        }
    }
}