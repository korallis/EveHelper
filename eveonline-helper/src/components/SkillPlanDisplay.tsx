import React from 'react';
import { SkillPlanEntry } from '../hooks/useSkillPlan';

/**
 * Props for SkillPlanDisplay
 */
export interface SkillPlanDisplayProps {
  skillPlan: SkillPlanEntry[] | null;
  loading: boolean;
  error: string | null;
}

/**
 * Displays a prioritized skill plan for a recommended fit
 * @param skillPlan The skill plan array
 * @param loading Loading state
 * @param error Error message
 */
export const SkillPlanDisplay: React.FC<SkillPlanDisplayProps> = ({ skillPlan, loading, error }) => {
  if (loading) return <div>Loading skill plan...</div>;
  if (error) return <div style={{ color: 'red' }}>Error: {error}</div>;
  if (!skillPlan || skillPlan.length === 0) return <div>No missing or under-leveled skills for this fit!</div>;

  return (
    <table className="min-w-full border border-gray-300 mt-2">
      <thead>
        <tr>
          <th className="border px-2 py-1">Skill Name</th>
          <th className="border px-2 py-1">Required Level</th>
          <th className="border px-2 py-1">Current Level</th>
        </tr>
      </thead>
      <tbody>
        {skillPlan.map((entry) => (
          <tr key={entry.skill_id}>
            <td className="border px-2 py-1">{entry.skill_name}</td>
            <td className="border px-2 py-1">{entry.required_level}</td>
            <td className="border px-2 py-1">{entry.current_level}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}; 