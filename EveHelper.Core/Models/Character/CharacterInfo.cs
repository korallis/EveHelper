using System;

namespace EveHelper.Core.Models.Character
{
    /// <summary>
    /// Represents character information for the character selection interface
    /// </summary>
    public class CharacterInfo
    {
        /// <summary>
        /// Character ID
        /// </summary>
        public long CharacterId { get; set; }

        /// <summary>
        /// Character name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Character description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Corporation ID
        /// </summary>
        public int CorporationId { get; set; }

        /// <summary>
        /// Corporation name
        /// </summary>
        public string CorporationName { get; set; } = string.Empty;

        /// <summary>
        /// Alliance ID (if applicable)
        /// </summary>
        public int? AllianceId { get; set; }

        /// <summary>
        /// Alliance name (if applicable)
        /// </summary>
        public string? AllianceName { get; set; }

        /// <summary>
        /// Character birthday
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// Character gender
        /// </summary>
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Race ID
        /// </summary>
        public int RaceId { get; set; }

        /// <summary>
        /// Race name
        /// </summary>
        public string RaceName { get; set; } = string.Empty;

        /// <summary>
        /// Security status
        /// </summary>
        public double? SecurityStatus { get; set; }

        /// <summary>
        /// Character portrait URL (64x64)
        /// </summary>
        public string PortraitUrl64 { get; set; } = string.Empty;

        /// <summary>
        /// Character portrait URL (128x128)
        /// </summary>
        public string PortraitUrl128 { get; set; } = string.Empty;

        /// <summary>
        /// Character portrait URL (256x256)
        /// </summary>
        public string PortraitUrl256 { get; set; } = string.Empty;

        /// <summary>
        /// Whether this character has a valid authentication token
        /// </summary>
        public bool HasValidToken { get; set; }

        /// <summary>
        /// When the token expires (if available)
        /// </summary>
        public DateTime? TokenExpiresAt { get; set; }

        /// <summary>
        /// Whether this is the default/selected character
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// When this character was last accessed
        /// </summary>
        public DateTime LastAccessed { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a character info from ESI character data
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="esiCharacterInfo">ESI character information</param>
        /// <returns>Character info instance</returns>
        public static CharacterInfo FromEsiData(long characterId, Models.Esi.EsiCharacterInfo esiCharacterInfo)
        {
            return new CharacterInfo
            {
                CharacterId = characterId,
                Name = esiCharacterInfo.Name,
                Description = esiCharacterInfo.Description,
                CorporationId = esiCharacterInfo.CorporationId,
                AllianceId = esiCharacterInfo.AllianceId,
                Birthday = esiCharacterInfo.Birthday,
                Gender = esiCharacterInfo.Gender,
                RaceId = esiCharacterInfo.RaceId,
                SecurityStatus = esiCharacterInfo.SecurityStatus,
                PortraitUrl64 = $"https://images.evetech.net/characters/{characterId}/portrait?size=64",
                PortraitUrl128 = $"https://images.evetech.net/characters/{characterId}/portrait?size=128",
                PortraitUrl256 = $"https://images.evetech.net/characters/{characterId}/portrait?size=256",
                HasValidToken = false, // Will be set by the service
                LastAccessed = DateTime.UtcNow
            };
        }
    }
} 