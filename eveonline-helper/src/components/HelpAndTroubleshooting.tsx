import React from 'react';

/**
 * In-app help and troubleshooting component
 * Provides FAQ, solutions, and links to docs/support
 * @accessibility
 * - Uses semantic headings and lists
 * - All links have descriptive text and aria-label if needed
 * - Uses role="region" and aria-labelledby for main container
 */
export const HelpAndTroubleshooting: React.FC = () => (
  <div
    className="max-w-2xl mx-auto p-6 bg-white rounded shadow mt-8"
    role="region"
    aria-labelledby="help-heading"
  >
    <h1 id="help-heading" className="text-2xl font-bold mb-4">Help & Troubleshooting</h1>
    <h2 className="text-xl font-semibold mt-6 mb-2">Frequently Asked Questions</h2>
    <ul className="list-disc ml-6 space-y-4">
      <li>
        <strong>How do I log in with EVE SSO?</strong>
        <div className="text-gray-700 text-sm">
          Use the "Login with EVE Online" button. If your browser does not open, check your system's default browser settings and try again.
        </div>
      </li>
      <li>
        <strong>Why do I see an offline warning?</strong>
        <div className="text-gray-700 text-sm">
          The app detected you are offline. You can still use most features, but ESI sync and SDE updates require an internet connection.
        </div>
      </li>
      <li>
        <strong>How do I launch the app on macOS if it is unsigned?</strong>
        <div className="text-gray-700 text-sm">
          See the <a href="/docs/ONBOARDING.md" className="text-blue-600 underline" aria-label="Onboarding guide for unsigned app launch on macOS">onboarding guide</a> for step-by-step instructions.
        </div>
      </li>
      <li>
        <strong>Why are my fits or skills not updating?</strong>
        <div className="text-gray-700 text-sm">
          Make sure you are online and logged in. If the problem persists, try restarting the app or checking for updates.
        </div>
      </li>
      <li>
        <strong>How do I contact support or get more help?</strong>
        <div className="text-gray-700 text-sm">
          Visit the <a href="/docs/HELP.md" className="text-blue-600 underline" aria-label="Full help documentation">full help documentation</a> or join our community Discord (link in README).
        </div>
      </li>
    </ul>
    <h2 className="text-xl font-semibold mt-8 mb-2">More Resources</h2>
    <ul className="list-disc ml-6 space-y-2 text-blue-700">
      <li><a href="/docs/ONBOARDING.md" className="underline" aria-label="Onboarding Guide">Onboarding Guide</a></li>
      <li><a href="/docs/HELP.md" className="underline" aria-label="Full Help Documentation">Full Help Documentation</a></li>
      <li><a href="https://developers.eveonline.com/docs/" target="_blank" rel="noopener noreferrer" className="underline" aria-label="EVE Developer Docs">EVE Developer Docs</a></li>
      <li><a href="https://github.com/your-repo/issues" target="_blank" rel="noopener noreferrer" className="underline" aria-label="Report an Issue on GitHub">Report an Issue</a></li>
    </ul>
  </div>
); 