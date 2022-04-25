using System;

namespace Baracuda.Monitoring.Internal.Utilities
{
    public enum UnitType
    {
        Field,
        Property,
        Event,
        Method
    }

    internal static class UnitTypeExtensions
    {
        public static string AsString(this UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Field => nameof(UnitType.Field),
                UnitType.Property => nameof(UnitType.Property),
                UnitType.Event => nameof(UnitType.Event),
                UnitType.Method => nameof(UnitType.Method),
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null)
            };
        }
    }
}