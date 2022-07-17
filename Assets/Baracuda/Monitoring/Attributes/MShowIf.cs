using System;

namespace Baracuda.Monitoring
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class MShowIf : MonitoringMetaAttribute
    {
        public Condition Condition { get; }
        public BinaryCondition BinaryCondition { get; }
        public string ValidationMethodName { get; }

        public MShowIf(Condition condition)
        {
            Condition = condition;
        }

        public MShowIf(string validationMethodName)
        {
            ValidationMethodName = validationMethodName;
            Condition = Condition.ValidationMethod;
        }

        public MShowIf(BinaryCondition condition, object other)
        {
            
        }
    }
    
    public enum Condition
    {
        ValidationMethod = -1,
        None = 0,
        True = 1,
        False = 2,
        Null = 3,
        NotNull = 4,
        NotZero = 5,
        Negative = 6,
        Positive = 7,
        DefaultValue = 8,
        NotDefaultValue = 9,
        NotNullOrEmpty = 10,
        NotNullOrWhiteSpace = 11
    }

    public enum BinaryCondition
    {
        Equals,
        EqualsNot,
        GreaterThan,
        LesserThan,
        GreaterOrEqual,
        LesserOrEqual,
    }
}