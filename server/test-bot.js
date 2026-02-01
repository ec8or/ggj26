/**
 * Test Bot - Simulates multiple players for testing
 *
 * Usage:
 *   npm run bots              (spawns 40 bots on localhost:3000)
 *   node test-bot.js [numBots] [serverUrl]
 *
 * Examples:
 *   node test-bot.js 10
 *   node test-bot.js 50 http://192.168.1.100:3000
 *
 * Press Ctrl+C to disconnect all bots
 */

const io = require('socket.io-client');

const NUM_BOTS = parseInt(process.argv[2]) || 10;
const SERVER_URL = process.argv[3] || 'http://localhost:3000';

console.log(`🤖 Starting ${NUM_BOTS} test bots...`);
console.log(`📡 Connecting to: ${SERVER_URL}\n`);

const bots = [];

class Bot {
  constructor(id) {
    this.id = id;
    this.socket = null;
    this.maskId = null;
    this.isAlive = true;
    this.autoTap = false; // Disabled by default - just for UI testing
  }

  connect() {
    this.socket = io(SERVER_URL);

    this.socket.on('connect', () => {
      console.log(`✅ Bot ${this.id} connected`);
      this.socket.emit('identify', {
        type: 'player',
        playerId: this.socket.id
      });
    });

    this.socket.on('mask_assigned', (data) => {
      this.maskId = data.maskId;
      console.log(`🎭 Bot ${this.id} assigned Mask #${this.maskId}`);
    });

    this.socket.on('game_state', (data) => {
      // Bot could react to game state here
      if (data.phase === 'playing' && this.autoTap && this.isAlive) {
        // Random delay before tapping (0-2 seconds)
        const delay = Math.random() * 2000;
        setTimeout(() => {
          if (this.isAlive) {
            this.tap();
          }
        }, delay);
      }
    });

    this.socket.on('eliminated', (data) => {
      this.isAlive = false;
      console.log(`💀 Bot ${this.id} (Mask #${this.maskId}) eliminated: ${data.reason}`);
    });

    this.socket.on('disconnect', () => {
      console.log(`❌ Bot ${this.id} disconnected`);
    });
  }

  tap() {
    if (!this.socket || !this.isAlive) return;

    this.socket.emit('tap', {
      playerId: this.socket.id,
      timestamp: Date.now()
    });
  }

  disconnect() {
    if (this.socket) {
      this.socket.disconnect();
    }
  }
}

// Create and connect bots with staggered timing
async function startBots() {
  for (let i = 1; i <= NUM_BOTS; i++) {
    const bot = new Bot(i);
    bot.connect();
    bots.push(bot);

    // Stagger connections to avoid overwhelming server
    if (i < NUM_BOTS) {
      await new Promise(resolve => setTimeout(resolve, 100));
    }
  }

  console.log(`\n✅ All ${NUM_BOTS} bots connected!`);
  console.log('💡 Auto-tap is DISABLED (bots just sit there for UI testing)');
  console.log('Press Ctrl+C to disconnect all bots\n');
}

// Keep process alive and handle shutdown
process.on('SIGINT', () => {
  console.log('\n🛑 Disconnecting all bots...');
  bots.forEach(bot => bot.disconnect());
  setTimeout(() => {
    console.log('✅ All bots disconnected');
    process.exit(0);
  }, 500);
});

// Start the bots
startBots();
