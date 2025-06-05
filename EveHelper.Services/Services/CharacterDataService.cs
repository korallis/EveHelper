using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using EveHelper.Core.Interfaces;
using EveHelper.Core.Models.Character;
using EveHelper.Core.Models.Esi;
using Microsoft.Extensions.Logging;

namespace EveHelper.Services.Services
{
    /// <summary>
    /// Service for retrieving and managing character data from ESI
    /// </summary>
    public class CharacterDataService : ICharacterDataService
    {
        private readonly IEsiClientService _esiClient;
        private readonly ILogger<CharacterDataService> _logger;
        private readonly string _cacheDirectory;
        private readonly SemaphoreSlim _cacheSemaphore;
        
        // In-memory cache for frequently accessed data
        private readonly ConcurrentDictionary<string, CachedData> _memoryCache;
        private readonly TimeSpan _memoryCacheTimeout = TimeSpan.FromMinutes(5);

        public event EventHandler<CharacterDataUpdatedEventArgs>? CharacterDataUpdated;

        public CharacterDataService(
            IEsiClientService esiClient,
            ILogger<CharacterDataService> logger)
        {
            _esiClient = esiClient;
            _logger = logger;
            _cacheSemaphore = new SemaphoreSlim(1, 1);
            _memoryCache = new ConcurrentDictionary<string, CachedData>();

            // Set up cache directory
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EveHelper", "Cache");
            Directory.CreateDirectory(appDataPath);
            _cacheDirectory = appDataPath;

            _logger.LogInformation("CharacterDataService initialized with cache directory: {CacheDirectory}", _cacheDirectory);
        }

        /// <summary>
        /// Retrieves all skills for a character
        /// </summary>
        public async Task<IEnumerable<CharacterSkill>> GetCharacterSkillsAsync(long characterId, bool forceRefresh = false)
        {
            var cacheKey = $"skills_{characterId}";
            
            try
            {
                // Check memory cache first
                if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out var cachedData) && 
                    DateTime.UtcNow - cachedData.Timestamp < _memoryCacheTimeout)
                {
                    _logger.LogDebug("Returning skills from memory cache for character {CharacterId}", characterId);
                    return (IEnumerable<CharacterSkill>)cachedData.Data;
                }

                // Check file cache
                if (!forceRefresh)
                {
                    var fileCachedSkills = await LoadFromFileCache<List<CharacterSkill>>(cacheKey);
                    if (fileCachedSkills != null)
                    {
                        _logger.LogDebug("Returning skills from file cache for character {CharacterId}", characterId);
                        _memoryCache[cacheKey] = new CachedData(fileCachedSkills, DateTime.UtcNow);
                        return fileCachedSkills;
                    }
                }

                // Fetch from ESI
                _logger.LogInformation("Fetching skills from ESI for character {CharacterId}", characterId);
                var esiSkills = await _esiClient.GetAsync<EsiCharacterSkills>($"/latest/characters/{characterId}/skills/", characterId);
                
                if (esiSkills == null)
                {
                    _logger.LogWarning("Failed to fetch skills from ESI for character {CharacterId}", characterId);
                    RaiseDataUpdatedEvent(characterId, CharacterDataType.Skills, false, "Failed to fetch skills from ESI");
                    return new List<CharacterSkill>();
                }

                // Transform to application models
                var skills = esiSkills.Skills.Select(skill => new CharacterSkill
                {
                    SkillId = skill.SkillId,
                    ActiveSkillLevel = skill.ActiveSkillLevel,
                    SkillPointsInSkill = skill.SkillpointsInSkill,
                    TrainedSkillLevel = skill.TrainedSkillLevel,
                    IsCurrentlyTraining = skill.TrainedSkillLevel.HasValue,
                    // TODO: Populate skill names from static data
                    SkillName = $"Skill {skill.SkillId}",
                    GroupName = "Unknown Group"
                }).ToList();

