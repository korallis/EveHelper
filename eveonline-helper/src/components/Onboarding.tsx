import React from 'react';

/**
 * Onboarding component for new users, including unsigned app launch guide for macOS
 * @accessibility
 * - Uses semantic headings and ordered list
 * - Continue button is focusable, with visible focus and aria-label
 */
export const Onboarding: React.FC<{ onComplete?: () => void }> = ({ onComplete }) => {
  return (
    <div className="max-w-xl mx-auto p-6 bg-white rounded shadow mt-8">
      <h1 className="text-2xl font-bold mb-4">Welcome to EveOnline Helper!</h1>
      <p className="mb-4">
        Thank you for installing EveOnline Helper. This onboarding guide will help you get started and ensure you can launch the app on macOS, even if it is unsigned.
      </p>
      <h2 className="text-xl font-semibold mt-6 mb-2">Launching the Unsigned App on macOS</h2>
      <ol className="list-decimal ml-6 mb-4 space-y-2">
        <li>
          <span className="font-semibold">Open Terminal</span> (press <kbd>Cmd</kbd> + <kbd>Space</kbd>, type <code>Terminal</code>, and hit <kbd>Enter</kbd>).
        </li>
        <li>
          <span className="font-semibold">Navigate to the app directory</span> (replace <code>~/Downloads/EveOnline-Helper</code> with your actual path):
          <pre className="bg-gray-100 rounded p-2 mt-1 text-sm">cd ~/Downloads/EveOnline-Helper</pre>
        </li>
        <li>
          <span className="font-semibold">Remove quarantine attribute</span> (required for unsigned apps):
          <pre className="bg-gray-100 rounded p-2 mt-1 text-sm">xattr -dr com.apple.quarantine ./target/release/bundle/app/EveOnline-Helper.app</pre>
        </li>
        <li>
          <span className="font-semibold">Launch the app</span>:
          <pre className="bg-gray-100 rounded p-2 mt-1 text-sm">open ./target/release/bundle/app/EveOnline-Helper.app</pre>
        </li>
        <li>
          If you see a warning, right-click the app in Finder, choose <b>Open</b>, and confirm you want to open it.
        </li>
      </ol>
      <p className="mb-4 text-sm text-gray-600">
        For more help, see the <a href="/docs/ONBOARDING.md" className="text-blue-600 underline">full onboarding guide</a> or contact support.
      </p>
      <button
        className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-400"
        onClick={onComplete}
        aria-label="Continue to app"
      >
        Continue to App
      </button>
    </div>
  );
}; 