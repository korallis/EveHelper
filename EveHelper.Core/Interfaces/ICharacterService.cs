using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EveHelper.Core.Models.Character;

namespace EveHelper.Core.Interfaces
{
    /// <summary>
    /// Interface for character management and selection
    /// </summary>
    public interface ICharacterService
    {
        /// <summary>
        /// Event raised when the character list changes
        /// </summary>
        event EventHandler<CharacterListChangedEventArgs>? CharacterListChanged;

        /// <summary>
        /// Event raised when the default character changes
        /// </summary>
        event EventHandler<DefaultCharacterChangedEventArgs>? DefaultCharacterChanged;

        /// <summary>
        /// Gets all available characters
        /// </summary>
        /// <returns>List of character information</returns>
        Task<IEnumerable<CharacterInfo>> GetCharactersAsync();

        /// <summary>
        /// Gets the default/selected character
        /// </summary>
        /// <returns>Default character or null if none selected</returns>
        Task<CharacterInfo?> GetDefaultCharacterAsync();

        /// <summary>
        /// Sets the default character
        /// </summary>
        /// <param name="characterId">Character ID to set as default</param>
        /// <returns>True if successful</returns>
        Task<bool> SetDefaultCharacterAsync(long characterId);

        /// <summary>
        /// Adds a new character through authentication
        /// </summary>
        /// <returns>Character information if successful, null otherwise</returns>
        Task<CharacterInfo?> AddCharacterAsync();

        /// <summary>
        /// Removes a character and its associated token
        /// </summary>
        /// <param name="characterId">Character ID to remove</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveCharacterAsync(long characterId);

        /// <summary>
        /// Refreshes character information from ESI
        /// </summary>
        /// <param name="characterId">Character ID to refresh</param>
        /// <returns>Updated character information or null if failed</returns>
        Task<CharacterInfo?> RefreshCharacterAsync(long characterId);

        /// <summary>
        /// Refreshes all characters' information
        /// </summary>
        /// <returns>Updated list of characters</returns>
        Task<IEnumerable<CharacterInfo>> RefreshAllCharactersAsync();

        /// <summary>
        /// Validates that a character's token is still valid
        /// </summary>
        /// <param name="characterId">Character ID to validate</param>
        /// <returns>True if token is valid</returns>
        Task<bool> ValidateCharacterTokenAsync(long characterId);

        /// <summary>
        /// Gets character information by ID
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <returns>Character information or null if not found</returns>
        Task<CharacterInfo?> GetCharacterAsync(long characterId);
    }

    /// <summary>
    /// Event args for character list changes
    /// </summary>
    public class CharacterListChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Type of change that occurred
        /// </summary>
        public CharacterListChangeType ChangeType { get; set; }

        /// <summary>
        /// Character that was affected by the change
        /// </summary>
        public CharacterInfo? Character { get; set; }

        /// <summary>
        /// Updated list of all characters
        /// </summary>
        public IEnumerable<CharacterInfo> Characters { get; set; } = new List<CharacterInfo>();
    }

    /// <summary>
    /// Event args for default character changes
    /// </summary>
    public class DefaultCharacterChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Previous default character
        /// </summary>
        public CharacterInfo? PreviousDefault { get; set; }

        /// <summary>
        /// New default character
        /// </summary>
        public CharacterInfo? NewDefault { get; set; }
    }

    /// <summary>
    /// Types of character list changes
    /// </summary>
    public enum CharacterListChangeType
    {
        /// <summary>
        /// Character was added
        /// </summary>
        Added,

        /// <summary>
        /// Character was removed
        /// </summary>
        Removed,

        /// <summary>
        /// Character information was updated
        /// </summary>
        Updated,

        /// <summary>
        /// All characters were refreshed
        /// </summary>
        Refreshed
    }
} 