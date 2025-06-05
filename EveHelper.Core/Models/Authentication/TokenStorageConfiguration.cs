using System;

namespace EveHelper.Core.Models.Authentication
{
    /// <summary>
    /// Configuration settings for token storage and management
    /// </summary>
    public class TokenStorageConfiguration
    {
        /// <summary>
        /// Time before expiration when tokens should be refreshed
        /// Default: 5 minutes
        /// </summary>
        public TimeSpan RefreshThreshold { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Interval for checking token expiration
        /// Default: 1 minute
        /// </summary>
        public TimeSpan MonitoringInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Whether to use Windows Credential Manager for storage
        /// Default: true
        /// </summary>
        public bool UseCredentialManager { get; set; } = true;

        /// <summary>
        /// Fallback to encrypted file storage if Credential Manager fails
        /// Default: true
        /// </summary>
        public bool UseFallbackFileStorage { get; set; } = true;

        /// <summary>
        /// Directory for encrypted file storage (if used)
        /// Default: %APPDATA%/EveHelper/Tokens
        /// </summary>
        public string? FileStorageDirectory { get; set; }

        /// <summary>
        /// Encryption key derivation iterations for file storage
        /// Default: 10000
        /// </summary>
        public int EncryptionIterations { get; set; } = 10000;

        /// <summary>
        /// Target name prefix for Windows Credential Manager entries
        /// Default: "EveHelper_Token_"
        /// </summary>
        public string CredentialTargetPrefix { get; set; } = "EveHelper_Token_";

        /// <summary>
        /// Maximum number of retry attempts for token refresh
        /// Default: 3
        /// </summary>
        public int MaxRefreshRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts
        /// Default: 2 seconds
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Whether to automatically start token monitoring on service initialization
        /// Default: true
        /// </summary>
        public bool AutoStartMonitoring { get; set; } = true;

        /// <summary>
        /// Whether to log token operations (without sensitive data)
        /// Default: true
        /// </summary>
        public bool EnableLogging { get; set; } = true;
    }
} 