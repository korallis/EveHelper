import { useState, useCallback } from 'react';
// @ts-ignore
import { invoke } from '@tauri-apps/api/tauri';
import { Ship, Skill } from './useSkillPlan';

/**
 * TypeScript type for a progression suggestion
 */
export interface ShipProgressionSuggestion {
  tier: string;
  ship: Ship;
  required_skills: [number, string, number][];
}

/**
 * React hook to fetch the next ship progression suggestion
 * @returns { progression, loading, error, fetchProgression }
 */
export function useShipProgression() {
  const [progression, setProgression] = useState<ShipProgressionSuggestion | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  /**
   * Fetch the progression suggestion from the backend
   * @param userSkills The user's skills
   * @param eligibleShips Ships the user can currently fly
   * @param allShips All ships in the SDE
   */
  const fetchProgression = useCallback(async (userSkills: Skill[], eligibleShips: Ship[], allShips: Ship[]) => {
    setLoading(true);
    setError(null);
    try {
      const result = await (invoke as any)(
        'get_next_ship_progression',
        { userSkills, eligibleShips, allShips }
      ) as [string, Ship, [number, string, number][]] | null;
      if (result) {
        const [tier, ship, required_skills] = result;
        setProgression({ tier, ship, required_skills });
      } else {
        setProgression(null);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to fetch progression suggestion');
      setProgression(null);
    } finally {
      setLoading(false);
    }
  }, []);

  return { progression, loading, error, fetchProgression };
} 