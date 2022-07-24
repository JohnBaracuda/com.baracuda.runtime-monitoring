using System;
using Baracuda.Monitoring.Source.Utilities;

namespace Baracuda.Monitoring
{
    //TODO: Test with events & methods
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Event)]
    public class MConditionalAttribute : MonitoringMetaAttribute
    {
        public readonly Condition Condition;
        public readonly string MemberName;

        public readonly Comparison Comparison;
        public readonly object Other;

        internal readonly ValidationMethod ValidationMethod;

        /// <summary>
        /// The monitored member will only be displayed if the given condition is true.
        /// </summary>
        public MConditionalAttribute(Condition condition)
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
        public MConditionalAttribute(string memberName)
        {
            MemberName = memberName;
            ValidationMethod = ValidationMethod.ByMember;
        }
        
        /// <summary>
        /// The monitored member will only be displayed if the given comparison to the passed object is true.
        /// Please remember that adding conditions will add additional overhead.
        /// </summary>
        public MConditionalAttribute(Comparison comparison, object other)
        {
            Comparison = comparison;
            Other = other;
            ValidationMethod = ValidationMethod.Comparison;
        }
    }

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
    
    public enum Condition
    {
        /// <summary>
        /// Show if the value is true.
        /// </summary>
        True = 0,
        
        /// <summary>
        /// Show if the value is false.
        /// </summary>
        False = 1,
        
        /// <summary>
        /// Show if the value is null.
        /// </summary>
        Null = 2,
        
        /// <summary>
        /// Show if the value not null.
        /// </summary>
        NotNull = 3,
        
        /// <summary>
        /// Show if the value is a number and not zero(0).
        /// </summary>
        NotZero = 4,
        
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
        CollectionNotEmpty = 9
    }
}