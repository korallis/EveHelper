import React, { useState } from 'react';
import { SkillPlanEntry, exportSkillPlanEvemon } from '../hooks/useSkillPlan';

/**
 * Props for SkillPlanExportButton
 */
export interface SkillPlanExportButtonProps {
  skillPlan: SkillPlanEntry[];
}

/**
 * Button to export a skill plan as an EVEMon-compatible .txt file
 * @param skillPlan The skill plan array
 * @accessibility
 * - Button is focusable and can be activated by keyboard (Enter/Space)
 * - aria-label describes the action for screen readers
 * - Error message uses role="status" for live updates
 */
export const SkillPlanExportButton: React.FC<SkillPlanExportButtonProps> = ({ skillPlan }) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleExport = async () => {
    setLoading(true);
    setError(null);
    try {
      const evemonText = await exportSkillPlanEvemon(skillPlan);
      const blob = new Blob([evemonText], { type: 'text/plain' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'skill-plan-evemon.txt';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
    } catch (err: any) {
      setError(err.message || 'Failed to export skill plan');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="mt-2">
      <button
        className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 focus:outline-none focus:ring-2 focus:ring-blue-400"
        aria-label="Export skill plan to EVEMon"
        onClick={handleExport}
        disabled={loading || skillPlan.length === 0}
        // Button is keyboard accessible by default
      >
        {loading ? 'Exporting...' : 'Export to EVEMon'}
      </button>
      {error && <div className="text-red-600 mt-1" role="status">{error}</div>}
    </div>
  );
}; 