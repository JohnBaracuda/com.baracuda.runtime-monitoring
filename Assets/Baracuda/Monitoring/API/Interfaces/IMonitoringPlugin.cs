// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Monitoring
{
    /// <summary>
    /// API to get plugin information like version and links to documentation.
    /// </summary>
    public interface IMonitoringPlugin : IMonitoringSubsystem<IMonitoringPlugin>
    {
        /// <summary>
        /// Copyright notice.
        /// </summary>
        string Copyright { get; }

        /// <summary>
        /// The current version of the plugin.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Weblink to documentation.
        /// </summary>
        string Documentation { get; }

        /// <summary>
        /// Weblink to a public repository.
        /// </summary>
        string Repository { get; }

        /// <summary>
        /// Weblink to publishers website.
        /// </summary>
        string Website { get; }
    }
}