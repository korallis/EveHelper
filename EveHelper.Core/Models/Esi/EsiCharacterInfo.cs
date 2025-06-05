using System;
using System.Text.Json.Serialization;

namespace EveHelper.Core.Models.Esi
{
    /// <summary>
    /// Represents character information from ESI
    /// </summary>
    public class EsiCharacterInfo
    {
        /// <summary>
        /// Character name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Character description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Corporation ID
        /// </summary>
        [JsonPropertyName("corporation_id")]
        public int CorporationId { get; set; }

        /// <summary>
        /// Alliance ID (if applicable)
        /// </summary>
        [JsonPropertyName("alliance_id")]
        public int? AllianceId { get; set; }

        /// <summary>
        /// Character birthday
        /// </summary>
        [JsonPropertyName("birthday")]
        public DateTime Birthday { get; set; }

        /// <summary>
        /// Character gender
        /// </summary>
        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Race ID
        /// </summary>
        [JsonPropertyName("race_id")]
        public int RaceId { get; set; }

        /// <summary>
        /// Bloodline ID
        /// </summary>
        [JsonPropertyName("bloodline_id")]
        public int BloodlineId { get; set; }

        /// <summary>
        /// Ancestry ID
        /// </summary>
        [JsonPropertyName("ancestry_id")]
        public int AncestryId { get; set; }

        /// <summary>
        /// Security status
        /// </summary>
        [JsonPropertyName("security_status")]
        public double? SecurityStatus { get; set; }

        /// <summary>
        /// Character title (if applicable)
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }
} 