                // Cache the results
                await SaveToFileCache(cacheKey, skills);
                _memoryCache[cacheKey] = new CachedData(skills, DateTime.UtcNow);

                RaiseDataUpdatedEvent(characterId, CharacterDataType.Skills, true);
                _logger.LogInformation("Successfully retrieved {SkillCount} skills for character {CharacterId}", skills.Count, characterId);
                
                return skills;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving skills for character {CharacterId}", characterId);
                RaiseDataUpdatedEvent(characterId, CharacterDataType.Skills, false, ex.Message);
                return new List<CharacterSkill>();
            }
        }

        /// <summary>
        /// Retrieves the skill queue for a character
        /// </summary>
        public async Task<IEnumerable<SkillQueueItem>> GetCharacterSkillQueueAsync(long characterId, bool forceRefresh = false)
        {
            var cacheKey = $"skillqueue_{characterId}";
            
            try
            {
                // Check memory cache first
                if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out var cachedData) && 
                    DateTime.UtcNow - cachedData.Timestamp < _memoryCacheTimeout)
                {
                    _logger.LogDebug("Returning skill queue from memory cache for character {CharacterId}", characterId);
                    return (IEnumerable<SkillQueueItem>)cachedData.Data;
                }

                // Check file cache
                if (!forceRefresh)
                {
                    var fileCachedQueue = await LoadFromFileCache<List<SkillQueueItem>>(cacheKey);
                    if (fileCachedQueue != null)
                    {
                        _logger.LogDebug("Returning skill queue from file cache for character {CharacterId}", characterId);
                        _memoryCache[cacheKey] = new CachedData(fileCachedQueue, DateTime.UtcNow);
                        return fileCachedQueue;
                    }
                }

                // Fetch from ESI
                _logger.LogInformation("Fetching skill queue from ESI for character {CharacterId}", characterId);
                var esiQueue = await _esiClient.GetAsync<EsiSkillQueue>($"/latest/characters/{characterId}/skillqueue/", characterId);
                
                if (esiQueue == null)
                {
                    _logger.LogWarning("Failed to fetch skill queue from ESI for character {CharacterId}", characterId);
                    RaiseDataUpdatedEvent(characterId, CharacterDataType.SkillQueue, false, "Failed to fetch skill queue from ESI");
                    return new List<SkillQueueItem>();
                }

                // Transform to application models
                var queueItems = esiQueue.OrderBy(q => q.QueuePosition).Select(item => new SkillQueueItem
                {
                    QueuePosition = item.QueuePosition,
                    SkillId = item.SkillId,
                    FinishedLevel = item.FinishedLevel,
                    TrainingStartSp = item.TrainingStartSp,
                    LevelEndSp = item.LevelEndSp,
                    StartDate = item.StartDate,
                    FinishDate = item.FinishDate,
                    IsCurrentlyTraining = item.QueuePosition == 0,
                    // TODO: Populate skill names from static data
                    SkillName = $"Skill {item.SkillId}",
                    GroupName = "Unknown Group"
                }).ToList();

                // Cache the results
                await SaveToFileCache(cacheKey, queueItems);
                _memoryCache[cacheKey] = new CachedData(queueItems, DateTime.UtcNow);

                RaiseDataUpdatedEvent(characterId, CharacterDataType.SkillQueue, true);
                _logger.LogInformation("Successfully retrieved {QueueCount} skill queue items for character {CharacterId}", queueItems.Count, characterId);
                
                return queueItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving skill queue for character {CharacterId}", characterId);
                RaiseDataUpdatedEvent(characterId, CharacterDataType.SkillQueue, false, ex.Message);
                return new List<SkillQueueItem>();
            }
        }

        /// <summary>
        /// Retrieves character attributes
        /// </summary>
        public async Task<CharacterAttributes?> GetCharacterAttributesAsync(long characterId, bool forceRefresh = false)
        {
            var cacheKey = $"attributes_{characterId}";
            
            try
            {
                // Check memory cache first
                if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out var cachedData) && 
                    DateTime.UtcNow - cachedData.Timestamp < _memoryCacheTimeout)
                {
                    _logger.LogDebug("Returning attributes from memory cache for character {CharacterId}", characterId);
                    return (CharacterAttributes)cachedData.Data;
                }

                // Check file cache
                if (!forceRefresh)
                {
                    var fileCachedAttributes = await LoadFromFileCache<CharacterAttributes>(cacheKey);
                    if (fileCachedAttributes != null)
                    {
                        _logger.LogDebug("Returning attributes from file cache for character {CharacterId}", characterId);
                        _memoryCache[cacheKey] = new CachedData(fileCachedAttributes, DateTime.UtcNow);
                        return fileCachedAttributes;
                    }
                }

                // Fetch from ESI
                _logger.LogInformation("Fetching attributes from ESI for character {CharacterId}", characterId);
                var esiAttributesDoc = await _esiClient.GetAsync<JsonDocument>($"/latest/characters/{characterId}/attributes/", characterId);
                
                if (esiAttributesDoc == null)
                {
                    _logger.LogWarning("Failed to fetch attributes from ESI for character {CharacterId}", characterId);
                    RaiseDataUpdatedEvent(characterId, CharacterDataType.Attributes, false, "Failed to fetch attributes from ESI");
                    return null;
                }

                var esiAttributes = esiAttributesDoc.RootElement;

                // Transform to application model
                var attributes = new CharacterAttributes
                {
                    Intelligence = esiAttributes.GetProperty("intelligence").GetInt32(),
                    Memory = esiAttributes.GetProperty("memory").GetInt32(),
                    Perception = esiAttributes.GetProperty("perception").GetInt32(),
                    Willpower = esiAttributes.GetProperty("willpower").GetInt32(),
                    Charisma = esiAttributes.GetProperty("charisma").GetInt32()
                };

                // Handle optional properties
                if (esiAttributes.TryGetProperty("bonus_remaps", out var bonusRemaps))
                {
                    attributes.BonusRemaps = bonusRemaps.GetInt32();
                }

                if (esiAttributes.TryGetProperty("last_remap_date", out var lastRemapDate))
                {
                    attributes.LastRemapDate = lastRemapDate.GetDateTime();
                }

                if (esiAttributes.TryGetProperty("accrued_remap_cooldown_date", out var cooldownDate))
                {
                    attributes.AccruedRemapCooldownDate = cooldownDate.GetInt32();
                }

                // Cache the results
                await SaveToFileCache(cacheKey, attributes);
                _memoryCache[cacheKey] = new CachedData(attributes, DateTime.UtcNow);

                RaiseDataUpdatedEvent(characterId, CharacterDataType.Attributes, true);
                _logger.LogInformation("Successfully retrieved attributes for character {CharacterId}", characterId);
                
                return attributes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attributes for character {CharacterId}", characterId);
                RaiseDataUpdatedEvent(characterId, CharacterDataType.Attributes, false, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Retrieves character implants
        /// </summary>
        public async Task<IEnumerable<int>> GetCharacterImplantsAsync(long characterId, bool forceRefresh = false)
        {
            var cacheKey = $"implants_{characterId}";
            
            try
            {
                // Check memory cache first
                if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out var cachedData) && 
                    DateTime.UtcNow - cachedData.Timestamp < _memoryCacheTimeout)
                {
                    _logger.LogDebug("Returning implants from memory cache for character {CharacterId}", characterId);
                    return (IEnumerable<int>)cachedData.Data;
                }

                // Check file cache
                if (!forceRefresh)
                {
                    var fileCachedImplants = await LoadFromFileCache<List<int>>(cacheKey);
                    if (fileCachedImplants != null)
                    {
                        _logger.LogDebug("Returning implants from file cache for character {CharacterId}", characterId);
                        _memoryCache[cacheKey] = new CachedData(fileCachedImplants, DateTime.UtcNow);
                        return fileCachedImplants;
                    }
                }

                // Fetch from ESI
                _logger.LogInformation("Fetching implants from ESI for character {CharacterId}", characterId);
                var esiImplants = await _esiClient.GetAsync<List<int>>($"/latest/characters/{characterId}/implants/", characterId);
                
                if (esiImplants == null)
                {
                    _logger.LogWarning("Failed to fetch implants from ESI for character {CharacterId}", characterId);
                    RaiseDataUpdatedEvent(characterId, CharacterDataType.Implants, false, "Failed to fetch implants from ESI");
                    return new List<int>();
                }

                // Cache the results
                await SaveToFileCache(cacheKey, esiImplants);
                _memoryCache[cacheKey] = new CachedData(esiImplants, DateTime.UtcNow);

                RaiseDataUpdatedEvent(characterId, CharacterDataType.Implants, true);
                _logger.LogInformation("Successfully retrieved {ImplantCount} implants for character {CharacterId}", esiImplants.Count, characterId);
                
                return esiImplants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving implants for character {CharacterId}", characterId);
                RaiseDataUpdatedEvent(characterId, CharacterDataType.Implants, false, ex.Message);
                return new List<int>();
            }
        }

        /// <summary>
        /// Retrieves a specific skill for a character
        /// </summary>
        public async Task<CharacterSkill?> GetCharacterSkillAsync(long characterId, int skillId, bool forceRefresh = false)
        {
            try
            {
                var skills = await GetCharacterSkillsAsync(characterId, forceRefresh);
                return skills.FirstOrDefault(s => s.SkillId == skillId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving skill {SkillId} for character {CharacterId}", skillId, characterId);
                return null;
            }
        }

        /// <summary>
        /// Retrieves character skill summary
        /// </summary>
        public async Task<CharacterSkillSummary?> GetCharacterSkillSummaryAsync(long characterId, bool forceRefresh = false)
        {
            try
            {
                // Get skills and skill queue
                var skillsTask = GetCharacterSkillsAsync(characterId, forceRefresh);
                var queueTask = GetCharacterSkillQueueAsync(characterId, forceRefresh);
                
                await Task.WhenAll(skillsTask, queueTask);
                
                var skills = await skillsTask;
                var queue = await queueTask;

                var skillsList = skills.ToList();
                var queueList = queue.ToList();

                // Calculate summary
                var summary = new CharacterSkillSummary
                {
                    TotalSkillPoints = skillsList.Sum(s => s.SkillPointsInSkill),
                    TotalSkills = skillsList.Count,
                    SkillsAtLevelV = skillsList.Count(s => s.ActiveSkillLevel == 5),
                    CurrentlyTraining = skillsList.FirstOrDefault(s => s.IsCurrentlyTraining),
                    SkillsInQueue = queueList.Count,
                    QueueTimeRemaining = queueList.LastOrDefault()?.FinishDate - DateTime.UtcNow
                };

                _logger.LogInformation("Generated skill summary for character {CharacterId}: {TotalSP} SP, {TotalSkills} skills", 
                    characterId, summary.TotalSkillPoints, summary.TotalSkills);

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating skill summary for character {CharacterId}", characterId);
                return null;
            }
        }

        /// <summary>
        /// Refreshes all character data
        /// </summary>
        public async Task<bool> RefreshAllCharacterDataAsync(long characterId)
        {
            try
            {
                _logger.LogInformation("Refreshing all data for character {CharacterId}", characterId);

                var tasks = new List<Task>
                {
                    GetCharacterSkillsAsync(characterId, true),
                    GetCharacterSkillQueueAsync(characterId, true),
                    GetCharacterAttributesAsync(characterId, true),
                    GetCharacterImplantsAsync(characterId, true)
                };

                await Task.WhenAll(tasks);

                RaiseDataUpdatedEvent(characterId, CharacterDataType.All, true);
                _logger.LogInformation("Successfully refreshed all data for character {CharacterId}", characterId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing all data for character {CharacterId}", characterId);
                RaiseDataUpdatedEvent(characterId, CharacterDataType.All, false, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Clears cached data for a character
        /// </summary>
        public async Task ClearCharacterCacheAsync(long characterId)
        {
            try
            {
                await _cacheSemaphore.WaitAsync();
                
                // Clear memory cache
                var keysToRemove = _memoryCache.Keys.Where(k => k.Contains($"_{characterId}")).ToList();
                foreach (var key in keysToRemove)
                {
                    _memoryCache.TryRemove(key, out _);
                }

                // Clear file cache
                var cacheFiles = new[]
                {
                    $"skills_{characterId}.json",
                    $"skillqueue_{characterId}.json",
                    $"attributes_{characterId}.json",
                    $"implants_{characterId}.json"
                };

                foreach (var fileName in cacheFiles)
                {
                    var filePath = Path.Combine(_cacheDirectory, fileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                _logger.LogInformation("Cleared cache for character {CharacterId}", characterId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache for character {CharacterId}", characterId);
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        /// <summary>
        /// Gets the last updated time for character data
        /// </summary>
        public Task<DateTime?> GetLastUpdatedAsync(long characterId, CharacterDataType dataType)
        {
            try
            {
                var cacheKey = dataType switch
                {
                    CharacterDataType.Skills => $"skills_{characterId}",
                    CharacterDataType.SkillQueue => $"skillqueue_{characterId}",
                    CharacterDataType.Attributes => $"attributes_{characterId}",
                    CharacterDataType.Implants => $"implants_{characterId}",
                    _ => null
                };

                if (cacheKey == null) return Task.FromResult<DateTime?>(null);

                var filePath = Path.Combine(_cacheDirectory, $"{cacheKey}.json");
                if (File.Exists(filePath))
                {
                    return Task.FromResult<DateTime?>(File.GetLastWriteTimeUtc(filePath));
                }

                return Task.FromResult<DateTime?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting last updated time for character {CharacterId}, data type {DataType}", characterId, dataType);
                return Task.FromResult<DateTime?>(null);
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Loads data from file cache
        /// </summary>
        private async Task<T?> LoadFromFileCache<T>(string cacheKey) where T : class
        {
            try
            {
                await _cacheSemaphore.WaitAsync();
                
                var filePath = Path.Combine(_cacheDirectory, $"{cacheKey}.json");
                if (!File.Exists(filePath))
                {
                    return null;
                }

                // Check if cache is still valid (1 hour)
                var lastWrite = File.GetLastWriteTimeUtc(filePath);
                if (DateTime.UtcNow - lastWrite > TimeSpan.FromHours(1))
                {
                    File.Delete(filePath);
                    return null;
                }

                var json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading from file cache for key {CacheKey}", cacheKey);
                return null;
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        /// <summary>
        /// Saves data to file cache
        /// </summary>
        private async Task SaveToFileCache<T>(string cacheKey, T data)
        {
            try
            {
                await _cacheSemaphore.WaitAsync();
                
                var filePath = Path.Combine(_cacheDirectory, $"{cacheKey}.json");
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error saving to file cache for key {CacheKey}", cacheKey);
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        /// <summary>
        /// Raises the character data updated event
        /// </summary>
        private void RaiseDataUpdatedEvent(long characterId, CharacterDataType dataType, bool isSuccess, string? errorMessage = null)
        {
            try
            {
                CharacterDataUpdated?.Invoke(this, new CharacterDataUpdatedEventArgs
                {
                    CharacterId = characterId,
                    DataType = dataType,
                    IsSuccess = isSuccess,
                    ErrorMessage = errorMessage,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error raising character data updated event");
            }
        }

        #endregion

        /// <summary>
        /// Cached data wrapper
        /// </summary>
        private class CachedData
        {
            public object Data { get; }
            public DateTime Timestamp { get; }

            public CachedData(object data, DateTime timestamp)
            {
                Data = data;
                Timestamp = timestamp;
            }
        }
    }
} 