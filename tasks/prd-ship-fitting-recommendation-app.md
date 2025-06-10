# Ship Fitting Recommendation App PRD

## Introduction/Overview
A cross-platform desktop application for EVE Online that simplifies the ship fitting and skill training process for new and intermediate players. The app recommends optimal ship fits and skill plans for specific activities (e.g., missions, mining) based on the user's current skills, with a focus on actionable, safe, and effective recommendations. The goal is to help users know what to fly, what to fit, and what to train next, maximizing their in-game performance and progression.

## Goals
1. Enable new and intermediate EVE Online players to receive personalized ship fitting recommendations for their chosen activity and current skill set.
2. Provide clear, step-by-step skill training plans to unlock and improve recommended fits and activities.
3. Ensure all recommendations are safe (minimize risk of ship loss) and effective (maximize performance for the chosen activity).
4. Support offline-first operation, privacy, and zero-friction installation on macOS and Windows.
5. Integrate with GitHub and Cursor IDE for developer productivity and code quality.

## User Stories
- As a new player, I want to run missions for a specific corporation and ensure I am flying the best ship I can for this, given my current skills, so I can do the most damage without getting blown up.
- As an intermediate player, I have a lot of skills but am unsure what ship would give the best performance for mining, given my trained skills.
- As a new player, after receiving a suggested fit that works, I want to know what skills to train next to progress to higher-level missions (e.g., Level 2 and 3) and improve my effectiveness.

## Functional Requirements
1. The system must allow users to log in with EVE SSO using secure OAuth2 PKCE via the system browser.
2. The app must import and store the user's current skills and character data locally (encrypted SQLite).
3. The app must recommend ship fits for selected activities (missions, mining, PVP, etc.) based on the user's skills.
4. For each recommended ship, the app must provide five distinct fits (e.g., Maximum DPS, Maximum Tank, Balanced, Cap-Stable, etc.) for DPS/combat activities, and the five most optimal fits for other activities.
5. The app must validate fits for powergrid, CPU, slot layout, and skill requirements, and suggest alternatives if requirements are not met.
6. The app must generate a prioritized skill plan to unlock and improve the recommended fit, including a progression plan for the user's chosen activity.
7. The app must visualize skill progression timelines and allow export in EVEMon-compatible format.
8. All user data and static ship/module info must be stored locally and encrypted.
9. The app must function offline for all core features after initial SDE download.
10. The app must provide in-app onboarding, troubleshooting, and unsigned app launch guides for macOS.
11. The app must not collect telemetry or analytics.
12. The app must support GitHub integration for source, issues, releases, and documentation.
13. The app must enforce >80% code coverage via CI/CD workflows.

## Non-Goals (Out of Scope)
- No support for activities or ships not present in the current SDE snapshot.
- No telemetry, analytics, or third-party tracking.
- No cloud storage or external database integration.
- No code signing or developer certificate requirements for end users.
- No support for mobile platforms (desktop only).

## Design Considerations
- Use React 19 and Tailwind CSS 4 for a modern, responsive, and accessible UI.
- Ensure keyboard navigation and colorblind accessibility.
- Provide illustrated, step-by-step guides for unsigned app launch on macOS.
- Maintain a professional, user-friendly look, not necessarily matching EVE's in-game UI.

## Technical Considerations
- Use Tauri (Rust) backend for OS integration, secure token handling, and encrypted SQLite storage.
- All ESI tokens and sensitive logic must remain in the Rust backend, never exposed to the frontend.
- Use `sqlx` or `rusqlite` for database access, with default encryption.
- Support only one EVE account per app instance for MVP.
- Ship with a static SDE snapshot; allow manual refresh on demand.

## Success Metrics
- User can download, launch, and log in within 5 minutes on macOS and Windows, with no developer account needed.
- 100% of core features (fit recommendations, stats, skill plan export) work offline after initial setup.
- >80% unit/integration test coverage, enforced via GitHub Actions.
- PR reviews and code suggestions automated via Cursor IDE's GitHub integration.
- Identical UX on Mac Mini M4 Pro and Windows 11+.

## Open Questions
- Are there any legal or compliance requirements (e.g., EULA, CCP developer license) that must be displayed or agreed to in-app?
- Should the app support multiple EVE accounts per user in future versions?
- Are there any additional accessibility requirements (e.g., localization, screen reader support)? 