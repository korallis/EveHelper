using System;

namespace EveHelper.Core.Models.Character
{
    /// <summary>
    /// Represents an item in a character's skill queue
    /// </summary>
    public class SkillQueueItem
    {
        /// <summary>
        /// Position in the skill queue (0-based)
        /// </summary>
        public int QueuePosition { get; set; }

        /// <summary>
        /// Skill ID being trained
        /// </summary>
        public int SkillId { get; set; }

        /// <summary>
        /// Skill level being trained to
        /// </summary>
        public int FinishedLevel { get; set; }

        /// <summary>
        /// Skill level the skill is starting from
        /// </summary>
        public int TrainingStartSp { get; set; }

        /// <summary>
        /// Skill level (in SP) the skill will finish at
        /// </summary>
        public int LevelEndSp { get; set; }

        /// <summary>
        /// When training on this skill will start
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// When training on this skill will finish
        /// </summary>
        public DateTime? FinishDate { get; set; }

        /// <summary>
        /// Skill name (populated from static data)
        /// </summary>
        public string SkillName { get; set; } = string.Empty;

        /// <summary>
        /// Skill group name (populated from static data)
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is the currently training skill
        /// </summary>
        public bool IsCurrentlyTraining { get; set; }

        /// <summary>
        /// Time remaining for this skill to complete training
        /// </summary>
        public TimeSpan? TimeRemaining
        {
            get
            {
                if (FinishDate.HasValue && IsCurrentlyTraining)
                {
                    var remaining = FinishDate.Value - DateTime.UtcNow;
                    return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
                }
                return null;
            }
        }

        /// <summary>
        /// Total training time for this skill
        /// </summary>
        public TimeSpan? TotalTrainingTime
        {
            get
            {
                if (StartDate.HasValue && FinishDate.HasValue)
                {
                    return FinishDate.Value - StartDate.Value;
                }
                return null;
            }
        }

        /// <summary>
        /// Progress through this skill (0.0 to 1.0)
        /// </summary>
        public double TrainingProgress
        {
            get
            {
                if (!IsCurrentlyTraining || !StartDate.HasValue || !FinishDate.HasValue)
                    return 0.0;

                var totalTime = FinishDate.Value - StartDate.Value;
                var elapsedTime = DateTime.UtcNow - StartDate.Value;

                if (totalTime.TotalSeconds <= 0)
                    return 1.0;

                var progress = elapsedTime.TotalSeconds / totalTime.TotalSeconds;
                return Math.Max(0.0, Math.Min(1.0, progress));
            }
        }
    }
} 