using System;
using System.Threading.Tasks;
using EveHelper.Core.Models.Authentication;

namespace EveHelper.Core.Interfaces
{
    /// <summary>
    /// Interface for EVE Online authentication service
    /// </summary>
    public interface IEveAuthService
    {
        /// <summary>
        /// Event raised when authentication state changes
        /// </summary>
        event EventHandler<AuthenticationStateChangedEventArgs>? AuthenticationStateChanged;

        /// <summary>
        /// Gets whether a user is currently authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Gets the current access token if authenticated
        /// </summary>
        EveToken? CurrentToken { get; }

        /// <summary>
        /// Generates PKCE data and returns the authorization URL for EVE OAuth
        /// </summary>
        /// <returns>Authorization URL to redirect user to</returns>
        Task<string> StartAuthenticationAsync();

        /// <summary>
        /// Exchanges authorization code for access and refresh tokens
        /// </summary>
        /// <param name="authorizationCode">Authorization code from callback</param>
        /// <param name="state">State parameter to verify against CSRF</param>
        /// <returns>Authentication result with token information</returns>
        Task<AuthenticationResult> CompleteAuthenticationAsync(string authorizationCode, string state);

        /// <summary>
        /// Refreshes the current access token using the refresh token
        /// </summary>
        /// <returns>New token information</returns>
        Task<EveToken> RefreshTokenAsync();

        /// <summary>
        /// Refreshes a specific refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token to use</param>
        /// <returns>New token information</returns>
        Task<EveToken?> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Clears authentication state and logs out the user
        /// </summary>
        void Logout();

        /// <summary>
        /// Attempts to restore authentication state from persistent storage
        /// </summary>
        /// <returns>True if authentication was restored</returns>
        Task<bool> RestoreAuthenticationAsync();
    }

    /// <summary>
    /// Event arguments for authentication state changes
    /// </summary>
    public class AuthenticationStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Whether the user is now authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// The current token if authenticated
        /// </summary>
        public EveToken? Token { get; set; }

        /// <summary>
        /// Error message if authentication failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Result of authentication operation
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Whether authentication was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The obtained token if successful
        /// </summary>
        public EveToken? Token { get; set; }

        /// <summary>
        /// Error message if authentication failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Creates a successful authentication result
        /// </summary>
        public static AuthenticationResult Success(EveToken token) => new()
        {
            IsSuccess = true,
            Token = token
        };

        /// <summary>
        /// Creates a failed authentication result
        /// </summary>
        public static AuthenticationResult Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
} 