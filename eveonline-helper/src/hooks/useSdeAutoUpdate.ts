import { useEffect, useState } from 'react';
// @ts-ignore
import { invoke } from '@tauri-apps/api/tauri';

/**
 * React hook to detect if an automatic SDE update has occurred on startup.
 * Polls a Tauri command or timestamp to show a notification/banner.
 * @returns { showBanner: boolean, clearBanner: () => void }
 */
export function useSdeAutoUpdate() {
  const [showBanner, setShowBanner] = useState(false);

  useEffect(() => {
    // On mount, ask backend if an auto-update was performed
    (async () => {
      try {
        const updated = await (invoke as any)('sde_auto_update_occurred');
        if (updated) setShowBanner(true);
      } catch {
        // Ignore errors
      }
    })();
  }, []);

  const clearBanner = () => setShowBanner(false);

  return { showBanner, clearBanner };
} 