using System;

namespace Baracuda.Monitoring
{
    /// <summary>
    /// Contains string extensions for enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Better ToString method.
        /// </summary>
        public static string AsString(this UIPosition position)
        {
            switch (position)
            {
                case UIPosition.UpperLeft:
                    return nameof(UIPosition.UpperLeft);
                case UIPosition.UpperRight:
                    return nameof(UIPosition.UpperRight);
                case UIPosition.LowerLeft:
                    return nameof(UIPosition.LowerLeft);
                case UIPosition.LowerRight:
                    return nameof(UIPosition.LowerRight);
                default:
                    throw new ArgumentOutOfRangeException(nameof(position));
            }
        }
    }
}