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
 *   node test-bot.js 40 http://localhost:3000 2    (2 taps/second for sprint)
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
  console.log(`⚡ Auto-tap enabled with smart behaviors per round type\n`);
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
    this.currentRoundType = null;
    this.roundActive = false;
    this.behaviorTimeout = null;
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
      this.currentRoundType = data.roundType;

      if (data.phase === 'playing' && this.autoTap && this.isAlive) {
        this.roundActive = true;
        this.startRoundBehavior();
      } else {
        this.roundActive = false;
        this.stopBehavior();
      }
    });

    this.socket.on('eliminated', (data) => {
      this.isAlive = false;
      this.stopBehavior();
      console.log(`💀 Bot ${this.id} (Mask #${this.maskId}) eliminated: ${data.reason}`);
    });

    this.socket.on('disconnect', () => {
      console.log(`❌ Bot ${this.id} disconnected`);
    });
  }

  startRoundBehavior() {
    this.stopBehavior(); // Clear any existing behavior

    if (!this.roundActive || !this.isAlive) return;

    switch (this.currentRoundType) {
      case 'snap':
      case 'advanced':
        this.snapBehavior();
        break;
      case 'sprint':
        this.sprintBehavior();
        break;
      case 'reaction':
        this.reactionBehavior();
        break;
      case 'precision':
        this.precisionBehavior();
        break;
      default:
        // Unknown round type, use snap behavior as default
        this.snapBehavior();
    }
  }

  // SNAP/ADVANCED: Aggressive tapping to simulate active players
  snapBehavior() {
    const checkAndTap = () => {
      if (!this.roundActive || !this.isAlive) return;

      const rand = Math.random();

      if (rand < 0.70) {
        // 70% chance to tap (simulating "your mask is shown and you tap")
        const delay = Math.random() * 3000; // Random delay 0-3s
        this.behaviorTimeout = setTimeout(() => {
          if (this.roundActive && this.isAlive) {
            this.tap();
          }
        }, delay);
      } else if (rand < 0.80) {
        // 10% chance to tap (wrong tap - mask not shown but you tap anyway)
        const delay = Math.random() * 3000;
        this.behaviorTimeout = setTimeout(() => {
          if (this.roundActive && this.isAlive) {
            this.tap();
          }
        }, delay);
      }
      // Else: 20% don't tap (simulating mask not shown OR player missed it)
    };

    checkAndTap();
  }

  // SPRINT: Continuous tapping at specified rate
  sprintBehavior() {
    const baseIntervalMs = 1000 / this.tapsPerSecond;
    const initialDelay = Math.random() * baseIntervalMs * 0.5;

    this.behaviorTimeout = setTimeout(() => {
      if (!this.roundActive || !this.isAlive) return;

      const tapRepeatedly = () => {
        if (this.roundActive && this.isAlive) {
          this.tap();

          // Add ±20% randomness
          const variance = baseIntervalMs * 0.2;
          const randomInterval = baseIntervalMs + (Math.random() - 0.5) * variance;

          this.behaviorTimeout = setTimeout(tapRepeatedly, randomInterval);
        }
      };

      tapRepeatedly();
    }, initialDelay);
  }

  // REACTION: Very small chance to tap early (during red), then random reaction time
  reactionBehavior() {
    // 2% chance to tap too early (during red light)
    if (Math.random() < 0.02) {
      const earlyTapDelay = Math.random() * 2000; // Tap within first 2 seconds
      this.behaviorTimeout = setTimeout(() => {
        if (this.roundActive && this.isAlive) {
          this.tap(); // Will be eliminated for tapping during red
        }
      }, earlyTapDelay);
      return;
    }

    // Otherwise, wait for "green light" (assume it starts after 2-5s)
    // Then tap with random reaction time
    const redLightWait = 2000 + Math.random() * 3000; // 2-5 seconds
    const reactionTime = 100 + Math.random() * 1500; // 100-1600ms reaction

    this.behaviorTimeout = setTimeout(() => {
      if (this.roundActive && this.isAlive) {
        this.tap();
      }
    }, redLightWait + reactionTime);
  }

  // PRECISION: Tap at random time between 0-10 seconds
  precisionBehavior() {
    const tapTime = Math.random() * 10000; // Random time 0-10s

    this.behaviorTimeout = setTimeout(() => {
      if (this.roundActive && this.isAlive) {
        this.tap();
      }
    }, tapTime);
  }

  stopBehavior() {
    if (this.behaviorTimeout) {
      clearTimeout(this.behaviorTimeout);
      this.behaviorTimeout = null;
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
    this.stopBehavior();
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
    console.log(`🧠 Smart behaviors enabled:`);
    console.log(`   - Snap/Advanced: 70% correct tap, 10% wrong tap, 20% no tap`);
    console.log(`   - Sprint: ${TAPS_PER_SECOND} taps/second`);
    console.log(`   - Reaction: Random reaction times, 2% early tap`);
    console.log(`   - Precision: Random tap time 0-10s`);
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
