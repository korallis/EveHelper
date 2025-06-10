import React from 'react';
import { SkillPlanEntry } from '../hooks/useSkillPlan';

// Accessible status icons
const CheckIcon = () => (
  <span role="img" aria-label="Skill complete" className="text-green-700 mr-1">✔️</span>
);
const ClockIcon = () => (
  <span role="img" aria-label="Skill in progress" className="text-blue-700 mr-1">⏳</span>
);

/**
 * Props for SkillPlanTimeline
 */
export interface SkillPlanTimelineProps {
  skillPlan: SkillPlanEntry[];
  trainingTimes?: { [skill_id: number]: string };
}

/**
 * Visualizes the skill plan as a vertical timeline
 * @accessibility
 * - Status is shown with both color and icon
 * - Sufficient color contrast for all elements
 * - Semantic markup and ARIA labels for screen readers
 */
export const SkillPlanTimeline: React.FC<SkillPlanTimelineProps> = ({ skillPlan, trainingTimes }) => {
  if (!skillPlan || skillPlan.length === 0) {
    return <div>No skill plan to visualize.</div>;
  }

  return (
    <div className="flex flex-col gap-4 mt-4" aria-label="Skill plan timeline">
      {skillPlan.map((entry, idx) => {
        const complete = entry.current_level >= entry.required_level;
        const percent = Math.min(100, (entry.current_level / entry.required_level) * 100);
        return (
          <div
            key={entry.skill_id}
            className="flex items-center gap-4"
            tabIndex={0}
            aria-label={`${entry.skill_name}: Required Level ${entry.required_level}, Current Level ${entry.current_level}${complete ? ' (complete)' : ''}`}
            // If you add tooltips or popovers, manage focus here
          >
            {/* Timeline marker with icon */}
            <div className="flex flex-col items-center">
              <div className={`w-4 h-4 rounded-full flex items-center justify-center border-2 border-gray-700 ${complete ? 'bg-green-500' : 'bg-gray-400'}`}
                aria-label={complete ? 'Skill complete' : 'Skill in progress'}
                role="img"
              >
                {complete ? <CheckIcon /> : <ClockIcon />}
              </div>
              {idx < skillPlan.length - 1 && <div className="w-1 h-8 bg-gray-300"></div>}
            </div>
            {/* Skill info */}
            <div className="flex-1">
              <div className="font-semibold">{entry.skill_name}</div>
              <div className="text-sm text-gray-600">
                Required: Level {entry.required_level} | Current: Level {entry.current_level}
                {trainingTimes && trainingTimes[entry.skill_id] && (
                  <span className="ml-2 text-xs text-blue-600">
                    (Est. {trainingTimes[entry.skill_id]})
                  </span>
                )}
              </div>
              <div className="w-full bg-gray-200 rounded h-2 mt-1" aria-label="Skill progress bar">
                <div
                  className={`h-2 rounded ${complete ? 'bg-green-500' : 'bg-blue-400'}`}
                  style={{ width: `${percent}%` }}
                ></div>
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}; 