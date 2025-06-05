using System;
using System.Threading.Tasks;
using EveHelper.Core.Models.Authentication;

namespace EveHelper.Core.Interfaces
{
    /// <summary>
    /// Interface for secure token storage and management
    /// </summary>
    public interface ITokenStorageService
    {
        /// <summary>
        /// Event raised when a token is about to expire and needs refresh
        /// </summary>
        event EventHandler<TokenExpirationEventArgs>? TokenExpiring;

        /// <summary>
        /// Event raised when token refresh is completed
        /// </summary>
        event EventHandler<TokenRefreshEventArgs>? TokenRefreshed;

        /// <summary>
        /// Securely stores an EVE token for a character
        /// </summary>
        /// <param name="characterId">EVE character ID</param>
        /// <param name="token">Token to store</param>
        /// <returns>True if storage was successful</returns>
        Task<bool> StoreTokenAsync(long characterId, EveToken token);

        /// <summary>
        /// Retrieves a stored token for a character
        /// </summary>
        /// <param name="characterId">EVE character ID</param>
        /// <returns>The stored token or null if not found</returns>
        Task<EveToken?> GetTokenAsync(long characterId);

        /// <summary>
        /// Removes a stored token for a character
        /// </summary>
        /// <param name="characterId">EVE character ID</param>
        /// <returns>True if removal was successful</returns>
        Task<bool> RemoveTokenAsync(long characterId);

        /// <summary>
        /// Gets all stored character IDs
        /// </summary>
        /// <returns>Array of character IDs that have stored tokens</returns>
        Task<long[]> GetStoredCharacterIdsAsync();

        /// <summary>
        /// Checks if a token exists and is valid for a character
        /// </summary>
        /// <param name="characterId">EVE character ID</param>
        /// <returns>True if a valid token exists</returns>
        Task<bool> HasValidTokenAsync(long characterId);

        /// <summary>
        /// Refreshes a token if it's close to expiration
        /// </summary>
        /// <param name="characterId">EVE character ID</param>
        /// <returns>The refreshed token or null if refresh failed</returns>
        Task<EveToken?> RefreshTokenIfNeededAsync(long characterId);

        /// <summary>
        /// Clears all stored tokens (for logout scenarios)
        /// </summary>
        /// <returns>True if clearing was successful</returns>
        Task<bool> ClearAllTokensAsync();

        /// <summary>
        /// Starts the automatic token refresh monitoring
        /// </summary>
        void StartTokenMonitoring();

        /// <summary>
        /// Stops the automatic token refresh monitoring
        /// </summary>
        void StopTokenMonitoring();
    }

    /// <summary>
    /// Event args for token expiration events
    /// </summary>
    public class TokenExpirationEventArgs : EventArgs
    {
        /// <summary>
        /// Character ID whose token is expiring
        /// </summary>
        public long CharacterId { get; set; }

        /// <summary>
        /// The expiring token
        /// </summary>
        public EveToken Token { get; set; } = new();

        /// <summary>
        /// Time until expiration
        /// </summary>
        public TimeSpan TimeUntilExpiration { get; set; }
    }

    /// <summary>
    /// Event args for token refresh events
    /// </summary>
    public class TokenRefreshEventArgs : EventArgs
    {
        /// <summary>
        /// Character ID whose token was refreshed
        /// </summary>
        public long CharacterId { get; set; }

        /// <summary>
        /// Whether the refresh was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The new token (if refresh was successful)
        /// </summary>
        public EveToken? NewToken { get; set; }

        /// <summary>
        /// Error message if refresh failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
} 