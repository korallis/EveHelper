using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace EveHelper.Services.Services
{
    /// <summary>
    /// Service for making authenticated requests to the EVE Online ESI API
    /// </summary>
    public class EsiClientService : IEsiClientService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenStorageService _tokenStorage;
        private readonly IEveAuthService _authService;
        private readonly EsiClientConfiguration _config;
        private readonly ILogger<EsiClientService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly SemaphoreSlim _rateLimitSemaphore;
        private DateTime _lastRequestTime = DateTime.MinValue;
        private readonly object _rateLimitLock = new();

        public event EventHandler<EsiCacheEventArgs>? CacheInfoReceived;
        public event EventHandler<EsiRateLimitEventArgs>? RateLimitApplied;

        public EsiClientService(
            HttpClient httpClient,
            ITokenStorageService tokenStorage,
            IEveAuthService authService,
            EsiClientConfiguration config,
            ILogger<EsiClientService> logger)
        {
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
            _authService = authService;
            _config = config;
            _logger = logger;
            _rateLimitSemaphore = new SemaphoreSlim(1, 1);

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _config.UserAgent);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Makes an authenticated GET request to an ESI endpoint
        /// </summary>
        public async Task<T?> GetAsync<T>(string endpoint, long characterId) where T : class
        {
            return await GetAsync<T>(endpoint, characterId, null);
        }

        /// <summary>
        /// Makes an authenticated GET request to an ESI endpoint with optional parameters
        /// </summary>
        public async Task<T?> GetAsync<T>(string endpoint, long characterId, object? queryParameters) where T : class
        {
            var token = await GetValidTokenAsync(characterId);
            if (token == null)
            {
                _logger.LogWarning("No valid token available for character {CharacterId}", characterId);
                return null;
            }

            var url = BuildUrl(endpoint, queryParameters);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

            return await ExecuteRequestAsync<T>(request, endpoint);
        }

        /// <summary>
        /// Makes an authenticated POST request to an ESI endpoint
        /// </summary>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, long characterId, TRequest data) 
            where TResponse : class
        {
            var token = await GetValidTokenAsync(characterId);
            if (token == null)
            {
                _logger.LogWarning("No valid token available for character {CharacterId}", characterId);
                return null;
            }

            var url = BuildUrl(endpoint);
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await ExecuteRequestAsync<TResponse>(request, endpoint);
        }

        /// <summary>
        /// Makes an authenticated PUT request to an ESI endpoint
        /// </summary>
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, long characterId, TRequest data) 
            where TResponse : class
        {
            var token = await GetValidTokenAsync(characterId);
            if (token == null)
            {
                _logger.LogWarning("No valid token available for character {CharacterId}", characterId);
                return null;
            }

            var url = BuildUrl(endpoint);
            using var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await ExecuteRequestAsync<TResponse>(request, endpoint);
        }

        /// <summary>
        /// Makes an authenticated DELETE request to an ESI endpoint
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint, long characterId)
        {
            var token = await GetValidTokenAsync(characterId);
            if (token == null)
            {
                _logger.LogWarning("No valid token available for character {CharacterId}", characterId);
                return false;
            }

            var url = BuildUrl(endpoint);
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

            try
            {
                await ApplyRateLimitingAsync();
                var response = await _httpClient.SendAsync(request);
                ProcessCacheHeaders(response, endpoint);
                ProcessRateLimitHeaders(response);

                if (_config.EnableLogging)
                {
                    _logger.LogDebug("DELETE {Endpoint} returned {StatusCode}", endpoint, response.StatusCode);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DELETE request to {Endpoint} failed", endpoint);
                return false;
            }
        }

        /// <summary>
        /// Makes a public (unauthenticated) GET request to an ESI endpoint
        /// </summary>
        public async Task<T?> GetPublicAsync<T>(string endpoint) where T : class
        {
            return await GetPublicAsync<T>(endpoint, null);
        }

        /// <summary>
        /// Makes a public (unauthenticated) GET request to an ESI endpoint with parameters
        /// </summary>
        public async Task<T?> GetPublicAsync<T>(string endpoint, object? queryParameters) where T : class
        {
            var url = BuildUrl(endpoint, queryParameters);
            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            return await ExecuteRequestAsync<T>(request, endpoint);
        }

        /// <summary>
        /// Validates that a character token is valid and can access ESI
        /// </summary>
        public async Task<bool> ValidateCharacterTokenAsync(long characterId)
        {
            try
            {
                // Try to get character information as a simple validation
                var characterInfo = await GetAsync<object>($"/latest/characters/{characterId}/", characterId);
                return characterInfo != null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed for character {CharacterId}", characterId);
                return false;
            }
        }

        /// <summary>
        /// Executes an HTTP request with retry logic and error handling
        /// </summary>
        private async Task<T?> ExecuteRequestAsync<T>(HttpRequestMessage request, string endpoint) where T : class
        {
            Exception? lastException = null;

            for (int attempt = 1; attempt <= _config.MaxRetryAttempts; attempt++)
            {
                try
                {
                    await ApplyRateLimitingAsync();

                    using var clonedRequest = await CloneRequestAsync(request);
                    var response = await _httpClient.SendAsync(clonedRequest);

                    ProcessCacheHeaders(response, endpoint);
                    ProcessRateLimitHeaders(response);

                    if (_config.EnableLogging)
                    {
                        _logger.LogDebug("{Method} {Endpoint} returned {StatusCode} (attempt {Attempt})", 
                            request.Method, endpoint, response.StatusCode, attempt);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        
                        if (_config.LogRequestBodies && _config.EnableLogging && content.Length <= _config.MaxLogContentLength)
                        {
                            _logger.LogDebug("Response body: {Content}", content);
                        }

                        if (string.IsNullOrWhiteSpace(content))
                            return null;

                        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    }
                    else if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        // 304 Not Modified - cache is still valid
                        return null;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning("Unauthorized request to {Endpoint} - token may be invalid", endpoint);
                        return null;
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        _logger.LogWarning("Forbidden request to {Endpoint} - insufficient permissions", endpoint);
                        return null;
                    }
                    else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        if (attempt < _config.MaxRetryAttempts)
                        {
                            var retryAfter = GetRetryAfterDelay(response);
                            _logger.LogWarning("Rate limited on {Endpoint}, retrying after {Delay}ms", endpoint, retryAfter.TotalMilliseconds);
                            await Task.Delay(retryAfter);
                            continue;
                        }
                    }
                    else if ((int)response.StatusCode >= 500)
                    {
                        // Server errors - retry with exponential backoff
                        if (attempt < _config.MaxRetryAttempts)
                        {
                            var delay = CalculateRetryDelay(attempt);
                            _logger.LogWarning("Server error {StatusCode} on {Endpoint}, retrying in {Delay}ms", 
                                response.StatusCode, endpoint, delay.TotalMilliseconds);
                            await Task.Delay(delay);
                            continue;
                        }
                    }

                    // Log response for debugging
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Request to {Endpoint} failed with {StatusCode}: {Content}", 
                        endpoint, response.StatusCode, errorContent);
                    return null;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "HTTP request to {Endpoint} failed (attempt {Attempt})", endpoint, attempt);
                    
                    if (attempt < _config.MaxRetryAttempts)
                    {
                        var delay = CalculateRetryDelay(attempt);
                        await Task.Delay(delay);
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    lastException = ex;
                    _logger.LogWarning(ex, "Request to {Endpoint} timed out (attempt {Attempt})", endpoint, attempt);
                    
                    if (attempt < _config.MaxRetryAttempts)
                    {
                        var delay = CalculateRetryDelay(attempt);
                        await Task.Delay(delay);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error during request to {Endpoint}", endpoint);
                    return null;
                }
            }

            _logger.LogError(lastException, "All retry attempts failed for {Endpoint}", endpoint);
            return null;
        }

        /// <summary>
        /// Gets a valid token for a character, refreshing if necessary
        /// </summary>
        private async Task<Core.Models.Authentication.EveToken?> GetValidTokenAsync(long characterId)
        {
            return await _tokenStorage.RefreshTokenIfNeededAsync(characterId);
        }

        /// <summary>
        /// Builds a complete URL with query parameters
        /// </summary>
        private string BuildUrl(string endpoint, object? queryParameters = null)
        {
            var url = endpoint.TrimStart('/');
            
            var parameters = new List<string>();
            
            // Add datasource parameter
            parameters.Add($"datasource={Uri.EscapeDataString(_config.Datasource)}");

            // Add custom query parameters
            if (queryParameters != null)
            {
                var properties = queryParameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    var value = property.GetValue(queryParameters);
                    if (value != null)
                    {
                        var paramName = property.Name.ToLowerInvariant();
                        var paramValue = Uri.EscapeDataString(value.ToString() ?? string.Empty);
                        parameters.Add($"{paramName}={paramValue}");
                    }
                }
            }

            if (parameters.Any())
            {
                var separator = url.Contains('?') ? "&" : "?";
                url += separator + string.Join("&", parameters);
            }

            return url;
        }

        /// <summary>
        /// Applies rate limiting to respect ESI limits
        /// </summary>
        private async Task ApplyRateLimitingAsync()
        {
            if (!_config.RespectRateLimiting)
                return;

            await _rateLimitSemaphore.WaitAsync();
            try
            {
                lock (_rateLimitLock)
                {
                    var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
                    var minimumInterval = TimeSpan.FromMilliseconds(100); // Basic rate limiting: max 10 requests per second

                    if (timeSinceLastRequest < minimumInterval)
                    {
                        var delayNeeded = minimumInterval - timeSinceLastRequest;
                        if (_config.EnableLogging)
                        {
                            _logger.LogDebug("Applying rate limit delay of {Delay}ms", delayNeeded.TotalMilliseconds);
                        }

                        RateLimitApplied?.Invoke(this, new EsiRateLimitEventArgs
                        {
                            DelayApplied = delayNeeded,
                            ErrorLimitRemain = 100, // Default assumption
                            ErrorLimitReset = DateTime.UtcNow.AddMinutes(1)
                        });

                        Task.Delay(delayNeeded).Wait();
                    }

                    _lastRequestTime = DateTime.UtcNow;
                }
            }
            finally
            {
                _rateLimitSemaphore.Release();
            }
        }

        /// <summary>
        /// Processes cache headers from ESI responses
        /// </summary>
        private void ProcessCacheHeaders(HttpResponseMessage response, string endpoint)
        {
            if (!_config.RespectCacheHeaders)
                return;

            try
            {
                DateTime? cacheExpires = null;
                DateTime? lastModified = null;
                string? etag = null;

                if (response.Headers.CacheControl?.MaxAge.HasValue == true)
                {
                    cacheExpires = DateTime.UtcNow.Add(response.Headers.CacheControl.MaxAge.Value);
                }
                else if (response.Content.Headers.Expires.HasValue)
                {
                    cacheExpires = response.Content.Headers.Expires.Value.UtcDateTime;
                }

                if (response.Content.Headers.LastModified.HasValue)
                {
                    lastModified = response.Content.Headers.LastModified.Value.UtcDateTime;
                }

                if (response.Headers.ETag != null)
                {
                    etag = response.Headers.ETag.Tag;
                }

                if (cacheExpires.HasValue || lastModified.HasValue || !string.IsNullOrEmpty(etag))
                {
                    CacheInfoReceived?.Invoke(this, new EsiCacheEventArgs
                    {
                        Endpoint = endpoint,
                        CacheExpires = cacheExpires,
                        LastModified = lastModified,
                        ETag = etag
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process cache headers for {Endpoint}", endpoint);
            }
        }

        /// <summary>
        /// Processes rate limiting headers from ESI responses
        /// </summary>
        private void ProcessRateLimitHeaders(HttpResponseMessage response)
        {
            if (!_config.RespectRateLimiting)
                return;

            try
            {
                if (response.Headers.TryGetValues("X-ESI-Error-Limit-Remain", out var remainValues) &&
                    response.Headers.TryGetValues("X-ESI-Error-Limit-Reset", out var resetValues))
                {
                    if (int.TryParse(remainValues.FirstOrDefault(), out var errorLimitRemain) &&
                        int.TryParse(resetValues.FirstOrDefault(), out var errorLimitResetSeconds))
                    {
                        var errorLimitReset = DateTime.UtcNow.AddSeconds(errorLimitResetSeconds);
                        
                        if (_config.EnableLogging)
                        {
                            _logger.LogDebug("ESI rate limit: {Remain} errors remaining, resets at {Reset}", 
                                errorLimitRemain, errorLimitReset);
                        }

                        // If we're close to the error limit, apply additional delay
                        if (errorLimitRemain < 10)
                        {
                            var additionalDelay = TimeSpan.FromMilliseconds(500);
                            Task.Delay(additionalDelay).Wait();

                            RateLimitApplied?.Invoke(this, new EsiRateLimitEventArgs
                            {
                                ErrorLimitRemain = errorLimitRemain,
                                ErrorLimitReset = errorLimitReset,
                                DelayApplied = additionalDelay
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process rate limit headers");
            }
        }

        /// <summary>
        /// Gets the retry delay from Retry-After header
        /// </summary>
        private TimeSpan GetRetryAfterDelay(HttpResponseMessage response)
        {
            if (response.Headers.RetryAfter?.Delta.HasValue == true)
            {
                return response.Headers.RetryAfter.Delta.Value;
            }
            else if (response.Headers.RetryAfter?.Date.HasValue == true)
            {
                var delay = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                return delay > TimeSpan.Zero ? delay : TimeSpan.FromSeconds(1);
            }

            return TimeSpan.FromSeconds(1);
        }

        /// <summary>
        /// Calculates retry delay with optional exponential backoff
        /// </summary>
        private TimeSpan CalculateRetryDelay(int attempt)
        {
            if (!_config.UseExponentialBackoff)
                return _config.RetryDelay;

            var delayMs = _config.RetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
            return TimeSpan.FromMilliseconds(Math.Min(delayMs, 30000)); // Max 30 seconds
        }

        /// <summary>
        /// Clones an HTTP request for retry attempts
        /// </summary>
        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Version = original.Version
            };

            // Copy headers
            foreach (var header in original.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Copy content if present
            if (original.Content != null)
            {
                var content = await original.Content.ReadAsStringAsync();
                clone.Content = new StringContent(content, Encoding.UTF8, original.Content.Headers.ContentType?.MediaType ?? "application/json");

                foreach (var header in original.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }

        public void Dispose()
        {
            _rateLimitSemaphore?.Dispose();
            _httpClient?.Dispose();
        }
    }
} 