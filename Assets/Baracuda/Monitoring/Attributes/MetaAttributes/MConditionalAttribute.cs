using System;

namespace Baracuda.Monitoring
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class MConditionalAttribute : MonitoringMetaAttribute
    {
        public readonly Condition Condition;
        public readonly string MemberName;

        public readonly Comparison Comparison;
        public readonly object Other;

        public readonly ValidationMethod ValidationMethod;

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

    public enum ValidationMethod
    {
        ByMember,
        Comparison,
        Condition
    }
    
    public enum Comparison
    {
        Equals,
        EqualsNot,
        Greater,
        GreaterOrEqual,
        Lesser,
        LesserOrEqual
    }
    
    public enum Condition
    {
        True = 0,
        False = 1,
        Null = 2,
        NotNull = 3,
        NotZero = 4,
        Negative = 5,
        Positive = 6,
        NotNullOrEmptyString = 7,
        NotNullOrWhiteSpace = 8,
        NotEmpty = 9
    }
}