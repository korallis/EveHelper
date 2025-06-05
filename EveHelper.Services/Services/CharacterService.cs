using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Character;
using EveHelper.Core.Models.Esi;
using Microsoft.Extensions.Logging;

namespace EveHelper.Services.Services
{
    /// <summary>
    /// Service for managing character selection and authentication
    /// </summary>
    public class CharacterService : ICharacterService
    {
        private readonly ITokenStorageService _tokenStorage;
        private readonly IEsiClientService _esiClient;
        private readonly IEveAuthService _authService;
        private readonly ILogger<CharacterService> _logger;
        private readonly string _characterDataPath;
        private readonly SemaphoreSlim _characterDataSemaphore;

        public event EventHandler<CharacterListChangedEventArgs>? CharacterListChanged;
        public event EventHandler<DefaultCharacterChangedEventArgs>? DefaultCharacterChanged;

        public CharacterService(
            ITokenStorageService tokenStorage,
            IEsiClientService esiClient,
            IEveAuthService authService,
            ILogger<CharacterService> logger)
        {
            _tokenStorage = tokenStorage;
            _esiClient = esiClient;
            _authService = authService;
            _logger = logger;
            _characterDataSemaphore = new SemaphoreSlim(1, 1);

            // Set up character data storage path
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EveHelper");
            Directory.CreateDirectory(appDataPath);
            _characterDataPath = Path.Combine(appDataPath, "characters.json");
        }

