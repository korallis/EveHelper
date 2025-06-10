import React from 'react';
import { useSdeAutoUpdate } from '../hooks/useSdeAutoUpdate';

/**
 * Banner to notify user of automatic SDE update
 * Dismissible, accessible, and styled
 */
export const SdeAutoUpdateBanner: React.FC = () => {
  const { showBanner, clearBanner } = useSdeAutoUpdate();
  if (!showBanner) return null;
  return (
    <div
      className="w-full bg-blue-200 text-blue-900 p-3 text-center font-semibold shadow-md z-50 flex items-center justify-center relative"
      role="status"
      aria-live="polite"
    >
      <span>SDE database was automatically updated to the latest version.</span>
      <button
        className="ml-4 px-2 py-1 bg-blue-400 text-white rounded hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-600"
        onClick={clearBanner}
        aria-label="Dismiss SDE update notification"
      >
        ×
      </button>
    </div>
  );
}; 