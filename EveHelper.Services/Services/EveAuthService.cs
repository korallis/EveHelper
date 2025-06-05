using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Authentication;
using EveHelper.Services.Helpers;

namespace EveHelper.Services.Services
{
    /// <summary>
    /// Service for handling EVE Online OAuth 2.0 authentication
    /// </summary>
    public class EveAuthService : IEveAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly EveAuthConfiguration _config;
        private PkceData? _currentPkceData;
        private EveToken? _currentToken;

        /// <summary>
        /// Event raised when authentication state changes
        /// </summary>
        public event EventHandler<AuthenticationStateChangedEventArgs>? AuthenticationStateChanged;

        /// <summary>
        /// Gets whether a user is currently authenticated
        /// </summary>
        public bool IsAuthenticated => _currentToken != null && !_currentToken.IsExpired;

        /// <summary>
        /// Gets the current access token if authenticated
        /// </summary>
        public EveToken? CurrentToken => _currentToken;

        /// <summary>
        /// Initializes a new instance of the EVE authentication service
        /// </summary>
        /// <param name="httpClient">HTTP client for API requests</param>
        /// <param name="config">EVE authentication configuration</param>
        public EveAuthService(HttpClient httpClient, EveAuthConfiguration? config = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? EveAuthConfiguration.Default;

            // Set user agent for ESI compliance
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("EveHelper/1.0 (EVE Online Fitting Tool)");
        }

        /// <summary>
        /// Generates PKCE data and returns the authorization URL for EVE OAuth
        /// </summary>
        /// <returns>Authorization URL to redirect user to</returns>
        public async Task<string> StartAuthenticationAsync()
        {
            await Task.CompletedTask; // Make method async for future extensibility

            // Generate PKCE data for this authentication session
            _currentPkceData = PkceHelper.GeneratePkceData();

            // Construct authorization URL
            var queryParams = new Dictionary<string, string>
            {
                ["response_type"] = "code",
                ["client_id"] = _config.ClientId,
                ["redirect_uri"] = _config.CallbackUrl,
                ["scope"] = string.Join(" ", _config.RequiredScopes),
                ["code_challenge"] = _currentPkceData.CodeChallenge,
                ["code_challenge_method"] = _currentPkceData.CodeChallengeMethod,
                ["state"] = _currentPkceData.State
            };

            var queryString = BuildQueryString(queryParams);
            return $"{_config.AuthorizationEndpoint}?{queryString}";
        }

        /// <summary>
        /// Exchanges authorization code for access and refresh tokens
        /// </summary>
        /// <param name="authorizationCode">Authorization code from callback</param>
        /// <param name="state">State parameter to verify against CSRF</param>
        /// <returns>Authentication result with token information</returns>
        public async Task<AuthenticationResult> CompleteAuthenticationAsync(string authorizationCode, string state)
        {
            try
            {
                // Validate state parameter against CSRF attacks
                if (_currentPkceData == null || state != _currentPkceData.State)
                {
                    return AuthenticationResult.Failure("Invalid state parameter. Possible CSRF attack.");
                }

                // Prepare token exchange request
                var tokenRequest = new Dictionary<string, string>
                {
                    ["grant_type"] = "authorization_code",
                    ["client_id"] = _config.ClientId,
                    ["client_secret"] = _config.ClientSecret,
                    ["code"] = authorizationCode,
                    ["redirect_uri"] = _config.CallbackUrl,
                    ["code_verifier"] = _currentPkceData.CodeVerifier
                };

                var content = new FormUrlEncodedContent(tokenRequest);

                // Exchange authorization code for tokens
                var response = await _httpClient.PostAsync(_config.TokenEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return AuthenticationResult.Failure($"Token exchange failed: {responseContent}");
                }

                // Parse token response
                var tokenResponse = JsonSerializer.Deserialize<EveToken>(responseContent);
                if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                {
                    return AuthenticationResult.Failure("Invalid token response from EVE Online.");
                }

                // Extract character information from JWT access token
                await ExtractCharacterInfoFromToken(tokenResponse);

                // Store the token
                _currentToken = tokenResponse;
                _currentPkceData = null; // Clear PKCE data after successful exchange

                // Raise authentication state changed event
                OnAuthenticationStateChanged(true, tokenResponse, null);

                return AuthenticationResult.Success(tokenResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Authentication failed: {ex.Message}";
                OnAuthenticationStateChanged(false, null, errorMessage);
                return AuthenticationResult.Failure(errorMessage);
            }
        }

        /// <summary>
        /// Refreshes the current access token using the refresh token
        /// </summary>
        /// <returns>New token information</returns>
        public async Task<EveToken> RefreshTokenAsync()
        {
            if (_currentToken == null || !_currentToken.CanRefresh)
            {
                throw new InvalidOperationException("No valid refresh token available.");
            }

            try
            {
                var refreshRequest = new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = _config.ClientId,
                    ["client_secret"] = _config.ClientSecret,
                    ["refresh_token"] = _currentToken.RefreshToken
                };

                var content = new FormUrlEncodedContent(refreshRequest);
                var response = await _httpClient.PostAsync(_config.TokenEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Token refresh failed: {responseContent}");
                }

                var tokenResponse = JsonSerializer.Deserialize<EveToken>(responseContent);
                if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                {
                    throw new InvalidOperationException("Invalid token refresh response from EVE Online.");
                }

                // Update token with preserved character info
                _currentToken = _currentToken.WithUpdatedToken(
                    tokenResponse.AccessToken,
                    tokenResponse.ExpiresIn,
                    tokenResponse.RefreshToken);

                // Raise authentication state changed event
                OnAuthenticationStateChanged(true, _currentToken, null);

                return _currentToken;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Token refresh failed: {ex.Message}";
                OnAuthenticationStateChanged(false, null, errorMessage);
                throw;
            }
        }

