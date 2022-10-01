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

    /// <summary>
    /// Validation method type.
    /// </summary>
    public enum ValidationMethod
    {
        /// <summary>
        /// Value is validated via the result of an external member.
        /// </summary>
        ByMember,

        /// <summary>
        /// Value is validated via a comparison to another constant value that is passed set via attribute.
        /// </summary>
        Comparison,

        /// <summary>
        /// Value is validated via special condition. (e.g: collection not empty)
        /// </summary>
        Condition
    }

    /// <summary>
    /// Comparison type.
    /// </summary>
    public enum Comparison
    {
        /// <summary>
        /// Show if the current value and the passed 'other' value are equal.
        /// </summary>
        Equals,

        /// <summary>
        /// Show if the current value and the passed 'other' value are not equal.
        /// </summary>
        EqualsNot,

        /// <summary>
        /// Show if the current value is a number that is greater than the passed 'other' value.
        /// </summary>
        Greater,

        /// <summary>
        /// Show if the current value is a number that is greater or equal to the passed 'other' value.
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        /// Show if the current value is a number that is lesser than the passed 'other' value.
        /// </summary>
        Lesser,

        /// <summary>
        /// Show if the current value is a number that is lesser or equal to the passed 'other' value.
        /// </summary>
        LesserOrEqual
    }

    /// <summary>
    /// Condition type.
    /// </summary>
    public enum Condition
    {
        /// <summary>
        /// Show if the value is false.
        /// </summary>
        False = 0,

        /// <summary>
        /// Show if the value is true.
        /// </summary>
        True = 1,

        /// <summary>
        /// Show if the value is null.
        /// </summary>
        Null = 2,

        /// <summary>
        /// Show if the value not null.
        /// </summary>
        NotNull = 3,

        /// <summary>
        /// Show if the value is a number and not zero (0).
        /// </summary>
        NotZero = 4,

        /// <summary>
        /// Show if the value is a number and zero (0).
        /// </summary>
        Zero = 10,

        /// <summary>
        /// Show if the value is a negative number.
        /// </summary>
        Negative = 5,

        /// <summary>
        /// Show if the value is a positive number.
        /// </summary>
        Positive = 6,

        /// <summary>
        /// Show if the value is a string that is not null or empty.
        /// </summary>
        NotNullOrEmptyString = 7,

        /// <summary>
        /// Show if the value is a string that is not null or white space.
        /// </summary>
        NotNullOrWhiteSpace = 8,

        /// <summary>
        /// Show if the value is a collection that is not null and contains at least 1 item.
        /// </summary>
        CollectionNotEmpty = 9,
    }
}