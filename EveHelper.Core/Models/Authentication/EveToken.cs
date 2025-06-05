using System;
using System.Text.Json.Serialization;

namespace EveHelper.Core.Models.Authentication
{
    /// <summary>
    /// Represents an EVE Online OAuth 2.0 access token and associated metadata
    /// </summary>
    public class EveToken
    {
        /// <summary>
        /// OAuth 2.0 access token for API requests
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// OAuth 2.0 refresh token for obtaining new access tokens
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Token type (typically "Bearer")
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";

        /// <summary>
        /// Access token lifetime in seconds
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// OAuth scopes granted to this token
        /// </summary>
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// When this token was issued (calculated locally)
        /// </summary>
        [JsonIgnore]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this access token expires (calculated property)
        /// </summary>
        [JsonIgnore]
        public DateTime ExpiresAt => IssuedAt.AddSeconds(ExpiresIn);

        /// <summary>
        /// Whether this access token is expired
        /// </summary>
        [JsonIgnore]
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt.AddMinutes(-5); // 5-minute buffer

        /// <summary>
        /// Whether this token has a valid refresh token
        /// </summary>
        [JsonIgnore]
        public bool CanRefresh => !string.IsNullOrWhiteSpace(RefreshToken);

        /// <summary>
        /// Character ID associated with this token (from JWT payload)
        /// </summary>
        public int? CharacterId { get; set; }

        /// <summary>
        /// Character name associated with this token (from JWT payload)
        /// </summary>
        public string? CharacterName { get; set; }

        /// <summary>
        /// Creates a copy of this token with updated access token information
        /// </summary>
        /// <param name="newAccessToken">New access token</param>
        /// <param name="newExpiresIn">New expiration time in seconds</param>
        /// <param name="newRefreshToken">Optional new refresh token</param>
        /// <returns>Updated token instance</returns>
        public EveToken WithUpdatedToken(string newAccessToken, int newExpiresIn, string? newRefreshToken = null)
        {
            return new EveToken
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken ?? RefreshToken,
                TokenType = TokenType,
                ExpiresIn = newExpiresIn,
                Scope = Scope,
                IssuedAt = DateTime.UtcNow,
                CharacterId = CharacterId,
                CharacterName = CharacterName
            };
        }
    }
} 