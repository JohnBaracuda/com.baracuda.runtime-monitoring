// Copyright (c) 2022 Jonathan Lang
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
            switch (unitType)
            {
                case UnitType.Field:
                    return nameof(UnitType.Field);
                case UnitType.Property:
                    return nameof(UnitType.Property);
                case UnitType.Event:
                    return nameof(UnitType.Event);
                case UnitType.Method:
                    return nameof(UnitType.Method);
                default:
                    throw new ArgumentOutOfRangeException(nameof(unitType), unitType, null);
            }
        }
    }
}