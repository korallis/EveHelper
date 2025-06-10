import { useState, useCallback } from 'react';
// @ts-ignore
import { invoke } from '@tauri-apps/api/tauri';

/**
 * React hook to trigger SDE refresh via Tauri backend
 * @returns { refreshSde, loading, error, success }
 */
export function useSdeRefresh() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const refreshSde = useCallback(async () => {
    setLoading(true);
    setError(null);
    setSuccess(false);
    try {
      await (invoke as any)('refresh_sde_cmd');
      setSuccess(true);
    } catch (err: any) {
      setError(err.message || 'Failed to refresh SDE');
    } finally {
      setLoading(false);
    }
  }, []);

  return { refreshSde, loading, error, success };
} 