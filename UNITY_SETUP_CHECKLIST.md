# Unity Setup Checklist

Use this checklist when setting up the Unity scene manually.

---

Part 1-6 DONE

---

## ☐ Part 7: Test with Real Phone (5 min)

### Connect Phone
- [ ] Server running: `npm start`
- [ ] Unity in Play mode
- [ ] Scan QR code with phone (or manually open URL)
- [ ] Phone should show:
  - [ ] "Connected!" status (green)
  - [ ] Mask sprite
  - [ ] "Mask #1"
  - [ ] Large TAP button

### Test Tap Events
- [ ] Tap button on phone
- [ ] Check Unity Console for:
  - [ ] "👤 Player joined: [id]"
  - [ ] "👆 Tap from [id]"

### Test Game Start
- [ ] Press SPACE in Unity Editor
- [ ] QR Code panel should hide
- [ ] Game should start
- [ ] Console shows: "🎮 GAME STARTING!"
- [ ] After few seconds, masks appear on screen

---

## ☐ Troubleshooting

### "SocketIOUnity not found" error
- [ ] Verify package installed in Package Manager
- [ ] Try: Assets → Reimport All
- [ ] Try: Edit → Preferences → External Tools → Regenerate project files

### "TextMeshPro not found" error
- [ ] Window → TextMeshPro → Import TMP Essential Resources
- [ ] Restart Unity

### NullReferenceException errors
- [ ] Check all Inspector references are linked
- [ ] Ensure GameManager has all scripts attached
- [ ] Verify UI objects exist in scene

### Cannot connect to server
- [ ] Verify server is running: `cd server && npm start`
- [ ] Check server URL in NetworkManager matches
- [ ] Check firewall isn't blocking port 3000
- [ ] Try with localhost first, then network IP

### Masks don't appear on screen
- [ ] Check MaskManager has "Mask Display Container" linked
- [ ] Check MaskManager has "Mask Prefab" linked
- [ ] Check "Use Placeholder Sprites" is checked
- [ ] Check Console for sprite creation logs

---

## ☐ Final Verification

### Scene Complete Checklist
- [ ] MainGame scene saved in `Assets/Scenes/`
- [ ] GameManager exists with all 9 scripts
- [ ] Canvas exists with all UI elements
- [ ] All Inspector references linked
- [ ] No errors in Console when entering Play mode
- [ ] Server connection successful
- [ ] Phone can connect and tap
- [ ] SPACE key starts game

---

## Next Steps

Once checklist complete:
1. Test with 3+ phones
2. Play through a full game
3. Test bonus rounds (round 5, 10, 15)
4. Run stress test with test-bot.js
5. Replace placeholder sprites with real artwork
6. Add polish (animations, effects)

---

**Estimated Time**: 45-60 minutes for full setup

**Need Help?** See `/Users/nils/Dev/GGJ26/SETUP.md` for detailed instructions.
