# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Full-stack chess game with an AI opponent. C#/.NET 9.0 API backend, React/TypeScript frontend, MongoDB database, all orchestrated via Docker.

## Common Commands

```bash
# Development
npm run build:css          # Generate Tailwind CSS output (required on first run)
npm run start:db           # Start MongoDB via Docker
npm run start:app          # Start API (dotnet run) + client (Vite dev) concurrently

# Build & Verify
npm run verify             # Build both API and client
npm run verify:api         # dotnet build (in api/)
npm run verify:client      # Full client build (Tailwind + tsc + Vite)

# Lint
cd client && npm run lint  # ESLint with auto-fix on client/src

# Tests (Playwright E2E)
cd playwright_tests && npx playwright test              # Run all tests
cd playwright_tests && npx playwright test --project=chromium  # Chromium only
cd playwright_tests && npx playwright test -g "test name"      # Run single test by name
```

## Architecture

### Backend (api/)

- **ASP.NET Core Web API** targeting .NET 9.0, routes prefixed `/api/v1/game`
- **GameController.cs** — 4 endpoints: `tryGetSavedGame`, `startGame`, `compMove`, `click`
- **GameRepository.cs** — Core game logic and MongoDB persistence via `MongoDB.Driver`
- **HelperClasses/Chess/** — Chess engine:
  - `MinMaxEngine.cs` — Minimax AI with configurable depth (default 4) and 10-second timeout
  - `MoveHelper.cs` — Move validation and generation
  - `PieceHelper.cs` — Piece-specific move logic
  - `BoardHelper.cs` — Board state manipulation
  - `CheckTracker.cs` — Check/checkmate detection
- **Pieces/** — Polymorphic piece types implementing `IPiece` (and optional `IPieceCanPin`, `IPieceDirectAttacker`, `IPieceHasMoved` interfaces)
- **BackgroundJobs/DeactivateGame.cs** — Cleans inactive games from memory cache (1-min TTL, 30-sec interval)
- **Middleware/ErrorMiddleware.cs** — Global error handling with MongoDB error logging
- Active games held in `IMemoryCache`; game sessions tracked via cookies

### Frontend (client/)

- **React 18 + TypeScript + Vite** on port 5175
- **Tailwind CSS** for styling (input: `src/input.css`, output: `public/output.css`)
- **src/api/GeneratedAPI.ts** — Auto-generated TypeScript client from Swagger via NSwag (generated during API build as a post-build step, output goes to `client/src/api/`)
- **src/components/board.tsx** — Chess board rendering
- **src/components/resetbuttons.tsx** — Game control buttons
- **src/App.tsx** — Main game state and logic (React hooks, no external state library)

### API Client Generation

The API build has a post-build event that generates `swagger.json` then runs NSwag to produce `client/src/api/GeneratedAPI.ts`. Rebuild the API (`dotnet build` in `api/`) to regenerate the TypeScript client after endpoint changes.

### Database

MongoDB 6.0 with collections: Games, MoveHistory, ErrorLog. Connection strings in `appsettings.json` (production/Docker) and `appsettings.Development.json` (localhost). Default credentials: `root:r00t`.

### Docker

- `Docker/MongoDB.yml` — MongoDB only (for local dev)
- `docker-compose.yml` — Full stack local build
- `docker-compose-latest.yml` — Production with pre-built images from GitHub Container Registry

### CI/CD (.github/workflows/)

- **PullRequest.yml** — Builds both projects, runs Playwright tests on PR
- **Push.yml** — Builds and pushes Docker images to GHCR on merge to main

## Code Style

- Client uses Prettier: single quotes, no semicolons, trailing commas, auto end-of-line
- ESLint configured with TypeScript and React plugins (flat config format)
- CORS origins hardcoded in `api/Program.cs` for localhost ports 3000 and 5175
