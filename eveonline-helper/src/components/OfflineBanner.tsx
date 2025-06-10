import React from 'react';

/**
 * Banner to display when the app is offline
 * @param message Optional custom message
 * @accessibility
 * - Uses role="status" and aria-live="polite" for screen readers
 * - Sufficient color contrast
 */
export const OfflineBanner: React.FC<{ message?: string }> = ({ message }) => (
  <div
    className="w-full bg-yellow-200 text-yellow-900 p-3 text-center font-semibold shadow-md z-50"
    role="status"
    aria-live="polite"
  >
    {message || 'You are offline. Some features may be unavailable.'}
  </div>
); 