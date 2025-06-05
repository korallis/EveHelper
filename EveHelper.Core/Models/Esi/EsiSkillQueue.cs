using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EveHelper.Core.Models.Esi
{
    /// <summary>
    /// Collection of skill queue items from ESI
    /// </summary>
    public class EsiSkillQueue : List<EsiSkillQueueItem>
    {
    }

    /// <summary>
    /// Individual skill queue item from ESI
    /// </summary>
    public class EsiSkillQueueItem
    {
        /// <summary>
        /// Finish date for the skill queue item
        /// </summary>
        [JsonPropertyName("finish_date")]
        public DateTime? FinishDate { get; set; }

        /// <summary>
        /// Level the skill will be at when training is complete
        /// </summary>
        [JsonPropertyName("finished_level")]
        public int FinishedLevel { get; set; }

        /// <summary>
        /// Amount of SP that was in the skill when training started
        /// </summary>
        [JsonPropertyName("level_end_sp")]
        public int LevelEndSp { get; set; }

        /// <summary>
        /// Position in the skill queue
        /// </summary>
        [JsonPropertyName("queue_position")]
        public int QueuePosition { get; set; }

        /// <summary>
        /// Skill ID
        /// </summary>
        [JsonPropertyName("skill_id")]
        public int SkillId { get; set; }

        /// <summary>
        /// Start date for the skill queue item
        /// </summary>
        [JsonPropertyName("start_date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Amount of SP the skill had at the start of training
        /// </summary>
        [JsonPropertyName("training_start_sp")]
        public int TrainingStartSp { get; set; }
    }
} 