        /// <summary>
        /// Clears authentication state and logs out the user
        /// </summary>
        public void Logout()
        {
            _currentToken = null;
            _currentPkceData = null;
            OnAuthenticationStateChanged(false, null, null);
        }

        /// <summary>
        /// Attempts to restore authentication state from persistent storage
        /// </summary>
        /// <returns>True if authentication was restored</returns>
        public async Task<bool> RestoreAuthenticationAsync()
        {
            // TODO: Implement token persistence (will be handled in subtask 2.2)
            await Task.CompletedTask;
            return false;
        }

        /// <summary>
        /// Extracts character information from the JWT access token
        /// </summary>
        /// <param name="token">Token to extract character info from</param>
        private async Task ExtractCharacterInfoFromToken(EveToken token)
        {
            await Task.CompletedTask; // Make method async for future extensibility

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token.AccessToken);

                // Extract character ID and name from JWT claims
                var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "sub");
                var nameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "name");

                if (subClaim != null && subClaim.Value.StartsWith("CHARACTER:EVE:"))
                {
                    var characterIdString = subClaim.Value.Replace("CHARACTER:EVE:", "");
                    if (int.TryParse(characterIdString, out var characterId))
                    {
                        token.CharacterId = characterId;
                    }
                }

                if (nameClaim != null)
                {
                    token.CharacterName = nameClaim.Value;
                }
            }
            catch (Exception)
            {
                // JWT parsing failed - continue without character info
                // This is not critical as we can get character info from ESI later
            }
        }

        /// <summary>
        /// Builds a URL query string from a dictionary of parameters
        /// </summary>
        /// <param name="parameters">Parameters to encode</param>
        /// <returns>URL-encoded query string</returns>
        private static string BuildQueryString(Dictionary<string, string> parameters)
        {
            var queryBuilder = new StringBuilder();
            foreach (var param in parameters)
            {
                if (queryBuilder.Length > 0)
                    queryBuilder.Append('&');

                queryBuilder.Append(Uri.EscapeDataString(param.Key));
                queryBuilder.Append('=');
                queryBuilder.Append(Uri.EscapeDataString(param.Value));
            }
            return queryBuilder.ToString();
        }

        /// <summary>
        /// Raises the AuthenticationStateChanged event
        /// </summary>
        /// <param name="isAuthenticated">Whether user is authenticated</param>
        /// <param name="token">Current token if authenticated</param>
        /// <param name="errorMessage">Error message if authentication failed</param>
        private void OnAuthenticationStateChanged(bool isAuthenticated, EveToken? token, string? errorMessage)
        {
            AuthenticationStateChanged?.Invoke(this, new AuthenticationStateChangedEventArgs
            {
                IsAuthenticated = isAuthenticated,
                Token = token,
                ErrorMessage = errorMessage
            });
        }
    }
} 