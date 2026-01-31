const socket = io();

let playerId = null;
let maskId = null;
let isEliminated = false;
let tapCount = 0;
let currentRoundType = 'main'; // Track round type for Sprint optimizations
let sprintComplete = false;

// DOM elements
const statusEl = document.getElementById('status');
const maskImg = document.getElementById('mask-img');
const tapBtn = document.getElementById('tap-btn');
const eliminatedEl = document.getElementById('eliminated');
const eliminatedReason = document.getElementById('eliminated-reason');
const eliminatedPlace = document.getElementById('eliminated-place');

// Prevent accidental refresh
window.addEventListener('beforeunload', (e) => {
  if (!isEliminated) {
    e.preventDefault();
    e.returnValue = '';
  }
});

// Prevent zoom on iOS
document.addEventListener('gesturestart', (e) => {
  e.preventDefault();
});

// Connection
socket.on('connect', () => {
  playerId = socket.id;
  statusEl.textContent = 'Connected!';
  statusEl.classList.add('connected');
  statusEl.classList.remove('disconnected');

  // Identify as player
  socket.emit('identify', { type: 'player', playerId });
});

socket.on('disconnect', () => {
  statusEl.textContent = 'Disconnected';
  statusEl.classList.add('disconnected');
  statusEl.classList.remove('connected');
  tapBtn.classList.add('hidden');
});

// Receive mask assignment from Unity (via server)
socket.on('mask_assigned', (data) => {
  maskId = data.maskId;

  // Load mask sprite
  if (data.maskUrl) {
    maskImg.src = data.maskUrl;
  } else {
    // Use local PNG masks - browser will cache after first load
    maskImg.src = `/masks/mask_${maskId}.png`;

    // Fallback to colored background if image fails
    maskImg.onerror = () => {
      console.warn(`Failed to load mask_${maskId}.png, using colored placeholder`);
      maskImg.removeAttribute('src');
      const hue = (maskId * 6) % 360;
      maskImg.style.background = `hsl(${hue}, 70%, 50%)`;
      maskImg.style.width = '90%';
      maskImg.style.height = '90%';
      maskImg.style.display = 'block';
    };
  }

  tapBtn.classList.remove('hidden');
});

// Game state updates
socket.on('game_state', (data) => {
  console.log('Game state:', data);

  // Clean up victory overlay from previous Sprint round
  const victoryDiv = document.getElementById('sprint-victory');
  if (victoryDiv) {
    victoryDiv.remove();
  }

  // Show tap button again for new round
  if (!isEliminated) {
    tapBtn.classList.remove('hidden');
  }

  // Detect round type from Unity's game_state message
  // Unity now sends: { phase: 'playing', roundType: 'sprint', ... }
  if (data.roundType) {
    currentRoundType = data.roundType; // 'main', 'sprint', 'reaction', 'precision', 'advanced'
  }

  // Reset tap count when new round starts
  if (data.phase === 'playing') {
    tapCount = 0;
    sprintComplete = false;
  }
});

// Tap button
tapBtn.addEventListener('click', () => {
  if (isEliminated || sprintComplete) return;

  tapCount++;

  // BANDWIDTH OPTIMIZATION: Send every 5th tap (Unity multiplies by 5)
  // Reduces 4,000 messages to 800 messages (80% savings!)
  const shouldBroadcast = currentRoundType === 'sprint'
    ? (tapCount % 5 === 0 || tapCount === 1) // First tap + every 5th tap
    : true; // Every tap during normal rounds

  if (shouldBroadcast) {
    socket.emit('tap', {
      playerId,
      timestamp: Date.now()
    });
  }

  // SPRINT ROUND: Show victory at 100 taps
  if (currentRoundType === 'sprint' && tapCount >= 100 && !sprintComplete) {
    sprintComplete = true;
    showSprintVictory();
  }

  // Haptic feedback
  if (navigator.vibrate) {
    navigator.vibrate(50);
  }

  // Visual feedback
  tapBtn.classList.add('tapped');
  setTimeout(() => {
    tapBtn.classList.remove('tapped');
  }, 200);
});

// Eliminated
socket.on('eliminated', (data) => {
  isEliminated = true;
  tapBtn.classList.add('hidden');
  eliminatedReason.textContent = getReason(data.reason);

  if (data.playersRemaining !== undefined) {
    const place = data.playersRemaining + 1;
    eliminatedPlace.textContent = `Place: ${getOrdinal(place)}`;
  }

  eliminatedEl.classList.remove('hidden');

  // Triple vibrate for elimination
  if (navigator.vibrate) {
    navigator.vibrate([200, 100, 200]);
  }
});

function getReason(reason) {
  const reasons = {
    'wrong_tap': 'Tapped when you shouldn\'t have!',
    'missed_tap': 'Didn\'t tap when you should have!',
    'too_slow': 'Too slow!',
    'too_early': 'Tapped too early!',
    'failed_bonus': 'Failed bonus round!'
  };
  return reasons[reason] || 'Eliminated!';
}

function getOrdinal(n) {
  const s = ['th', 'st', 'nd', 'rd'];
  const v = n % 100;
  return n + (s[(v - 20) % 10] || s[v] || s[0]);
}

// SPRINT ROUND: Show victory message when player reaches 100 taps
function showSprintVictory() {
  // Hide tap button - player is done!
  tapBtn.classList.add('hidden');

  // Create victory overlay
  const victoryDiv = document.createElement('div');
  victoryDiv.id = 'sprint-victory';
  victoryDiv.innerHTML = `
    <h1>🏆 YOU WIN! 🏆</h1>
    <p>100/100 TAPS!</p>
    <p style="font-size: 18px; margin-top: 20px;">You're safe! Wait for others to finish...</p>
  `;
  document.body.appendChild(victoryDiv);

  // Triple vibrate for celebration
  if (navigator.vibrate) {
    navigator.vibrate([100, 50, 100, 50, 100]);
  }
}
