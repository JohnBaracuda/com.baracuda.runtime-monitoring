using System.Reflection;

namespace Baracuda.Monitoring.Internal.Utils
{
    /// <summary>
    /// Struct acts as a wrapper for additional arguments that need to be passed when constructing a unit profile.
    /// </summary>
    public readonly struct MonitorProfileCtorArgs
    {
        public readonly BindingFlags ReflectedMemberFlags;

        public MonitorProfileCtorArgs(BindingFlags reflectedMemberFlags)
        {
            ReflectedMemberFlags = reflectedMemberFlags;
        }
    }
}