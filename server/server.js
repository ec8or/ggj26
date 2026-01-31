const express = require('express');
const http = require('http');
const socketIO = require('socket.io');
const QRCode = require('qrcode');
const os = require('os');

const app = express();
const server = http.createServer(app);
const io = socketIO(server, {
  pingTimeout: 10000,
  pingInterval: 5000,
  maxHttpBufferSize: 1e6, // 1MB max message size
  transports: ['websocket', 'polling']
});

// Get local IP for QR code
function getLocalIP() {
  const interfaces = os.networkInterfaces();
  for (let name of Object.keys(interfaces)) {
    for (let iface of interfaces[name]) {
      if (iface.family === 'IPv4' && !iface.internal) {
        return iface.address;
      }
    }
  }
  return 'localhost';
}

const PORT = 3000;
const LOCAL_IP = getLocalIP();
const URL = `http://${LOCAL_IP}:${PORT}`;

// Serve mobile client
app.use(express.static('public'));

// Track connections
let players = new Map(); // socket.id → playerId
let unitySocket = null;

io.on('connection', (socket) => {
  console.log(`New connection: ${socket.id}`);

  // Detect if Unity or mobile client
  socket.on('identify', (data) => {
    if (data.type === 'unity') {
      unitySocket = socket;
      console.log('✅ Unity host connected');
    } else if (data.type === 'player') {
      players.set(socket.id, data.playerId);
      console.log(`✅ Player ${data.playerId} connected (Total: ${players.size})`);

      // Forward to Unity
      if (unitySocket) {
        unitySocket.emit('player_joined', {
          socketId: socket.id,
          playerId: data.playerId
        });
      }
    }
  });

  // Forward tap events to Unity
  socket.on('tap', (data) => {
    console.log(`👆 Tap from ${data.playerId}`);
    if (unitySocket) {
      unitySocket.emit('tap', {
        playerId: data.playerId,
        timestamp: data.timestamp || Date.now()
      });
    }
  });

  // Forward game state from Unity to all players
  socket.on('game_state', (data) => {
    io.emit('game_state', data);
  });

  // Forward mask assignment to specific player
  socket.on('mask_assigned', (data) => {
    const targetSocket = Array.from(players.entries())
      .find(([sid, pid]) => pid === data.playerId);
    if (targetSocket) {
      io.to(targetSocket[0]).emit('mask_assigned', data);
    }
  });

  // Forward eliminations to specific player
  socket.on('eliminated', (data) => {
    const targetSocket = Array.from(players.entries())
      .find(([sid, pid]) => pid === data.playerId);
    if (targetSocket) {
      io.to(targetSocket[0]).emit('eliminated', data);
      console.log(`💀 Player ${data.playerId} eliminated: ${data.reason}`);
    }
  });

  // Handle disconnect
  socket.on('disconnect', () => {
    if (socket === unitySocket) {
      console.log('❌ Unity disconnected');
      unitySocket = null;
    } else if (players.has(socket.id)) {
      const playerId = players.get(socket.id);
      console.log(`❌ Player ${playerId} disconnected (Total: ${players.size - 1})`);
      players.delete(socket.id);

      if (unitySocket) {
        unitySocket.emit('player_left', { playerId });
      }
    }
  });
});

// Start server
server.listen(PORT, '0.0.0.0', () => {
  console.log(`\n🎮 ========================================`);
  console.log(`   CROWD CONTROL - Relay Server Running`);
  console.log(`========================================`);
  console.log(`   Local:   http://localhost:${PORT}`);
  console.log(`   Network: ${URL}`);
  console.log(`========================================\n`);
  console.log(`📱 Scan QR code with mobile phones:\n`);

  // Generate QR code
  QRCode.toString(URL, { type: 'terminal', small: true }, (err, qr) => {
    if (!err) {
      console.log(qr);
    } else {
      console.log('⚠️  Could not generate QR code');
    }
  });

  console.log(`\n========================================`);
  console.log(`Waiting for Unity host to connect...`);
  console.log(`========================================\n`);
});
