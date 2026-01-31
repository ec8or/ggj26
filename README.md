# Crowd Control GGJ26

## Quick Start

### 1. Start Server
```bash
cd server
npm install
npm start
```

Server will display:
- QR code in terminal
- URL to join (e.g., http://192.168.1.100:3000)

### 2. Start Unity
1. Open `/unity/` in Unity 2022.3 LTS
2. Open `MainGame` scene
3. Press Play
4. Verify "Connected to Server" in Console

### 3. Connect Mobile Phones
1. Open camera app on phone
2. Scan QR code displayed in terminal
3. Tap "Join Game" button
4. See your mask appear on phone screen

### 4. Start Game
1. In Unity, press SPACE or click "Start Game" button
2. Game begins, masks appear on screen
3. Players tap when they see their mask
4. Last player standing wins!

## Architecture
- **Server**: Relay only (no game logic)
- **Unity**: All game logic & display
- **Mobile**: Thin client (tap + feedback)

## Development
See `/CLAUDE.md` for full implementation plan.
See `/server/CLAUDE.md` for server implementation.
See `/unity/CLAUDE.md` for Unity implementation.
