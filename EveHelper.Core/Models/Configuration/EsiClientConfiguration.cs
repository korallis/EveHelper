using System;

namespace EveHelper.Core.Models.Configuration
{
    /// <summary>
    /// Configuration for the ESI client service
    /// </summary>
    public class EsiClientConfiguration
    {
        /// <summary>
        /// ESI API base URL
        /// </summary>
        public string BaseUrl { get; set; } = "https://esi.evetech.net";

        /// <summary>
        /// Default datasource for ESI requests
        /// </summary>
        public string Datasource { get; set; } = "tranquility";

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum number of retry attempts for failed requests
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Base delay between retry attempts
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Whether to use exponential backoff for retries
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        /// Whether to respect ESI cache headers
        /// </summary>
        public bool RespectCacheHeaders { get; set; } = true;

        /// <summary>
        /// Whether to respect ESI rate limiting headers
        /// </summary>
        public bool RespectRateLimiting { get; set; } = true;

        /// <summary>
        /// User agent string for ESI requests
        /// </summary>
        public string UserAgent { get; set; } = "EveHelper/1.0 (https://github.com/korallis/EveHelper)";

        /// <summary>
        /// Whether to enable detailed logging of requests and responses
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Whether to log request and response bodies (for debugging)
        /// </summary>
        public bool LogRequestBodies { get; set; } = false;

        /// <summary>
        /// Maximum content length to log (prevents logging huge responses)
        /// </summary>
        public long MaxLogContentLength { get; set; } = 10240; // 10KB

        /// <summary>
        /// Default configuration with recommended settings
        /// </summary>
        public static EsiClientConfiguration Default => new()
        {
            BaseUrl = "https://esi.evetech.net",
            Datasource = "tranquility",
            TimeoutSeconds = 30,
            MaxRetryAttempts = 3,
            RetryDelay = TimeSpan.FromMilliseconds(500),
            UseExponentialBackoff = true,
            RespectCacheHeaders = true,
            RespectRateLimiting = true,
            UserAgent = "EveHelper/1.0 (https://github.com/korallis/EveHelper)",
            EnableLogging = true,
            LogRequestBodies = false,
            MaxLogContentLength = 10240
        };
    }
} 