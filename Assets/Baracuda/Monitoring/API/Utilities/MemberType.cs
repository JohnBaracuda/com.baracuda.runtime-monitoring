// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    public enum MemberType
    {
        Field,
        Property,
        Event,
        Method
    }

    public static class MemberTypeExtensions
    {
        public static string AsString(this MemberType memberType)
        {
            switch (memberType)
            {
                case MemberType.Field:
                    return nameof(MemberType.Field);
                case MemberType.Property:
                    return nameof(MemberType.Property);
                case MemberType.Event:
                    return nameof(MemberType.Event);
                case MemberType.Method:
                    return nameof(MemberType.Method);
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberType), memberType, null);
            }
        }
    }
}