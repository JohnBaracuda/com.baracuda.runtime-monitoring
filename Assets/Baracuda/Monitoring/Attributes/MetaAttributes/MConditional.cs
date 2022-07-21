using System;

namespace Baracuda.Monitoring
{
    //TODO: Add this 
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class MConditional : MonitoringMetaAttribute
    {
        public Condition Condition { get; }
        public string Validator { get; }

        public MConditional(Condition condition)
        {
            Condition = condition;
        }

        public MConditional(string validator)
        {
            Validator = validator;
            Condition = Condition.ValidationMethod;
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
}