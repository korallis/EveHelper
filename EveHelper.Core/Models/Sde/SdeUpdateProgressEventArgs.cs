using System;

namespace EveHelper.Core.Models.Sde
{
    /// <summary>
    /// Event arguments for SDE update progress notifications.
    /// </summary>
    public class SdeUpdateProgressEventArgs : EventArgs
    {
        /// <summary>
        /// A message describing the current status of the update.
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>
        /// Progress percentage (0.0 to 100.0).
        /// </summary>
        public double ProgressPercentage { get; set; }

        /// <summary>
        /// Number of bytes downloaded so far.
        /// </summary>
        public long BytesDownloaded { get; set; }

        /// <summary>
        /// Total bytes to be downloaded for the SDE.
        /// </summary>
        public long TotalBytesToDownload { get; set; }

        /// <summary>
        /// True if the current phase is checksum verification.
        /// </summary>
        public bool IsVerifying { get; set; }

        /// <summary>
        /// True if the current phase is applying/installing the SDE.
        /// </summary>
        public bool IsApplying { get; set; }
    }
} 