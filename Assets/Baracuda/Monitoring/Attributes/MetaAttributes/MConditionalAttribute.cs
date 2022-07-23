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
        /// TODO
        /// Please remember that adding conditions will add additional overhead.
        /// </summary>
        public MConditionalAttribute(Condition condition)
        {
            Condition = condition;
            ValidationMethod = ValidationMethod.Condition;
        }

        /// <summary>
        /// TODO
        /// Please remember that adding conditions will add additional overhead.
        /// </summary>
        public MConditionalAttribute(string memberName)
        {
            MemberName = memberName;
            ValidationMethod = ValidationMethod.ByMember;
        }
        
        /// <summary>
        /// TODO
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
        NotNullOrEmpty = 7,
        NotNullOrWhiteSpace = 8
    }
}