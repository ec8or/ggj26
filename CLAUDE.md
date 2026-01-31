# Crowd Control GGJ26

## Project Overview
A 50-60 player shared-screen multiplayer battle royale game where players use mobile phones as controllers.

**Critical Context**: This is a 48-hour game jam project. Prioritize MVP features.

## Architecture
- **Relay Server Model**: Server does NOT run game logic, only relays messages
- **Unity Host**: All game logic, timers, and state management happens in Unity
- **Mobile Clients**: Thin clients that send taps and receive feedback

## Tech Stack
- Server: Node.js + Socket.io (relay only)
- Client: Vanilla JS/HTML5 (mobile controller)
- Host: Unity + SocketIOUnity (game display & logic)

## Communication Flow
1. Mobile → Server: Player taps
2. Server → Unity: Relayed tap events
3. Unity → Server: Game state updates, elimination events
4. Server → Mobile: Haptic feedback, death notifications

## Sub-Projects
- `/server/` - See server/CLAUDE.md
- `/unity/` - See unity/CLAUDE.md

## Development Milestones

### Milestone 1: Basic Connectivity (Hour 0-6)
- Server relay functional
- Mobile phones can connect via QR
- Unity receives tap events
- Player count displays

### Milestone 2: Core Game Loop (Hour 6-18)
- Main round logic (show masks, detect correct taps)
- Elimination system
- Kill feed display
- 60 mask sprites loaded

### Milestone 3: Bonus Rounds (Hour 18-30)
- Sprint round (tap counter)
- Reaction round (GO signal)
- Precision round (timing)

### Milestone 4: Polish & Testing (Hour 30-48)
- Haptic feedback on mobile
- Anti-zoom/refresh on mobile
- Visual effects in Unity
- Multi-device testing

## Out of Scope (Do NOT implement)
- Player accounts/persistence
- Spectator mode
- Audio (no sound effects or music)
- Replays
- Advanced analytics
- Server-side game logic validation
