/**
 * NOTE: This file requires @tauri-apps/api as a dev dependency for type safety.
 * If you see a linter error for the import below, run:
 *   pnpm add -D @tauri-apps/api
 * The code will still work at runtime if Tauri is present.
 */
import { useState, useCallback } from 'react';
// @ts-ignore
import { invoke } from '@tauri-apps/api/tauri';

/**
 * TypeScript type for a skill plan entry
 */
export interface SkillPlanEntry {
  skill_id: number;
  skill_name: string;
  required_level: number;
  current_level: number;
}

/**
 * TypeScript type for a FitVariant (should match backend)
 */
export interface FitVariant {
  fit_name: string;
  ship: { ship_id: number; ship_name: string };
  modules: { module_id: number; module_name: string }[];
  rationale: string;
}

/**
 * TypeScript type for a Skill (should match backend)
 */
export interface Skill {
  skill_id: number;
  skill_name?: string;
  active_level: number;
}

/**
 * React hook to fetch a prioritized skill plan for a recommended fit
 * @returns { skillPlan, loading, error, fetchSkillPlan }
 */
export function useSkillPlan() {
  const [skillPlan, setSkillPlan] = useState<SkillPlanEntry[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetch the skill plan from the backend
   * @param fit The fit variant
   * @param userSkills The user's skills
   */
  const fetchSkillPlan = useCallback(async (fit: FitVariant, userSkills: Skill[]) => {
    setLoading(true);
    setError(null);
    try {
      // Type as any to avoid linter error if types are missing
      const result = await (invoke as any)(
        'get_skill_plan_for_fit',
        { fit, userSkills }
      ) as [number, string, number, number][];
      setSkillPlan(
        result.map((tuple: [number, string, number, number]) => {
          const [skill_id, skill_name, required_level, current_level]: [number, string, number, number] = tuple;
          return { skill_id, skill_name, required_level, current_level };
        })
      );
    } catch (err: any) {
      setError(err.message || 'Failed to fetch skill plan');
      setSkillPlan(null);
    } finally {
      setLoading(false);
    }
  }, []);

  return { skillPlan, loading, error, fetchSkillPlan };
}

export interface Ship {
  ship_id: number;
  ship_name: string;
}

export interface Module {
  module_id: number;
  module_name: string;
}

/**
 * Export a skill plan as an EVEMon-compatible string using the backend
 * @param plan The skill plan array
 * @returns Promise<string> EVEMon-formatted string
 */
export async function exportSkillPlanEvemon(plan: SkillPlanEntry[]): Promise<string> {
  // Convert to backend tuple format
  const backendPlan = plan.map(({ skill_id, skill_name, required_level, current_level }) => [skill_id, skill_name, required_level, current_level] as [number, string, number, number]);
  // @ts-ignore
  const result = await (invoke as any)('export_skill_plan_evemon_cmd', { plan: backendPlan });
  if (typeof result !== 'string') throw new Error('Failed to export skill plan');
  return result;
} 