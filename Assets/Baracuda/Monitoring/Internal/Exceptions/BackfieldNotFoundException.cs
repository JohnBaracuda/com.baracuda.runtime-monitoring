using System;
using System.Reflection;

namespace Baracuda.Monitoring.Internal.Exceptions
{
    /// <summary>
    /// Exception occurs if the backing field of a monitored property cannot be found.
    /// </summary>
    internal class BackfieldNotFoundException : Exception
    {
        public BackfieldNotFoundException(PropertyInfo propertyInfo) 
            : base($"Backfield of {propertyInfo.Name} in {propertyInfo.DeclaringType?.FullName} was not found!")
        {
        }
    }
}