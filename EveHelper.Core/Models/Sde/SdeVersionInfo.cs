using System;

namespace EveHelper.Core.Models.Sde
{
    /// <summary>
    /// Holds information about a specific SDE version release.
    /// </summary>
    public class SdeVersionInfo
    {
        /// <summary>
        /// Version identifier, typically a date string like "YYYYMMDD".
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Direct URL to download the SDE archive (e.g., sqlite-latest.zip).
        /// </summary>
        public Uri? DownloadUrl { get; set; }

        /// <summary>
        /// The checksum value provided for file integrity verification.
        /// </summary>
        public string? Checksum { get; set; }

        /// <summary>
        /// The type of checksum (e.g., "MD5", "SHA256").
        /// </summary>
        public string? ChecksumType { get; set; }

        /// <summary>
        /// The release date of this SDE version.
        /// </summary>
        public DateTime ReleaseDate { get; set; }
    }
} 