import { useState, useEffect, useCallback } from 'react';
// @ts-ignore
import { invoke } from '@tauri-apps/api/core';

/**
 * React hook to check and set EULA acceptance via Tauri backend
 * @returns { isAccepted, loading, error, acceptEula, declineEula }
 */
export function useEulaAcceptance() {
  const [isAccepted, setIsAccepted] = useState<boolean | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      setLoading(true);
      setError(null);
      try {
        const accepted = await (invoke as any)('get_eula_accepted');
        setIsAccepted(!!accepted);
      } catch (err: any) {
        setError(err.message || 'Failed to check EULA acceptance');
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  const acceptEula = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      await (invoke as any)('set_eula_accepted_cmd', { accepted: true });
      setIsAccepted(true);
    } catch (err: any) {
      setError(err.message || 'Failed to accept EULA');
    } finally {
      setLoading(false);
    }
  }, []);

  const declineEula = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      await (invoke as any)('set_eula_accepted_cmd', { accepted: false });
      setIsAccepted(false);
    } catch (err: any) {
      setError(err.message || 'Failed to decline EULA');
    } finally {
      setLoading(false);
    }
  }, []);

  return { isAccepted, loading, error, acceptEula, declineEula };
} 