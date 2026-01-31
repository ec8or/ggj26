const fs = require('fs');
const { createCanvas } = require('canvas');
const path = require('path');

const OUTPUT_DIR = path.join(__dirname, 'public', 'masks');
const NUM_MASKS = 60;
const IMAGE_SIZE = 200;

// Generate random color
function randomColor() {
  const r = Math.floor(Math.random() * 256);
  const g = Math.floor(Math.random() * 256);
  const b = Math.floor(Math.random() * 256);
  return `rgb(${r}, ${g}, ${b})`;
}

// Generate a single mask PNG
function generateMask(maskId) {
  const canvas = createCanvas(IMAGE_SIZE, IMAGE_SIZE);
  const ctx = canvas.getContext('2d');

  // Random background color
  ctx.fillStyle = randomColor();
  ctx.fillRect(0, 0, IMAGE_SIZE, IMAGE_SIZE);

  // Add mask number in white
  ctx.fillStyle = 'white';
  ctx.font = 'bold 48px sans-serif';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillText(`#${maskId}`, IMAGE_SIZE / 2, IMAGE_SIZE / 2);

  // Save to file
  const buffer = canvas.toBuffer('image/png');
  const filename = path.join(OUTPUT_DIR, `mask_${maskId}.png`);
  fs.writeFileSync(filename, buffer);
  console.log(`Created ${filename}`);
}

// Generate all masks
console.log(`Generating ${NUM_MASKS} placeholder masks...`);
for (let i = 0; i < NUM_MASKS; i++) {
  generateMask(i);
}
console.log(`✓ Generated ${NUM_MASKS} masks in ${OUTPUT_DIR}`);
