import React from 'react';

/**
 * Modal for EULA/CCP Developer License acceptance.
 * Blocks app usage until accepted.
 * @param onAccept Callback for acceptance
 * @param onDecline Callback for decline
 */
export interface EulaModalProps {
  onAccept: () => void;
  onDecline: () => void;
}

const EulaModal: React.FC<EulaModalProps> = ({ onAccept, onDecline }) => (
  <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-60 z-50">
    <div className="bg-white p-8 rounded shadow-lg max-w-lg w-full">
      <h2 className="text-xl font-bold mb-4">CCP Developer License Agreement</h2>
      <div className="mb-6 max-h-64 overflow-y-auto text-sm text-gray-700">
        {/* EULA text would go here. For brevity, placeholder text is used. */}
        <p>Please review and accept the CCP Developer License to use this application. If you decline, the app will close.</p>
      </div>
      <div className="flex justify-end gap-4">
        <button className="px-4 py-2 bg-gray-300 rounded" onClick={onDecline}>Decline</button>
        <button className="px-4 py-2 bg-blue-600 text-white rounded" onClick={onAccept}>Accept</button>
      </div>
    </div>
  </div>
);

export default EulaModal; 