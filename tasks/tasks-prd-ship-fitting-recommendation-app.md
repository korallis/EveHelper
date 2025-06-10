## Relevant Files

- `eveonline-helper/.eslintrc` - ESLint configuration for TypeScript, React, and Tailwind linting.
- `eveonline-helper/.prettierrc` - Prettier configuration for code formatting.
- `eveonline-helper/package.json` - Project dependencies and scripts.
- `eveonline-helper/tsconfig.json` - TypeScript configuration.
- `eveonline-helper/tailwind.config.js` - Tailwind CSS configuration.
- `eveonline-helper/src/components/` - React components directory (created).
- `eveonline-helper/src/hooks/` - Custom React hooks directory (created).
- `eveonline-helper/src/utils/` - Utility functions directory (created).
- `eveonline-helper/src/styles/tailwind.css` - Tailwind and global CSS (created).
- `eveonline-helper/src/assets/` - Images, icons, and shared resources.
- `eveonline-helper/src-tauri/src/esi.rs` - ESI integration Rust module (created).
- `eveonline-helper/src-tauri/src/db.rs` - SQLite DB logic Rust module (created).
- `eveonline-helper/src-tauri/src/main.rs` - Main Rust backend for Tauri, handles OS integration, secure token storage, and backend logic.
- `eveonline-helper/src-tauri/src/lib.rs` - Shared Rust logic for Tauri backend.
- `eveonline-helper/src-tauri/Cargo.toml` - Rust dependencies and project configuration.
- `eveonline-helper/src-tauri/tauri.conf.json` - Tauri app configuration.
- `eveonline-helper/src/App.tsx` - Main React app entry point.
- `eveonline-helper/vite.config.ts` - Vite build configuration.
- `eveonline-helper/README.md` - Project overview and folder structure documentation (updated).
- `eveonline-helper/docs/CONTRIBUTING.md` - Contribution guidelines (created).
- `eveonline-helper/docs/CHANGELOG.md` - Project changelog (created).
- `eveonline-helper/docs/ONBOARDING.md` - Onboarding guide for new developers (created).
- `src-tauri/tests/` - Rust backend unit/integration tests.
- `src/components/__tests__/` - Frontend React component tests.

### Notes

- ESLint, Prettier, and plugins for React, Tailwind, and TypeScript installed for frontend linting/formatting.
- Prettier plugin for Tailwind ensures class sorting and formatting.
- rustfmt and clippy are available for Rust linting/formatting.
- Project dependencies installed with pnpm for optimal cross-platform support.
- Tauri, Rust, and Node environments verified for macOS ARM64 and Windows 11+ compatibility.
- Unit tests should typically be placed alongside the code files they are testing (e.g., `MyComponent.tsx` and `MyComponent.test.tsx` in the same directory).
- Use `npx jest [optional/path/to/test/file]` to run tests. Running without a path executes all tests found by the Jest configuration.

## Tasks

- [x] 1.0 Set up project scaffolding and core architecture
  - [x] 1.1 Initialize Tauri + React + Tailwind project structure
  - [x] 1.2 Configure project for cross-platform builds (macOS ARM64, Windows 11+)
  - [x] 1.3 Set up TypeScript, Rust, and Tailwind linting/formatting tools
  - [x] 1.4 Establish folder structure for backend, frontend, and shared assets
  - [x] 1.5 Add initial README and documentation templates

- [x] 2.0 Implement secure EVE SSO login and local encrypted data storage
  - [x] 2.1 Integrate EVE SSO OAuth2 PKCE flow in Rust backend
  - [x] 2.2 Implement system browser login and local callback handling
  - [x] 2.3 Store ESI tokens securely in OS keychain/credential manager
  - [x] 2.4 Design and implement encrypted SQLite schema for user/ship/skill data
  - [x] 2.5 Add logic to import and update user skills and character data from ESI
  - [x] 2.6 Write backend and integration tests for authentication and storage

- [x] 3.0 Develop ship fitting recommendation engine and fit validation
  - [x] 3.1 Import static SDE data into local encrypted database
  - [x] 3.2 Implement logic to match user skills to eligible ships and modules
  - [x] 3.3 Design algorithm to generate five fit variants per ship/activity
  - [x] 3.4 Validate fits for powergrid, CPU, slots, and skill requirements
  - [x] 3.5 Suggest alternative fits if requirements are not met
  - [x] 3.6 Expose fit recommendation API to frontend via Tauri commands
  - [x] 3.7 Write unit and integration tests for fit engine and validation

- [ ] 4.0 Build skill plan generation, visualization, and export features
  - [ ] 4.1 Implement algorithm to generate prioritized skill plans for recommended fits
  - [ ] 4.2 Add logic for progression planning (e.g., next ship/activity tier)
  - [ ] 4.3 Create React component for skill plan timeline visualization
  - [ ] 4.4 Enable EVEMon-compatible skill plan export
  - [ ] 4.5 Write tests for skill plan generation and export logic

- [ ] 5.0 Create onboarding, offline support, and documentation features
  - [ ] 5.1 Build onboarding UI and step-by-step unsigned app launch guide (macOS)
  - [ ] 5.2 Implement offline-first logic for all core features
  - [ ] 5.3 Add in-app troubleshooting and help documentation
  - [ ] 5.4 Ensure accessibility (keyboard navigation, colorblind support)
  - [ ] 5.5 Write tests for onboarding and offline support features

- [ ] 6.0 Integrate GitHub workflows, CI/CD, and enforce code/test quality
  - [ ] 6.1 Set up GitHub Actions for linting, testing, and building on PRs
  - [ ] 6.2 Enforce >80% code coverage with automated test reporting
  - [ ] 6.3 Configure release workflow for unsigned .app/.exe artifacts
  - [ ] 6.4 Integrate documentation and changelog automation
  - [ ] 6.5 Add PR review and code suggestion automation via Cursor IDE 

- [ ] 7.0 SDE Management and Legal Compliance
  - [ ] 7.1 Add UI and backend logic for manual SDE refresh (download latest snapshot, update DB)
  - [ ] 7.2 Display EULA and/or CCP developer license on first launch (with accept/decline)
  - [ ] 7.3 (Optional) Add localization/i18n support for multi-language UI 