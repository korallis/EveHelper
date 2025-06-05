using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CredentialManagement;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Authentication;
using EveHelper.Services.Helpers;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace EveHelper.Services.Services
{
    /// <summary>
    /// Service for secure token storage and management using Windows Credential Manager
    /// with encrypted file fallback and automatic refresh monitoring
    /// </summary>
    public class TokenStorageService : ITokenStorageService, IDisposable
    {
        private readonly IEveAuthService _authService;
        private readonly TokenStorageConfiguration _config;
        private readonly ILogger<TokenStorageService> _logger;
        private readonly Timer _monitoringTimer;
        private readonly SemaphoreSlim _refreshSemaphore;
        private readonly string _machineKey;
        private readonly string _fileStorageDirectory;

        public event EventHandler<TokenExpirationEventArgs>? TokenExpiring;
        public event EventHandler<TokenRefreshEventArgs>? TokenRefreshed;

        /// <summary>
        /// Initializes a new instance of the TokenStorageService
        /// </summary>
        public TokenStorageService(
            IEveAuthService authService,
            TokenStorageConfiguration config,
            ILogger<TokenStorageService> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _refreshSemaphore = new SemaphoreSlim(1, 1);
            _machineKey = EncryptionHelper.GetMachineKey();
            
            // Setup file storage directory
            _fileStorageDirectory = _config.FileStorageDirectory ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EveHelper", "Tokens");
            
            Directory.CreateDirectory(_fileStorageDirectory);

            // Setup monitoring timer
            _monitoringTimer = new Timer(_config.MonitoringInterval.TotalMilliseconds);
            _monitoringTimer.Elapsed += OnMonitoringTimerElapsed;
            _monitoringTimer.AutoReset = true;

            if (_config.AutoStartMonitoring)
            {
                StartTokenMonitoring();
            }

            if (_config.EnableLogging)
            {
                _logger.LogInformation("TokenStorageService initialized with {StorageType} storage and {Interval}ms monitoring",
                    _config.UseCredentialManager ? "CredentialManager" : "File", 
                    _config.MonitoringInterval.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Securely stores an EVE token for a character
        /// </summary>
        public async Task<bool> StoreTokenAsync(long characterId, EveToken token)
        {
            try
            {
                var tokenJson = JsonSerializer.Serialize(token);
                var targetName = $"{_config.CredentialTargetPrefix}{characterId}";

                // Try Windows Credential Manager first
                if (_config.UseCredentialManager)
                {
                    try
                    {
                        using var credential = new Credential
                        {
                            Target = targetName,
                            Username = characterId.ToString(),
                            Password = tokenJson,
                            Type = CredentialType.Generic,
                            PersistanceType = PersistanceType.LocalComputer
                        };

                        if (credential.Save())
                        {
                            if (_config.EnableLogging)
                                _logger.LogDebug("Token stored in Credential Manager for character {CharacterId}", characterId);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to store token in Credential Manager for character {CharacterId}", characterId);
                    }
                }

                // Fallback to encrypted file storage
                if (_config.UseFallbackFileStorage)
                {
                    return await StoreTokenInFileAsync(characterId, tokenJson);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store token for character {CharacterId}", characterId);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a stored token for a character
        /// </summary>
        public async Task<EveToken?> GetTokenAsync(long characterId)
        {
            try
            {
                var targetName = $"{_config.CredentialTargetPrefix}{characterId}";

                // Try Windows Credential Manager first
                if (_config.UseCredentialManager)
                {
                    try
                    {
                        using var credential = new Credential { Target = targetName };
                        if (credential.Load())
                        {
                            var token = JsonSerializer.Deserialize<EveToken>(credential.Password);
                            if (token != null)
                            {
                                if (_config.EnableLogging)
                                    _logger.LogDebug("Token retrieved from Credential Manager for character {CharacterId}", characterId);
                                return token;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to retrieve token from Credential Manager for character {CharacterId}", characterId);
                    }
                }

                // Fallback to encrypted file storage
                if (_config.UseFallbackFileStorage)
                {
                    return await GetTokenFromFileAsync(characterId);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve token for character {CharacterId}", characterId);
                return null;
            }
        }

        /// <summary>
        /// Removes a stored token for a character
        /// </summary>
        public async Task<bool> RemoveTokenAsync(long characterId)
        {
            try
            {
                var targetName = $"{_config.CredentialTargetPrefix}{characterId}";
                var removed = false;

                // Remove from Credential Manager
                if (_config.UseCredentialManager)
                {
                    try
                    {
                        using var credential = new Credential { Target = targetName };
                        if (credential.Delete())
                        {
                            removed = true;
                            if (_config.EnableLogging)
                                _logger.LogDebug("Token removed from Credential Manager for character {CharacterId}", characterId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to remove token from Credential Manager for character {CharacterId}", characterId);
                    }
                }

                // Remove from file storage
                if (_config.UseFallbackFileStorage)
                {
                    if (await RemoveTokenFromFileAsync(characterId))
                    {
                        removed = true;
                    }
                }

                return removed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove token for character {CharacterId}", characterId);
                return false;
            }
        }

        /// <summary>
        /// Gets all stored character IDs
        /// </summary>
        public Task<long[]> GetStoredCharacterIdsAsync()
        {
            var characterIds = new HashSet<long>();

            // Get from Credential Manager
            // Note: CredentialManager package doesn't support enumeration, 
            // so we rely on file storage for listing stored character IDs
            if (_config.UseCredentialManager)
            {
                // We cannot enumerate credentials from Windows Credential Manager easily
                // This functionality will be provided by the file storage fallback
                if (_config.EnableLogging)
                    _logger.LogDebug("Credential Manager enumeration not available, using file storage for character ID listing");
            }

            // Get from file storage
            if (_config.UseFallbackFileStorage)
            {
                try
                {
                    var files = Directory.GetFiles(_fileStorageDirectory, "token_*.dat");
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        var idString = fileName.Substring("token_".Length);
                        if (long.TryParse(idString, out var characterId))
                        {
                            characterIds.Add(characterId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to enumerate token files");
                }
            }

            return Task.FromResult(characterIds.ToArray());
        }

        /// <summary>
        /// Checks if a token exists and is valid for a character
        /// </summary>
        public async Task<bool> HasValidTokenAsync(long characterId)
        {
            var token = await GetTokenAsync(characterId);
            return token != null && token.ExpiresAt > DateTime.UtcNow.AddMinutes(1);
        }

        /// <summary>
        /// Refreshes a token if it's close to expiration
        /// </summary>
        public async Task<EveToken?> RefreshTokenIfNeededAsync(long characterId)
        {
            var token = await GetTokenAsync(characterId);
            if (token == null)
                return null;

            var timeUntilExpiration = token.ExpiresAt - DateTime.UtcNow;
            if (timeUntilExpiration > _config.RefreshThreshold)
                return token; // Token is still valid

            // Token needs refresh
            await _refreshSemaphore.WaitAsync();
            try
            {
                // Check again in case another thread refreshed it
                token = await GetTokenAsync(characterId);
                if (token == null)
                    return null;

                timeUntilExpiration = token.ExpiresAt - DateTime.UtcNow;
                if (timeUntilExpiration > _config.RefreshThreshold)
                    return token;

                // Trigger expiration event
                TokenExpiring?.Invoke(this, new TokenExpirationEventArgs
                {
                    CharacterId = characterId,
                    Token = token,
                    TimeUntilExpiration = timeUntilExpiration
                });

                // Attempt refresh with retries
                for (int attempt = 1; attempt <= _config.MaxRefreshRetries; attempt++)
                {
                    try
                    {
                        var refreshedToken = await _authService.RefreshTokenAsync(token.RefreshToken);
                        if (refreshedToken != null)
                        {
                            await StoreTokenAsync(characterId, refreshedToken);
                            
                            TokenRefreshed?.Invoke(this, new TokenRefreshEventArgs
                            {
                                CharacterId = characterId,
                                Success = true,
                                NewToken = refreshedToken!
                            });

                            if (_config.EnableLogging)
                                _logger.LogInformation("Token refreshed successfully for character {CharacterId} on attempt {Attempt}", 
                                    characterId, attempt);

                            return refreshedToken;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Token refresh attempt {Attempt} failed for character {CharacterId}", 
                            attempt, characterId);

                        if (attempt < _config.MaxRefreshRetries)
                        {
                            await Task.Delay(_config.RetryDelay);
                        }
                    }
                }

                // All refresh attempts failed
                TokenRefreshed?.Invoke(this, new TokenRefreshEventArgs
                {
                    CharacterId = characterId,
                    Success = false,
                    ErrorMessage = "All refresh attempts failed"
                });

                return null;
            }
            finally
            {
                _refreshSemaphore.Release();
            }
        }

        /// <summary>
        /// Clears all stored tokens
        /// </summary>
        public async Task<bool> ClearAllTokensAsync()
        {
            try
            {
                var characterIds = await GetStoredCharacterIdsAsync();
                var allRemoved = true;

                foreach (var characterId in characterIds)
                {
                    if (!await RemoveTokenAsync(characterId))
                    {
                        allRemoved = false;
                    }
                }

                if (_config.EnableLogging)
                    _logger.LogInformation("Cleared {Count} stored tokens", characterIds.Length);

                return allRemoved;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear all tokens");
                return false;
            }
        }

        /// <summary>
        /// Starts the automatic token refresh monitoring
        /// </summary>
        public void StartTokenMonitoring()
        {
            _monitoringTimer.Start();
            if (_config.EnableLogging)
                _logger.LogInformation("Token monitoring started");
        }

        /// <summary>
        /// Stops the automatic token refresh monitoring
        /// </summary>
        public void StopTokenMonitoring()
        {
            _monitoringTimer.Stop();
            if (_config.EnableLogging)
                _logger.LogInformation("Token monitoring stopped");
        }

        #region Private Methods

        /// <summary>
        /// Stores token in encrypted file
        /// </summary>
        private async Task<bool> StoreTokenInFileAsync(long characterId, string tokenJson)
        {
            try
            {
                var fileName = Path.Combine(_fileStorageDirectory, $"token_{characterId}.dat");
                var encryptedData = EncryptionHelper.Encrypt(tokenJson, _machineKey, _config.EncryptionIterations);
                
                await File.WriteAllTextAsync(fileName, encryptedData);
                
                if (_config.EnableLogging)
                    _logger.LogDebug("Token stored in encrypted file for character {CharacterId}", characterId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store token in file for character {CharacterId}", characterId);
                return false;
            }
        }

        /// <summary>
        /// Retrieves token from encrypted file
        /// </summary>
        private async Task<EveToken?> GetTokenFromFileAsync(long characterId)
        {
            try
            {
                var fileName = Path.Combine(_fileStorageDirectory, $"token_{characterId}.dat");
                if (!File.Exists(fileName))
                    return null;

                var encryptedData = await File.ReadAllTextAsync(fileName);
                var tokenJson = EncryptionHelper.Decrypt(encryptedData, _machineKey, _config.EncryptionIterations);
                var token = JsonSerializer.Deserialize<EveToken>(tokenJson);

                if (_config.EnableLogging)
                    _logger.LogDebug("Token retrieved from encrypted file for character {CharacterId}", characterId);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve token from file for character {CharacterId}", characterId);
                return null;
            }
        }

        /// <summary>
        /// Removes token file
        /// </summary>
        private Task<bool> RemoveTokenFromFileAsync(long characterId)
        {
            try
            {
                var fileName = Path.Combine(_fileStorageDirectory, $"token_{characterId}.dat");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                    if (_config.EnableLogging)
                        _logger.LogDebug("Token file removed for character {CharacterId}", characterId);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove token file for character {CharacterId}", characterId);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Monitoring timer elapsed event handler
        /// </summary>
        private async void OnMonitoringTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                var characterIds = await GetStoredCharacterIdsAsync();
                
                foreach (var characterId in characterIds)
                {
                    // Fire and forget token refresh check
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await RefreshTokenIfNeededAsync(characterId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error during token monitoring for character {CharacterId}", characterId);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token monitoring cycle");
            }
        }

        #endregion

        /// <summary>
        /// Disposes the service and stops monitoring
        /// </summary>
        public void Dispose()
        {
            _monitoringTimer?.Stop();
            _monitoringTimer?.Dispose();
            _refreshSemaphore?.Dispose();
        }
    }
} 