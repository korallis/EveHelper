using System;

namespace EveHelper.Core.Models.Character
{
    /// <summary>
    /// Represents a character's skill
    /// </summary>
    public class CharacterSkill
    {
        /// <summary>
        /// Skill ID from the EVE skill database
        /// </summary>
        public int SkillId { get; set; }

        /// <summary>
        /// Current skill level (0-5)
        /// </summary>
        public int ActiveSkillLevel { get; set; }

        /// <summary>
        /// Current skill points in this skill
        /// </summary>
        public long SkillPointsInSkill { get; set; }

        /// <summary>
        /// Skill level currently being trained (if any)
        /// </summary>
        public int? TrainedSkillLevel { get; set; }

        /// <summary>
        /// Skill name (populated from static data)
        /// </summary>
        public string SkillName { get; set; } = string.Empty;

        /// <summary>
        /// Skill group ID
        /// </summary>
        public int? GroupId { get; set; }

        /// <summary>
        /// Skill group name (populated from static data)
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this skill is currently being trained
        /// </summary>
        public bool IsCurrentlyTraining { get; set; }

        /// <summary>
        /// Skill points required for next level
        /// </summary>
        public long? SkillPointsForNextLevel { get; set; }

        /// <summary>
        /// Progress towards next level (0.0 to 1.0)
        /// </summary>
        public double ProgressToNextLevel { get; set; }
    }
} 