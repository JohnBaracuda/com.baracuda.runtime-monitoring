// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// The monitored member will only be displayed if the given value of the passed member returns true.<br/>
    /// Methods can accept the members current value as a parameter and use custom logic based on the members current
    /// state to determine if the member should be displayed or not.<br/>
    /// Passed events must be of type <see cref="Action{T}"/> (boolean) and will trigger the member to be displayed based
    /// on the passed bool value.
    /// Please remember that adding conditions will add additional overhead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MShowIfAttribute : MonitoringMetaAttribute
    {
        /// <summary>
        /// Name of the member.
        /// </summary>
        public string MemberName { get; }

        /// <summary>
        /// Required conditional value.
        /// </summary>
        public bool RequiredResult { get; }

        /// <summary>
        /// Condition type.
        /// </summary>
        public Condition Condition { get; }

        /// <summary>
        /// Comparison method.
        /// </summary>
        public Comparison Comparison { get; }

        /// <summary>
        /// Other value boxed in an object.
        /// </summary>
        public object Other { get; }

        /// <summary>
        /// Validation method type.
        /// </summary>
        public ValidationMethod ValidationMethod { get; }

        /// <summary>
        /// The monitored member will only be displayed if the given condition is true.
        /// </summary>
        public MShowIfAttribute(Condition condition)
        {
            Condition = condition;
            ValidationMethod = ValidationMethod.Condition;
        }

        /// <summary>
        /// The monitored member will only be displayed if the given value of the passed member returns true.<br/>
        /// Methods can accept the members current value as a parameter and use custom logic based on the members current
        /// state to determine if the member should be displayed or not.<br/>
        /// Passed events must be of type <see cref="Action{T}"/> (boolean) and will trigger the member to be displayed based
        /// on the passed bool value.
        /// Please remember that adding conditions will add additional overhead.
        /// </summary>
        public MShowIfAttribute(string memberName, bool result = true)
        {
            MemberName = memberName;
            RequiredResult = result;
            ValidationMethod = ValidationMethod.ByMember;
        }

        /// <summary>
        /// The monitored member will only be displayed if the given comparison to the passed object is true.
        /// Please remember that adding conditions will add additional overhead.
        /// </summary>
        public MShowIfAttribute(Comparison comparison, object other)
        {
            Comparison = comparison;
            Other = other;
            ValidationMethod = ValidationMethod.Comparison;
        }
    }
}