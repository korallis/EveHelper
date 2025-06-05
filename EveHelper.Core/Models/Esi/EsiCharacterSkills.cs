using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EveHelper.Core.Models.Esi
{
    /// <summary>
    /// ESI response for character skills
    /// </summary>
    public class EsiCharacterSkills
    {
        /// <summary>
        /// List of character skills
        /// </summary>
        [JsonPropertyName("skills")]
        public List<EsiSkill> Skills { get; set; } = new List<EsiSkill>();

        /// <summary>
        /// Total skill points
        /// </summary>
        [JsonPropertyName("total_sp")]
        public long TotalSp { get; set; }

        /// <summary>
        /// Unallocated skill points
        /// </summary>
        [JsonPropertyName("unallocated_sp")]
        public int? UnallocatedSp { get; set; }
    }

    /// <summary>
    /// Individual skill from ESI
    /// </summary>
    public class EsiSkill
    {
        /// <summary>
        /// Active skill level
        /// </summary>
        [JsonPropertyName("active_skill_level")]
        public int ActiveSkillLevel { get; set; }

        /// <summary>
        /// Skill ID
        /// </summary>
        [JsonPropertyName("skill_id")]
        public int SkillId { get; set; }

        /// <summary>
        /// Current skill points
        /// </summary>
        [JsonPropertyName("skillpoints_in_skill")]
        public long SkillpointsInSkill { get; set; }

        /// <summary>
        /// Level being trained to (if training)
        /// </summary>
        [JsonPropertyName("trained_skill_level")]
        public int? TrainedSkillLevel { get; set; }
    }
} 