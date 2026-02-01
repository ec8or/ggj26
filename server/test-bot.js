/**
 * Test Bot - Simulates multiple players for testing
 *
 * Usage:
 *   npm run bots              (spawns 40 bots on localhost:3000)
 *   node test-bot.js [numBots] [serverUrl] [tapsPerSecond]
 *
 * Examples:
 *   node test-bot.js 10
 *   node test-bot.js 50 http://192.168.1.100:3000
 *   node test-bot.js 40 http://localhost:3000 2    (2 taps/second)
 *   node test-bot.js 40 http://localhost:3000 0.5  (1 tap every 2 seconds)
 *
 * Press Ctrl+C to disconnect all bots
 */

const io = require('socket.io-client');

const NUM_BOTS = parseInt(process.argv[2]) || 10;
const SERVER_URL = process.argv[3] || 'http://localhost:3000';
const TAPS_PER_SECOND = parseFloat(process.argv[4]) || 0;

console.log(`🤖 Starting ${NUM_BOTS} test bots...`);
console.log(`📡 Connecting to: ${SERVER_URL}`);
if (TAPS_PER_SECOND > 0) {
  console.log(`⚡ Auto-tap: ${TAPS_PER_SECOND} taps/second\n`);
} else {
  console.log(`💤 Auto-tap: DISABLED\n`);
}

const bots = [];

class Bot {
  constructor(id, tapsPerSecond) {
    this.id = id;
    this.socket = null;
    this.maskId = null;
    this.isAlive = true;
    this.tapsPerSecond = tapsPerSecond;
    this.autoTap = tapsPerSecond > 0;
    this.tapInterval = null;
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
      // Start/stop auto-tapping based on game state
      if (data.phase === 'playing' && this.autoTap && this.isAlive) {
        this.startAutoTap();
      } else {
        this.stopAutoTap();
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

  startAutoTap() {
    if (this.tapInterval) return; // Already tapping

    const baseIntervalMs = 1000 / this.tapsPerSecond;

    // Add random initial delay (0-50% of interval) so bots don't start in sync
    const initialDelay = Math.random() * baseIntervalMs * 0.5;

    setTimeout(() => {
      if (!this.isAlive) return;

      // Tap with randomized intervals to prevent lockstep
      const tapWithRandomness = () => {
        if (this.isAlive) {
          this.tap();

          // Add ±20% randomness to each tap interval
          const variance = baseIntervalMs * 0.2;
          const randomInterval = baseIntervalMs + (Math.random() - 0.5) * variance;

          this.tapInterval = setTimeout(tapWithRandomness, randomInterval);
        } else {
          this.stopAutoTap();
        }
      };

      tapWithRandomness();
    }, initialDelay);
  }

  stopAutoTap() {
    if (this.tapInterval) {
      clearTimeout(this.tapInterval);
      this.tapInterval = null;
    }
  }

  tap() {
    if (!this.socket || !this.isAlive) return;

    this.socket.emit('tap', {
      playerId: this.socket.id,
      timestamp: Date.now()
    });
  }

  disconnect() {
    this.stopAutoTap();
    if (this.socket) {
      this.socket.disconnect();
    }
  }
}

// Create and connect bots with staggered timing
async function startBots() {
  for (let i = 1; i <= NUM_BOTS; i++) {
    const bot = new Bot(i, TAPS_PER_SECOND);
    bot.connect();
    bots.push(bot);

    // Stagger connections to avoid overwhelming server
    if (i < NUM_BOTS) {
      await new Promise(resolve => setTimeout(resolve, 100));
    }
  }

  console.log(`\n✅ All ${NUM_BOTS} bots connected!`);
  if (TAPS_PER_SECOND > 0) {
    console.log(`⚡ Bots will auto-tap at ${TAPS_PER_SECOND} taps/second during rounds`);
  } else {
    console.log('💡 Auto-tap is DISABLED (bots just sit there for UI testing)');
  }
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
