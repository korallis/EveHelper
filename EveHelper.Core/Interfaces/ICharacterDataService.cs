using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EveHelper.Core.Models.Character;

namespace EveHelper.Core.Interfaces
{
    /// <summary>
    /// Interface for character data retrieval services
    /// </summary>
    public interface ICharacterDataService
    {
        /// <summary>
        /// Event raised when character data is updated
        /// </summary>
        event EventHandler<CharacterDataUpdatedEventArgs>? CharacterDataUpdated;

        /// <summary>
        /// Retrieves all skills for a character
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="forceRefresh">Force refresh from ESI even if cached</param>
        /// <returns>List of character skills</returns>
        Task<IEnumerable<CharacterSkill>> GetCharacterSkillsAsync(long characterId, bool forceRefresh = false);

        /// <summary>
        /// Retrieves the skill queue for a character
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="forceRefresh">Force refresh from ESI even if cached</param>
        /// <returns>List of skill queue items</returns>
        Task<IEnumerable<SkillQueueItem>> GetCharacterSkillQueueAsync(long characterId, bool forceRefresh = false);

        /// <summary>
        /// Retrieves character attributes
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="forceRefresh">Force refresh from ESI even if cached</param>
        /// <returns>Character attributes</returns>
        Task<CharacterAttributes?> GetCharacterAttributesAsync(long characterId, bool forceRefresh = false);

        /// <summary>
        /// Retrieves character implants
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="forceRefresh">Force refresh from ESI even if cached</param>
        /// <returns>List of implant type IDs</returns>
        Task<IEnumerable<int>> GetCharacterImplantsAsync(long characterId, bool forceRefresh = false);

        /// <summary>
        /// Retrieves a specific skill for a character
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="skillId">Skill ID</param>
        /// <param name="forceRefresh">Force refresh from ESI even if cached</param>
        /// <returns>Character skill or null if not trained</returns>
        Task<CharacterSkill?> GetCharacterSkillAsync(long characterId, int skillId, bool forceRefresh = false);

        /// <summary>
        /// Retrieves character skill summary (total SP, unallocated SP)
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="forceRefresh">Force refresh from ESI even if cached</param>
        /// <returns>Skill summary information</returns>
        Task<CharacterSkillSummary?> GetCharacterSkillSummaryAsync(long characterId, bool forceRefresh = false);

        /// <summary>
        /// Refreshes all character data
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <returns>True if successful</returns>
        Task<bool> RefreshAllCharacterDataAsync(long characterId);

        /// <summary>
        /// Clears cached data for a character
        /// </summary>
        /// <param name="characterId">Character ID</param>
        Task ClearCharacterCacheAsync(long characterId);

        /// <summary>
        /// Gets the last updated time for character data
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="dataType">Type of data to check</param>
        /// <returns>Last updated time or null if never updated</returns>
        Task<DateTime?> GetLastUpdatedAsync(long characterId, CharacterDataType dataType);
    }

    /// <summary>
    /// Event args for character data updates
    /// </summary>
    public class CharacterDataUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Character ID that was updated
        /// </summary>
        public long CharacterId { get; set; }

        /// <summary>
        /// Type of data that was updated
        /// </summary>
        public CharacterDataType DataType { get; set; }

        /// <summary>
        /// Whether the update was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if update failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// When the data was updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Types of character data
    /// </summary>
    public enum CharacterDataType
    {
        /// <summary>
        /// Character skills
        /// </summary>
        Skills,

        /// <summary>
        /// Skill queue
        /// </summary>
        SkillQueue,

        /// <summary>
        /// Character attributes
        /// </summary>
        Attributes,

        /// <summary>
        /// Character implants
        /// </summary>
        Implants,

        /// <summary>
        /// All character data
        /// </summary>
        All
    }

    /// <summary>
    /// Character attributes from ESI
    /// </summary>
    public class CharacterAttributes
    {
        /// <summary>
        /// Intelligence attribute
        /// </summary>
        public int Intelligence { get; set; }

        /// <summary>
        /// Memory attribute
        /// </summary>
        public int Memory { get; set; }

        /// <summary>
        /// Perception attribute
        /// </summary>
        public int Perception { get; set; }

        /// <summary>
        /// Willpower attribute
        /// </summary>
        public int Willpower { get; set; }

        /// <summary>
        /// Charisma attribute
        /// </summary>
        public int Charisma { get; set; }

        /// <summary>
        /// Bonus remaps available
        /// </summary>
        public int? BonusRemaps { get; set; }

        /// <summary>
        /// When attributes can be remapped again
        /// </summary>
        public DateTime? LastRemapDate { get; set; }

        /// <summary>
        /// Accumulated remap cooldown time
        /// </summary>
        public int? AccruedRemapCooldownDate { get; set; }
    }

    /// <summary>
    /// Summary of character skills
    /// </summary>
    public class CharacterSkillSummary
    {
        /// <summary>
        /// Total skill points
        /// </summary>
        public long TotalSkillPoints { get; set; }

        /// <summary>
        /// Unallocated skill points
        /// </summary>
        public int UnallocatedSkillPoints { get; set; }

        /// <summary>
        /// Number of skills trained
        /// </summary>
        public int TotalSkills { get; set; }

        /// <summary>
        /// Number of skills at level V
        /// </summary>
        public int SkillsAtLevelV { get; set; }

        /// <summary>
        /// Currently training skill
        /// </summary>
        public CharacterSkill? CurrentlyTraining { get; set; }

        /// <summary>
        /// Skills in queue
        /// </summary>
        public int SkillsInQueue { get; set; }

        /// <summary>
        /// Total time remaining in skill queue
        /// </summary>
        public TimeSpan? QueueTimeRemaining { get; set; }
    }
} 