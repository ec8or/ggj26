# Setup Guide - Crowd Control GGJ26

## Prerequisites

- Node.js 16+ installed
- Unity 2022.3 LTS or newer
- Multiple mobile devices (iOS/Android) for testing

---

## Server Setup

### 1. Install Dependencies

```bash
cd /Users/nils/Dev/GGJ26/server
npm install
```

This will install:
- express (web server)
- socket.io (WebSocket relay)
- qrcode (QR code generation)

### 2. Start Server

```bash
npm start
```

You should see:
- Server running message
- QR code in terminal
- Network URL (e.g., http://192.168.1.100:3000)

### 3. Test Mobile Connection

1. Open phone camera
2. Scan QR code
3. Should see "Connected!" status
4. Large purple TAP button appears

---

## Unity Setup

### 1. Open Project in Unity

1. Launch Unity Hub
2. Click "Open" or "Add"
3. Navigate to `/Users/nils/Dev/GGJ26/unity`
4. Unity will import and compile scripts

### 2. Install SocketIOUnity Package

**Method 1: Package Manager (Recommended)**
1. Open Unity
2. Window → Package Manager
3. Click "+" → "Add package from git URL"
4. Enter: `https://github.com/itisnajim/SocketIOUnity.git`
5. Click "Add"

**Method 2: Manual Install**
1. Download SocketIOUnity from GitHub
2. Extract to `Assets/Plugins/SocketIOUnity`

### 3. Create Main Scene

Since Unity scenes can't be created via scripts, you'll need to set up the scene manually:

**Create Scene:**
1. File → New Scene
2. Save as `Assets/Scenes/MainGame.unity`

**Add Game Manager:**
1. Create Empty GameObject: "GameManager"
2. Add Scripts:
   - NetworkManager.cs
   - GameManager.cs
   - PlayerManager.cs
   - RoundController.cs
   - MaskManager.cs
   - UIManager.cs
   - SprintRound.cs
   - ReactionRound.cs
   - PrecisionRound.cs

**Set NetworkManager Server URL:**
1. Select GameManager
2. In NetworkManager component
3. Set "Server Url" to match your server (e.g., `http://192.168.1.100:3000`)

**Create UI Canvas:**
1. Right-click Hierarchy → UI → Canvas
2. Canvas Scaler → Scale With Screen Size → 1920x1080

**Add UI Elements:**

**Lobby Panel:**
- Create Panel: "QR Code Panel"
- Add TextMeshPro: "Instructions" (center, large font)
- Text: "Press SPACE to start\nScan QR code on phones"

**Game HUD:**
- TextMeshPro (top-left): "Player Count" → Link to UIManager
- TextMeshPro (top-center): "Round Number" → Link to UIManager
- TextMeshPro (top-right): "Round Timer" → Link to UIManager

**Mask Display Container:**
- Create Empty GameObject: "Mask Display Container"
- Set RectTransform to center of screen
- Link to MaskManager

**Bonus Round Panel:**
- Create Panel: "Bonus Round Panel" (initially hidden)
- Add TextMeshPro: "Bonus Title" (huge font, centered)
- Link to UIManager

**Winner Panel:**
- Create Panel: "Winner Panel" (initially hidden, full screen)
- Add TextMeshPro: "Winner Text" (centered, huge)
- Link to UIManager

**Create Mask Prefab:**
1. Create UI → Image (name it "MaskDisplay")
2. Set size to 400x400
3. Drag to `Assets/Prefabs/` folder
4. Delete from scene
5. Link prefab to MaskManager component

### 4. Link Components

In GameManager Inspector:
- Link all required script references
- Verify no "None (Script)" references

In MaskManager Inspector:
- Link "Mask Display Container" (empty GameObject from step above)
- Link "Mask Prefab" (the prefab you created)
- Check "Use Placeholder Sprites" = true (for testing)

In UIManager Inspector:
- Link all TextMeshPro fields
- Link panels (QR Code, Bonus Round, Winner)

### 5. Test Connection

1. Make sure server is running
2. Press Play in Unity
3. Check Console for:
   - "🔌 Connecting to server..."
   - "✅ Connected to server"

---

## Testing the Full System

### Basic Test (1 Phone)

1. Start server: `cd server && npm start`
2. Start Unity (Press Play)
3. Connect 1 phone via QR code
4. Verify Unity logs: "👤 Player joined"
5. Verify phone shows: "Mask #1"
6. Press SPACE in Unity
7. Tap button on phone when mask appears
8. Check Unity Console for tap logs

### Multi-Player Test (5+ Phones)

1. Connect 5+ phones
2. Verify each gets unique Mask ID
3. Press SPACE to start game
4. Round 1: 3-5 masks appear
5. Players with those masks tap
6. Others don't tap
7. Verify eliminations correct
8. Continue to round 5 for bonus round

---

## Troubleshooting

### Server Issues

**Problem:** Server won't start
- Check if port 3000 is in use: `lsof -i :3000`
- Kill process or change PORT in server.js

**Problem:** QR code not showing
- Check console for errors
- Verify qrcode package installed

**Problem:** Phones can't connect
- Ensure phone and computer on same WiFi
- Check firewall settings
- Try accessing http://[IP]:3000 in phone browser

### Unity Issues

**Problem:** Scripts won't compile
- Install SocketIOUnity package
- Install TextMeshPro (Window → TextMeshPro → Import Essentials)
- Check for missing namespaces

**Problem:** "SocketIOUnity not found"
- Verify package installed via Package Manager
- Try reimporting package
- Check `Packages/manifest.json` includes the package

**Problem:** No tap events received
- Check NetworkManager server URL matches server
- Verify Console shows "Connected to server"
- Check server logs for tap events

**Problem:** NullReferenceException in UI
- Verify all UI components linked in Inspector
- Check TextMeshPro fields aren't null
- Ensure Canvas exists

### Mobile Issues

**Problem:** Cannot scan QR code
- Manually type URL in browser
- Ensure camera permissions enabled

**Problem:** Can zoom/pinch on mobile
- Check style.css has `user-scalable=no`
- Try different browser (Chrome vs Safari)

**Problem:** Button not responding
- Check browser console for errors (use Remote Debugging)
- Verify Socket.io connected (see status at top)

---

## Performance Tips

### Server
- Reduce logging in production for better performance
- Use `NODE_ENV=production npm start` for optimized mode

### Unity
- Use object pooling for mask displays (not critical for 60 players)
- Profile with Unity Profiler (Window → Analysis → Profiler)
- Disable vsync if frame rate drops: Edit → Project Settings → Quality → VSync Count = Don't Sync

### Mobile
- Close other browser tabs
- Use WiFi (not cellular)
- Keep phones plugged in (haptic feedback drains battery)

---

## Quick Commands Reference

```bash
# Start server
cd /Users/nils/Dev/GGJ26/server
npm install  # First time only
npm start

# Open Unity project
# Use Unity Hub to open: /Users/nils/Dev/GGJ26/unity

# View server logs
# Check terminal where server is running

# View Unity logs
# Check Unity Console window
```

---

## Next Steps

Once basic testing works:
1. Replace placeholder mask sprites with real artwork (see designer deliverables)
2. Fine-tune round timing (roundDuration, bonusRoundInterval)
3. Add visual polish (animations, effects)
4. Test with 30+ players
5. Prepare build for demo display

**Good luck at GGJ26! 🎮**