        /// <summary>
        /// Gets all available characters
        /// </summary>
        public async Task<IEnumerable<CharacterInfo>> GetCharactersAsync()
        {
            try
            {
                var charactersEnum = await LoadCharacterDataAsync();
                var characters = charactersEnum.ToList();
                var storedCharacterIds = await _tokenStorage.GetStoredCharacterIdsAsync();

                // Update token validity for all characters
                foreach (var character in characters)
                {
                    character.HasValidToken = storedCharacterIds.Contains(character.CharacterId);
                    if (character.HasValidToken)
                    {
                        var token = await _tokenStorage.GetTokenAsync(character.CharacterId);
                        character.TokenExpiresAt = token?.ExpiresAt;
                    }
                }

                // Remove characters that no longer have tokens
                var validCharacters = characters.Where(c => c.HasValidToken).ToList();
                if (validCharacters.Count != characters.Count)
                {
                    await SaveCharacterDataAsync(validCharacters);
                }

                return validCharacters.OrderByDescending(c => c.IsDefault).ThenByDescending(c => c.LastAccessed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get characters");
                return new List<CharacterInfo>();
            }
        }

        /// <summary>
        /// Gets the default/selected character
        /// </summary>
        public async Task<CharacterInfo?> GetDefaultCharacterAsync()
        {
            try
            {
                var characters = await GetCharactersAsync();
                return characters.FirstOrDefault(c => c.IsDefault);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get default character");
                return null;
            }
        }

        /// <summary>
        /// Sets the default character
        /// </summary>
        public async Task<bool> SetDefaultCharacterAsync(long characterId)
        {
            try
            {
                await _characterDataSemaphore.WaitAsync();
                var characters = (await LoadCharacterDataAsync()).ToList();
                
                var previousDefault = characters.FirstOrDefault(c => c.IsDefault);
                var newDefault = characters.FirstOrDefault(c => c.CharacterId == characterId);

                if (newDefault == null)
                {
                    _logger.LogWarning("Character {CharacterId} not found when setting as default", characterId);
                    return false;
                }

                // Update default status
                foreach (var character in characters)
                {
                    character.IsDefault = character.CharacterId == characterId;
                }

                newDefault.LastAccessed = DateTime.UtcNow;
                await SaveCharacterDataAsync(characters);

                // Raise event
                DefaultCharacterChanged?.Invoke(this, new DefaultCharacterChangedEventArgs
                {
                    PreviousDefault = previousDefault,
                    NewDefault = newDefault
                });

                _logger.LogInformation("Default character set to {CharacterName} ({CharacterId})", 
                    newDefault.Name, characterId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set default character {CharacterId}", characterId);
                return false;
            }
            finally
            {
                _characterDataSemaphore.Release();
            }
        }

        /// <summary>
        /// Adds a new character through authentication
        /// </summary>
        public async Task<CharacterInfo?> AddCharacterAsync()
        {
            try
            {
                _logger.LogInformation("Starting character authentication process");

                // Start authentication process
                var authUrl = await _authService.StartAuthenticationAsync();
                
                // For now, we'll need to implement a proper UI flow for authentication
                // This is a placeholder - in a real implementation, this would involve UI interaction
                _logger.LogInformation("Authentication URL generated: {AuthUrl}", authUrl);
                
                // TODO: Implement proper authentication flow with UI
                // For now, we'll return null to indicate that authentication needs UI implementation
                _logger.LogWarning("Authentication requires UI implementation - not yet available");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add character");
                return null;
            }
        }

        /// <summary>
        /// Removes a character and its associated token
        /// </summary>
        public async Task<bool> RemoveCharacterAsync(long characterId)
        {
            try
            {
                await _characterDataSemaphore.WaitAsync();
                var characters = (await LoadCharacterDataAsync()).ToList();
                var characterToRemove = characters.FirstOrDefault(c => c.CharacterId == characterId);

                if (characterToRemove == null)
                {
                    _logger.LogWarning("Character {CharacterId} not found for removal", characterId);
                    return false;
                }

                // Remove token
                await _tokenStorage.RemoveTokenAsync(characterId);

                // Remove from character list
                characters.RemoveAll(c => c.CharacterId == characterId);

                // If this was the default character, set a new default
                if (characterToRemove.IsDefault && characters.Any())
                {
                    var newDefault = characters.OrderByDescending(c => c.LastAccessed).First();
                    newDefault.IsDefault = true;
                }

                await SaveCharacterDataAsync(characters);

                // Raise events
                CharacterListChanged?.Invoke(this, new CharacterListChangedEventArgs
                {
                    ChangeType = CharacterListChangeType.Removed,
                    Character = characterToRemove,
                    Characters = characters
                });

                if (characterToRemove.IsDefault)
                {
                    var newDefault = characters.FirstOrDefault(c => c.IsDefault);
                    DefaultCharacterChanged?.Invoke(this, new DefaultCharacterChangedEventArgs
                    {
                        PreviousDefault = characterToRemove,
                        NewDefault = newDefault
                    });
                }

                _logger.LogInformation("Character {CharacterName} ({CharacterId}) removed successfully", 
                    characterToRemove.Name, characterId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove character {CharacterId}", characterId);
                return false;
            }
            finally
            {
                _characterDataSemaphore.Release();
            }
        }

        /// <summary>
        /// Refreshes character information from ESI
        /// </summary>
        public async Task<CharacterInfo?> RefreshCharacterAsync(long characterId)
        {
            try
            {
                var esiCharacterInfo = await _esiClient.GetAsync<EsiCharacterInfo>($"/latest/characters/{characterId}/", characterId);
                if (esiCharacterInfo == null)
                {
                    _logger.LogWarning("Failed to refresh character information from ESI for character {CharacterId}", characterId);
                    return null;
                }

                await _characterDataSemaphore.WaitAsync();
                try
                {
                    var characters = (await LoadCharacterDataAsync()).ToList();
                    var existingCharacter = characters.FirstOrDefault(c => c.CharacterId == characterId);

                    if (existingCharacter == null)
                    {
                        _logger.LogWarning("Character {CharacterId} not found for refresh", characterId);
                        return null;
                    }

                    // Update character information
                    var updatedCharacter = CharacterInfo.FromEsiData(characterId, esiCharacterInfo);
                    updatedCharacter.IsDefault = existingCharacter.IsDefault;
                    updatedCharacter.LastAccessed = existingCharacter.LastAccessed;
                    updatedCharacter.HasValidToken = existingCharacter.HasValidToken;
                    updatedCharacter.TokenExpiresAt = existingCharacter.TokenExpiresAt;

                    // Replace in list
                    var index = characters.FindIndex(c => c.CharacterId == characterId);
                    characters[index] = updatedCharacter;

                    await SaveCharacterDataAsync(characters);

                    // Raise event
                    CharacterListChanged?.Invoke(this, new CharacterListChangedEventArgs
                    {
                        ChangeType = CharacterListChangeType.Updated,
                        Character = updatedCharacter,
                        Characters = characters
                    });

                    _logger.LogInformation("Character {CharacterName} ({CharacterId}) refreshed successfully", 
                        updatedCharacter.Name, characterId);

                    return updatedCharacter;
                }
                finally
                {
                    _characterDataSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh character {CharacterId}", characterId);
                return null;
            }
        }

        /// <summary>
        /// Refreshes all characters' information
        /// </summary>
        public async Task<IEnumerable<CharacterInfo>> RefreshAllCharactersAsync()
        {
            try
            {
                var characters = (await GetCharactersAsync()).ToList();
                var refreshedCharacters = new List<CharacterInfo>();

                foreach (var character in characters)
                {
                    var refreshed = await RefreshCharacterAsync(character.CharacterId);
                    if (refreshed != null)
                    {
                        refreshedCharacters.Add(refreshed);
                    }
                }

                // Raise event
                CharacterListChanged?.Invoke(this, new CharacterListChangedEventArgs
                {
                    ChangeType = CharacterListChangeType.Refreshed,
                    Characters = refreshedCharacters
                });

                _logger.LogInformation("Refreshed {Count} characters", refreshedCharacters.Count);
                return refreshedCharacters;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh all characters");
                return new List<CharacterInfo>();
            }
        }

        /// <summary>
        /// Validates that a character's token is still valid
        /// </summary>
        public async Task<bool> ValidateCharacterTokenAsync(long characterId)
        {
            try
            {
                return await _esiClient.ValidateCharacterTokenAsync(characterId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate token for character {CharacterId}", characterId);
                return false;
            }
        }

        /// <summary>
        /// Gets character information by ID
        /// </summary>
        public async Task<CharacterInfo?> GetCharacterAsync(long characterId)
        {
            try
            {
                var characters = await GetCharactersAsync();
                return characters.FirstOrDefault(c => c.CharacterId == characterId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get character {CharacterId}", characterId);
                return null;
            }
        }

        /// <summary>
        /// Loads character data from local storage
        /// </summary>
        private async Task<IEnumerable<CharacterInfo>> LoadCharacterDataAsync()
        {
            try
            {
                if (!File.Exists(_characterDataPath))
                {
                    return new List<CharacterInfo>();
                }

                var json = await File.ReadAllTextAsync(_characterDataPath);
                var characters = JsonSerializer.Deserialize<List<CharacterInfo>>(json);
                return characters ?? new List<CharacterInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load character data from {Path}", _characterDataPath);
                return new List<CharacterInfo>();
            }
        }

        /// <summary>
        /// Saves character data to local storage
        /// </summary>
        private async Task SaveCharacterDataAsync(IEnumerable<CharacterInfo> characters)
        {
            try
            {
                var json = JsonSerializer.Serialize(characters, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(_characterDataPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save character data to {Path}", _characterDataPath);
            }
        }
    }
} 