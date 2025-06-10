# EveOnline Helper

A cross-platform desktop application for EVE Online ship fitting and skill planning, built with Tauri, React, TypeScript, and Tailwind CSS.

## Folder Structure

```
.
├── public/                # Static assets (favicon, images, etc.)
├── src/                   # Frontend source code
│   ├── assets/            # Images, icons, and shared resources
│   ├── components/        # React components (to be created)
│   ├── hooks/             # Custom React hooks (to be created)
│   ├── utils/             # Utility functions (to be created)
│   └── styles/            # Tailwind and global CSS (to be created)
├── src-tauri/             # Tauri (Rust) backend
│   ├── src/               # Rust source code
│   │   ├── esi.rs         # ESI integration (to be created)
│   │   ├── db.rs          # SQLite DB logic (to be created)
│   │   └── ...
│   ├── icons/             # App icons
│   ├── capabilities/      # Tauri plugin configs
│   ├── Cargo.toml         # Rust dependencies
│   └── tauri.conf.json    # Tauri app config
├── .vscode/               # VSCode settings
├── package.json           # Project dependencies and scripts
├── tsconfig.json          # TypeScript config
├── vite.config.ts         # Vite config
└── README.md              # Project overview
```

## Getting Started

1. Install dependencies: `pnpm install`
2. Start the app: `pnpm tauri dev`

## Linting & Formatting
- TypeScript/React: `pnpm lint` (to be configured)
- Rust: `cargo fmt` and `cargo clippy`
- Tailwind: Prettier plugin for class sorting

## License
MIT
