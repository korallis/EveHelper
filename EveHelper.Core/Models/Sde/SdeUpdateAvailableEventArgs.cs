using System;

namespace EveHelper.Core.Models.Sde
{
    /// <summary>
    /// Event arguments for when an SDE update is found.
    /// </summary>
    public class SdeUpdateAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Information about the latest available SDE version.
        /// </summary>
        public SdeVersionInfo AvailableVersion { get; }

        /// <summary>
        /// The currently installed SDE version, if any.
        /// </summary>
        public string? CurrentLocalVersion { get; }

        /// <summary>
        /// Indicates if an update is truly available (i.e., versions differ).
        /// </summary>
        public bool IsUpdateAvailable => string.IsNullOrEmpty(CurrentLocalVersion) || CurrentLocalVersion != AvailableVersion.Version;

        public SdeUpdateAvailableEventArgs(SdeVersionInfo availableVersion, string? currentLocalVersion)
        {
            AvailableVersion = availableVersion;
            CurrentLocalVersion = currentLocalVersion;
        }
    }
} 