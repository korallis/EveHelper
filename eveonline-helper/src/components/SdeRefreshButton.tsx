import React from 'react';
import { useSdeRefresh } from '../hooks/useSdeRefresh';

/**
 * Button to manually refresh the SDE snapshot
 * Shows loading, error, and success states
 * @accessibility
 * - Button is focusable, with visible focus and aria-label
 * - Status messages use role="status"
 */
export const SdeRefreshButton: React.FC = () => {
  const { refreshSde, loading, error, success } = useSdeRefresh();

  return (
    <div className="mt-4">
      <button
        className="px-4 py-2 bg-green-700 text-white rounded hover:bg-green-800 focus:outline-none focus:ring-2 focus:ring-green-400"
        onClick={refreshSde}
        disabled={loading}
        aria-label="Refresh SDE database"
      >
        {loading ? 'Refreshing SDE...' : 'Refresh SDE Now'}
      </button>
      {error && <div className="text-red-600 mt-2" role="status">{error}</div>}
      {success && !loading && <div className="text-green-700 mt-2" role="status">SDE updated successfully!</div>}
    </div>
  );
}; 