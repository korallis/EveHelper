using System;
using System.Threading.Tasks;

namespace EveHelper.Core.Interfaces
{
    /// <summary>
    /// Interface for making authenticated requests to the EVE Online ESI API
    /// </summary>
    public interface IEsiClientService
    {
        /// <summary>
        /// Makes an authenticated GET request to an ESI endpoint
        /// </summary>
        /// <typeparam name="T">Expected response type</typeparam>
        /// <param name="endpoint">ESI endpoint path (without base URL)</param>
        /// <param name="characterId">Character ID for authentication</param>
        /// <returns>Deserialized response or null if request failed</returns>
        Task<T?> GetAsync<T>(string endpoint, long characterId) where T : class;

        /// <summary>
        /// Makes an authenticated GET request to an ESI endpoint with optional parameters
        /// </summary>
        /// <typeparam name="T">Expected response type</typeparam>
        /// <param name="endpoint">ESI endpoint path (without base URL)</param>
        /// <param name="characterId">Character ID for authentication</param>
        /// <param name="queryParameters">Optional query parameters</param>
        /// <returns>Deserialized response or null if request failed</returns>
        Task<T?> GetAsync<T>(string endpoint, long characterId, object? queryParameters) where T : class;

        /// <summary>
        /// Makes an authenticated POST request to an ESI endpoint
        /// </summary>
        /// <typeparam name="TRequest">Request body type</typeparam>
        /// <typeparam name="TResponse">Expected response type</typeparam>
        /// <param name="endpoint">ESI endpoint path (without base URL)</param>
        /// <param name="characterId">Character ID for authentication</param>
        /// <param name="data">Request body data</param>
        /// <returns>Deserialized response or null if request failed</returns>
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, long characterId, TRequest data) 
            where TResponse : class;

        /// <summary>
        /// Makes an authenticated PUT request to an ESI endpoint
        /// </summary>
        /// <typeparam name="TRequest">Request body type</typeparam>
        /// <typeparam name="TResponse">Expected response type</typeparam>
        /// <param name="endpoint">ESI endpoint path (without base URL)</param>
        /// <param name="characterId">Character ID for authentication</param>
        /// <param name="data">Request body data</param>
        /// <returns>Deserialized response or null if request failed</returns>
        Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, long characterId, TRequest data) 
            where TResponse : class;

        /// <summary>
        /// Makes an authenticated DELETE request to an ESI endpoint
        /// </summary>
        /// <param name="endpoint">ESI endpoint path (without base URL)</param>
        /// <param name="characterId">Character ID for authentication</param>
        /// <returns>True if the request was successful</returns>
        Task<bool> DeleteAsync(string endpoint, long characterId);

        /// <summary>
        /// Makes a public (unauthenticated) GET request to an ESI endpoint
        /// </summary>
        /// <typeparam name="T">Expected response type</typeparam>
        /// <param name="endpoint">ESI endpoint path (without base URL)</param>
        /// <returns>Deserialized response or null if request failed</returns>
        Task<T?> GetPublicAsync<T>(string endpoint) where T : class;

        /// <summary>
        /// Makes a public (unauthenticated) GET request to an ESI endpoint with parameters
        /// </summary>
        /// <typeparam name="T">Expected response type</typeparam>
        /// <param name="endpoint">ESI endpoint path (without base URL)</param>
        /// <param name="queryParameters">Optional query parameters</param>
        /// <returns>Deserialized response or null if request failed</returns>
        Task<T?> GetPublicAsync<T>(string endpoint, object? queryParameters) where T : class;

        /// <summary>
        /// Validates that a character token is valid and can access ESI
        /// </summary>
        /// <param name="characterId">Character ID to validate</param>
        /// <returns>True if the character has a valid token</returns>
        Task<bool> ValidateCharacterTokenAsync(long characterId);

        /// <summary>
        /// Event raised when an ESI request returns cache information
        /// </summary>
        event EventHandler<EsiCacheEventArgs>? CacheInfoReceived;

        /// <summary>
        /// Event raised when rate limiting is applied to requests
        /// </summary>
        event EventHandler<EsiRateLimitEventArgs>? RateLimitApplied;
    }

    /// <summary>
    /// Event args for ESI cache information
    /// </summary>
    public class EsiCacheEventArgs : EventArgs
    {
        /// <summary>
        /// The endpoint that was called
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Cache expiration time
        /// </summary>
        public DateTime? CacheExpires { get; set; }

        /// <summary>
        /// Last modified time
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// ETag from the response
        /// </summary>
        public string? ETag { get; set; }
    }

    /// <summary>
    /// Event args for ESI rate limiting
    /// </summary>
    public class EsiRateLimitEventArgs : EventArgs
    {
        /// <summary>
        /// Current error limit window
        /// </summary>
        public int ErrorLimitRemain { get; set; }

        /// <summary>
        /// Time when error limit resets
        /// </summary>
        public DateTime ErrorLimitReset { get; set; }

        /// <summary>
        /// Delay applied to respect rate limits
        /// </summary>
        public TimeSpan DelayApplied { get; set; }
    }
} 