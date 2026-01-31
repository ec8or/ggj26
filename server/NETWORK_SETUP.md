# Network Setup & Troubleshooting

**Problem**: Local IP (192.168.x.x) not accessible from phones
**Last Updated**: 2026-01-30

---

## Why Local IP Doesn't Work

Common causes:
1. **Firewall blocking port 3000** on your computer
2. **Router client isolation** (phones can't talk to each other or computer)
3. **Different networks** (computer on ethernet, phones on WiFi)
4. **VPN active** on computer or phones
5. **Corporate/university network** with restrictions

---

## ❌ Why Static Hosting Won't Work

**Netlify/Vercel/GitHub Pages**: These host **static files** only
- ❌ No Node.js runtime
- ❌ No WebSocket support
- ❌ Cannot run `server.js`

**What you need**: A server that can:
- ✅ Run Node.js continuously
- ✅ Accept WebSocket connections
- ✅ Be accessible from phones

---

## ✅ Recommended Solutions (Best to Worst)

### Option 1: ngrok (EASIEST - 5 minutes) ⭐

**What it does**: Creates public URL tunnel to your local server

**Setup**:
```bash
# 1. Install ngrok (one-time)
brew install ngrok    # Mac
# OR download from https://ngrok.com/download

# 2. Start your server
cd /Users/nils/Dev/GGJ26/server
npm start    # Running on localhost:3000

# 3. In another terminal, start ngrok
ngrok http 3000
```

**Output**:
```
Forwarding  https://abc123.ngrok.io -> http://localhost:3000
```

**Use the ngrok URL**:
- Update Unity NetworkManager: `https://abc123.ngrok.io`
- Phones can now connect from anywhere!
- New QR code will use ngrok URL

**Pros**:
- ✅ Works instantly
- ✅ No network configuration needed
- ✅ Phones can be on any network (even cellular!)
- ✅ HTTPS support (better browser compatibility)
- ✅ Free tier available

**Cons**:
- ⚠️ URL changes each restart (unless paid plan)
- ⚠️ Slight latency (~50-100ms added)
- ⚠️ Free tier has connection limits

**Best for**: Game jam, testing, demos

---

### Option 2: Fix Local Network (10-30 minutes)

**Step 1: Check Firewall**

Mac:
```bash
# Allow port 3000
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --add $(which node)
sudo /usr/libexec/ApplicationFirewall/socketfilterfw --unblockapp $(which node)

# OR: System Preferences → Security & Privacy → Firewall → Firewall Options
# Allow incoming connections for Node
```

Windows:
```powershell
# Windows Firewall → Allow an app
# Add Node.js, allow both Private and Public networks
```

Linux:
```bash
sudo ufw allow 3000/tcp
```

**Step 2: Check Router Settings**

Access router admin (usually `192.168.1.1` or `192.168.0.1`):
- Look for "AP Isolation" or "Client Isolation"
- **Disable it** (it blocks devices from talking to each other)
- Look for "Guest Network" - don't use guest network!

**Step 3: Verify Computer IP**

```bash
# Mac/Linux
ifconfig | grep "inet "

# Windows
ipconfig

# Look for 192.168.x.x address (not 127.0.0.1)
```

**Step 4: Test Connection**

From phone browser, try:
```
http://192.168.86.173:3000
```

If you see the mobile UI, it works!

**Pros**:
- ✅ No external services needed
- ✅ Low latency (same network)
- ✅ No internet required

**Cons**:
- ⚠️ Network-specific (won't work on other WiFi)
- ⚠️ Can be tricky to debug
- ⚠️ Might not work on restricted networks

**Best for**: Final demo if you control the network

---

### Option 3: Personal Hotspot (15 minutes)

**Setup**:
1. Enable Personal Hotspot on your phone (iPhone/Android)
2. Connect computer to phone's hotspot
3. Connect other test phones to same hotspot
4. Get computer's IP on hotspot network: `ifconfig` (look for 172.x.x.x)
5. Use that IP in server/QR code

**Pros**:
- ✅ You control the network (no router issues)
- ✅ Works anywhere
- ✅ Portable setup

**Cons**:
- ⚠️ Uses cellular data
- ⚠️ Limited to ~5-10 connected devices
- ⚠️ Battery drain on host phone
- ⚠️ Slower than WiFi

**Best for**: Quick tests, outdoor demos

---

### Option 4: Tailscale (30 minutes setup)

**What it does**: Private VPN between your devices

**Setup**:
```bash
# 1. Install Tailscale on computer
brew install tailscale    # Mac
# OR download from https://tailscale.com/download

# 2. Start Tailscale
sudo tailscale up

# 3. Get your Tailscale IP
tailscale ip -4
# Output: 100.x.x.x

# 4. Install Tailscale app on EACH phone
# iOS: App Store → "Tailscale"
# Android: Play Store → "Tailscale"

# 5. Sign in to same account on all devices
```

**Use Tailscale IP**:
- Update server URL: `http://100.x.x.x:3000`
- All devices must have Tailscale running

**Pros**:
- ✅ Works on any network (even cellular + WiFi mixed)
- ✅ Secure and encrypted
- ✅ Stable IPs (don't change)
- ✅ Free for personal use

**Cons**:
- ❌ Must install on EVERY phone (tedious for 60 players!)
- ⚠️ All players need Tailscale account
- ⚠️ Complex setup for non-technical users

**Best for**: Small team testing, not for 60-player demos

---

### Option 5: Cloud VPS Deployment (1-2 hours)

**Services**: Railway, Render, Fly.io, DigitalOcean, Heroku

**Example: Railway (easiest)**

```bash
# 1. Install Railway CLI
npm install -g @railway/cli

# 2. Login
railway login

# 3. Deploy
cd /Users/nils/Dev/GGJ26/server
railway init
railway up

# 4. Get public URL
railway domain
# Output: https://your-app.railway.app
```

**Update Unity**:
```csharp
serverUrl = "https://your-app.railway.app"
```

**Pros**:
- ✅ Professional setup
- ✅ Always accessible
- ✅ No network issues
- ✅ Stable URL
- ✅ Scales to 100+ players

**Cons**:
- ⚠️ Costs money after free tier (~$5-10/month)
- ⚠️ Requires credit card
- ⚠️ Takes time to set up
- ⚠️ Needs deployment process

**Best for**: Production, long-term hosting

---

## Quick Decision Tree

```
Do you have 1 hour to set up?
  NO  → Use ngrok (Option 1) ⭐
  YES → Continue...

Is the network restricted (school/corporate)?
  YES → Use ngrok (Option 1) ⭐
  NO  → Continue...

Do you control the WiFi router?
  YES → Fix local network (Option 2)
  NO  → Continue...

Need to test with 5-10 phones quickly?
  YES → Personal hotspot (Option 3)
  NO  → Continue...

Need to scale to 60+ players long-term?
  YES → Cloud VPS (Option 5)
  NO  → Use ngrok (Option 1) ⭐
```

---

## Step-by-Step: ngrok Setup (Recommended)

### 1. Install ngrok

**Mac**:
```bash
brew install ngrok
```

**Windows**:
Download from https://ngrok.com/download

**Linux**:
```bash
curl -s https://ngrok-agent.s3.amazonaws.com/ngrok.asc | \
  sudo tee /etc/apt/trusted.gpg.d/ngrok.asc >/dev/null && \
  echo "deb https://ngrok-agent.s3.amazonaws.com buster main" | \
  sudo tee /etc/apt/sources.list.d/ngrok.list && \
  sudo apt update && sudo apt install ngrok
```

### 2. Sign up for free account

```bash
# Go to https://dashboard.ngrok.com/signup
# Copy your authtoken

# Configure
ngrok config add-authtoken YOUR_TOKEN_HERE
```

### 3. Start your server

```bash
cd /Users/nils/Dev/GGJ26/server
npm start
```

Output:
```
🎮 CROWD CONTROL - Relay Server Running
   Local:   http://localhost:3000
   Network: http://192.168.86.173:3000
```

### 4. Start ngrok (in new terminal)

```bash
ngrok http 3000
```

Output:
```
Session Status                online
Account                       your-email@example.com
Region                        United States (us)
Forwarding                    https://abc123xyz.ngrok.io -> http://localhost:3000
```

### 5. Update Unity

Open Unity:
- Select GameManager
- Find NetworkManager component
- Change "Server Url" to: `https://abc123xyz.ngrok.io`
- ⚠️ **No trailing slash!**
- ⚠️ **Use HTTPS, not HTTP**

### 6. Test on phone

Open phone browser:
```
https://abc123xyz.ngrok.io
```

Should see mobile controller UI!

### 7. Generate new QR code

The server QR code shows local IP. For ngrok, you need to manually generate QR:

**Option A**: Use online QR generator
- Go to: https://www.qr-code-generator.com/
- Enter: `https://abc123xyz.ngrok.io`
- Download QR code image
- Show on screen for scanning

**Option B**: Update server to show ngrok URL
Edit `server.js` to generate QR for ngrok URL instead of local IP

---

## Testing Checklist

After setup, verify:
- [ ] Server running: `npm start` shows no errors
- [ ] Tunnel/network active
- [ ] Unity connected: Console shows "✅ Connected to server"
- [ ] Phone can access URL in browser
- [ ] Phone shows "Connected!" status (green)
- [ ] Phone tap reaches Unity (check Unity console)

---

## Common Issues

### Issue: "Connection refused" on phone

**Cause**: Server not accessible

**Solutions**:
1. Check firewall (Option 2, Step 1)
2. Verify server running
3. Try ngrok (Option 1)

---

### Issue: "WebSocket connection failed"

**Cause**: Protocol mismatch (HTTP vs HTTPS)

**Solutions**:
1. ngrok: Use `https://` in Unity (not `http://`)
2. Local: Use `http://` (not `https://`)
3. Check Unity server URL has no trailing slash

---

### Issue: "This site can't be reached"

**Cause**: Wrong IP or network issue

**Solutions**:
1. Ping the IP: `ping 192.168.86.173`
2. Check computer and phone on same WiFi
3. Try ngrok (Option 1)

---

### Issue: ngrok "Session Expired"

**Cause**: Free tier session timeout (8 hours)

**Solution**: Restart ngrok
```bash
# Press Ctrl+C to stop ngrok
ngrok http 3000    # Start again
```

New URL will be different - update Unity!

---

### Issue: "Too many connections" on ngrok

**Cause**: Free tier limit reached

**Solutions**:
1. Restart ngrok (resets counter)
2. Upgrade to paid plan ($10/month)
3. Use alternative tunnel (localtunnel, serveo)

---

## Alternative Tunneling Services

If ngrok doesn't work:

### localtunnel
```bash
npm install -g localtunnel
lt --port 3000
```

### serveo
```bash
ssh -R 80:localhost:3000 serveo.net
```

### Cloudflare Tunnel
```bash
cloudflared tunnel --url http://localhost:3000
```

---

## For Game Jam Day

**Recommended setup**:

1. **Development** (your machine):
   - Use ngrok for testing
   - Fast iteration

2. **Demo Day** (shared screen):
   - If venue has good WiFi: Fix local network (Option 2)
   - If venue WiFi is bad: Personal hotspot (Option 3)
   - Backup plan: Keep ngrok running

3. **Have ready**:
   - ngrok installed and tested
   - Personal hotspot as backup
   - QR codes pre-generated for both scenarios

---

## Network Requirements

**Minimum**:
- Latency: <500ms (for tap events)
- Bandwidth: ~10KB/s per player
- For 60 players: ~600KB/s total (~5 Mbps)

**Recommended**:
- Latency: <200ms
- Bandwidth: 10 Mbps upload (venue WiFi)
- Stable connection for 30 minutes

---

## Quick Reference Commands

```bash
# Get your local IP (Mac/Linux)
ifconfig | grep "inet " | grep -v 127.0.0.1

# Get your local IP (Windows)
ipconfig | findstr IPv4

# Test if port 3000 is accessible
curl http://192.168.86.173:3000

# Check what's using port 3000
lsof -i :3000    # Mac/Linux
netstat -ano | findstr :3000    # Windows

# Start ngrok
ngrok http 3000

# Kill ngrok
Ctrl+C (in ngrok terminal)
```

---

**Recommended for GGJ26**: Use ngrok (Option 1) for fastest setup! 🚀

---

**Last Updated**: 2026-01-30
**Tested On**: Mac, Windows, iOS Safari, Android Chrome
