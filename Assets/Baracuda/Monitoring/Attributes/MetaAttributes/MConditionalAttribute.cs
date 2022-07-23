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
        /// ...
        /// Please remember that adding conditions will add additional overhead.
        /// </summary>
        public MConditionalAttribute(Condition condition)
        {
            Condition = condition;
            ValidationMethod = ValidationMethod.Condition;
        }

        /// <summary>
        /// ...
        /// Please remember that adding conditions will add additional overhead.
        /// </summary>
        public MConditionalAttribute(string memberName)
        {
            MemberName = memberName;
            ValidationMethod = ValidationMethod.ByMember;
        }
        
        /// <summary>
        /// ...
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
        None = 0,
        True = 1,
        False = 2,
        Null = 3,
        NotNull = 4,
        NotZero = 5,
        Negative = 6,
        Positive = 7,
        NotNullOrEmpty = 10,
        NotNullOrWhiteSpace = 11
    }
}