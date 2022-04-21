using System;
using Baracuda.Monitoring.Internal.Reflection;

[assembly: DisableAssemblyReflection]
namespace Baracuda.Monitoring.Internal.Reflection
{
    /// <summary>
    /// Disable reflection for the target assembly or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class  | AttributeTargets.Struct)]
    public class DisableAssemblyReflectionAttribute : Attribute
    {
    }
}
