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
        private Func<bool> CreateStaticValidatorInternal(MShowIfAttribute attribute, MemberInfo memberInfo)
        {
            return attribute.ValidationMethod == ValidationMethod.ByMember ? CreateValidatorMethod(attribute.MemberName, memberInfo.DeclaringType) : null;
        }

        private Func<bool> CreateValidatorMethod(string name, Type declaringType)
        {
            var method = declaringType.GetMethod(name, STATIC_FLAGS);
            if (method != null)
            {
                return CreateValidatorFromStaticMethod(method);
            }

            var property = declaringType.GetProperty(name, STATIC_FLAGS);
            if (property != null)
            {
                return CreateValidatorFromStaticProperty(property);
            }

            var field = declaringType.GetField(name, STATIC_FLAGS);
            if (field != null)
            {
                return CreateValidatorFromStaticField(field);
            }
            
            return null;
        }

        /*
         * Method   
         */

        private Func<bool> CreateValidatorFromStaticMethod(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();

            if (!methodInfo.IsStatic)
            {
                return null;
            }

            if (methodInfo.ReturnType != typeof(bool))
            {
                return null;
            }

            if (parameters.Length == 0)
            {
                return (Func<bool>) methodInfo.CreateDelegate(typeof(Func<bool>), null);
            }

            return null;
        }

        /*
         * Property   
         */

        private Func<bool> CreateValidatorFromStaticProperty(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.IsStatic())
            {
                return null;
            }

            if (propertyInfo.PropertyType != typeof(bool))
            {
                return null;
            }

            return (Func<bool>) propertyInfo.GetMethod.CreateDelegate(typeof(Func<bool>), null);
        }

        /*
         * Field   
         */

        private Func<bool> CreateValidatorFromStaticField(FieldInfo fieldInfo)
        {
            if (!fieldInfo.IsStatic)
            {
                return null;
            }

            if (fieldInfo.FieldType != typeof(bool))
            {
                return null;
            }

            return fieldInfo.CreateStaticGetter<bool>();
        }
    }
}