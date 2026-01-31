# Designer Brief - Crowd Control GGJ26

**Project**: 50-60 Player Battle Royale Mobile Game
**Role**: Mask Sprite Artist
**Deadline**: Hour 30 of Game Jam
**Deliverables**: 60 unique mask sprites (512x512px PNG)

---

## Overview

Players control masks on a shared screen using their mobile phones. **The masks are the players' identities** - they need to be:
- **Instantly recognizable** from 10 feet away
- **Visually distinct** from each other
- **Hand-drawn/sketchy style** (not photorealistic)
- **Colorful and expressive**

**Critical**: Masks should be **intentionally similar within categories** but **clearly different between categories**. This creates gameplay tension - players must focus to find their specific mask among similar ones.

---

## Mask Categories (10 masks per category)

### Category 1: Hannya / Tengu (The Myths)
**Vibe**: Traditional Japanese Folklore
**Quantity**: 10 masks (mask_001.png to mask_010.png)

**Key Shapes**:
- Long pointed noses (Tengu)
- Sharp horns (Hannya)
- Bared teeth, fierce expressions
- Angular, dramatic features

**Colors**:
- Deep reds, blacks, and golds
- Crimson, blood red, burgundy
- Black outlines, gold accents

**Recognition Factor**:
- The only set with sharp, protruding "pokes" (horns/noses)
- Most aggressive/demonic appearance
- Traditional Japanese aesthetic

**Examples**:
- Hannya (demon with horns and fangs)
- Tengu (red long-nosed goblin)
- Oni (ogre with horns)

---

### Category 2: Luchador (The Brawlers)
**Vibe**: Mexican Wrestling
**Quantity**: 10 masks (mask_011.png to mask_020.png)

**Key Shapes**:
- Full-head fabric wraps
- Bold cutouts for eyes and mouth
- Smooth, tight-fitting appearance
- Geometric patterns

**Colors**:
- High-contrast vibrants
- Neon Pink/Yellow
- Blue/Silver
- Red/Gold
- Green/Purple

**Recognition Factor**:
- Bold, geometric patterns on forehead/crown:
  - Lightning bolts
  - Stars
  - Flames
  - Stripes
- Most colorful and high-contrast category

**Examples**:
- El Santo style (silver mask)
- Rey Mysterio style (split colors)
- Blue Demon style (solid with star)

---

### Category 3: Theatre / Culture (The Classics)
**Vibe**: Venetian Masquerade & Greek Drama
**Quantity**: 10 masks (mask_021.png to mask_030.png)

**Key Shapes**:
- Porcelain faces (smooth, human-like)
- Jester hats with bells
- Feathered plumes
- "Guy Fawkes" (Anonymous) mustache style
- Half-masks (covering only eyes/upper face)

**Colors**:
- White, ivory, cream
- Gold, silver
- Deep purple, royal blue
- Velvet textures suggested

**Recognition Factor**:
- The "human-ish" faces
- Tall hats or ornate accessories
- Most elegant/refined appearance
- Theatrical exaggeration (comedy/tragedy)

**Examples**:
- Venetian half-mask with feathers
- Jester with tri-point hat
- Comedy/tragedy theatre masks
- Guy Fawkes mask
- Phantom of the Opera style

---

### Category 4: Animal (The Wild)
**Vibe**: Totemic or Hotline Miami-esque
**Quantity**: 10 masks (mask_031.png to mask_040.png)

**Key Shapes**:
- Antlers (Deer, Elk)
- Beaks (Owl, Raven, Plague Doctor)
- Snouts (Boar, Wolf, Fox)
- Ears (Rabbit, Cat, Horse)

**Colors**:
- Earth tones (browns, tans, grays)
- Natural fur/feather colors
- **Glowing eyes** (yellow, green, red)

**Recognition Factor**:
- Distinctive organic protrusions (ears/horns/beaks)
- Most "bestial" appearance
- Glowing eyes as signature feature

**Examples**:
- Deer skull with antlers
- Owl with large eyes
- Plague doctor with long beak
- Wolf with pointed ears
- Fox with sharp snout

---

### Category 5: Sports (The Gear)
**Vibe**: Extreme and Tactical
**Quantity**: 10 masks (mask_041.png to mask_050.png)

**Key Shapes**:
- Mesh grids (Fencing, Hockey)
- Goggle-heavy (Paintball, Skiing, Motocross)
- Full-vented (Diver, Motorcycle)
- Protective padding visible

**Colors**:
- Matte black
- Safety orange
- Carbon fiber gray
- White with colored accents

**Recognition Factor**:
- Heavy use of **grids** and **glass** textures
- Most "functional/protective" appearance
- Vents, straps, buckles visible

**Examples**:
- Fencing mask (mesh grid)
- Hockey goalie mask
- Paintball mask (dual goggles)
- Ski mask with goggles
- Motorcycle helmet
- Scuba diving mask

---

### Category 6: Tech / Futuristic (The Cyber)
**Vibe**: Daft Punk / Cyberpunk 2077
**Quantity**: 10 masks (mask_051.png to mask_060.png)

**Key Shapes**:
- Sleek visors (full-face coverage)
- LED matrix faces (pixelated displays)
- Antenna-heavy robot heads
- Smooth, minimal curves

**Colors**:
- Chrome, metallic silver
- Neon cyan, electric blue
- Electric lime green
- Hot pink accents

**Recognition Factor**:
- The only set with **emissive (glowing) parts**
- Most "futuristic/alien" appearance
- Digital/tech aesthetic
- LED screens, circuit patterns

**Examples**:
- Daft Punk helmet (smooth chrome)
- LED matrix face (emoji display)
- Cyberpunk visor (single glowing line for eyes)
- Robot head with antennas
- VR headset style

---

## Technical Specifications

### File Format:
- **Format**: PNG-24 (with alpha transparency)
- **Size**: 512x512 pixels (square)
- **Background**: Transparent (no background)
- **Color Mode**: RGBA

### File Naming:
**Critical**: Files must be named exactly as specified:
```
mask_001.png  (Hannya/Tengu #1)
mask_002.png  (Hannya/Tengu #2)
...
mask_010.png  (Hannya/Tengu #10)
mask_011.png  (Luchador #1)
...
mask_020.png  (Luchador #10)
mask_021.png  (Theatre #1)
...
mask_030.png  (Theatre #10)
mask_031.png  (Animal #1)
...
mask_040.png  (Animal #10)
mask_041.png  (Sports #1)
...
mask_050.png  (Sports #10)
mask_051.png  (Tech #1)
...
mask_060.png  (Tech #10)
```

**Use zero-padding**: `mask_001.png` NOT `mask_1.png`

### Art Style:
- **Hand-drawn aesthetic**: Sketchy lines, not photorealistic
- **Bold outlines**: 3-5px black strokes for readability
- **Flat colors** with optional simple shading
- **High contrast**: Must be visible from 10 feet away
- **Expressive**: Personality in each mask

### Canvas Layout:
```
┌─────────────────┐
│                 │  <- Leave ~50px padding
│    ┌───────┐    │
│    │ MASK  │    │  <- Mask centered
│    │ FACE  │    │     ~400px tall
│    └───────┘    │
│                 │  <- Leave ~50px padding
└─────────────────┘
    512x512px
```

- Keep mask within **~400x400px** center area
- Leave padding for Unity display scaling
- Center the mask horizontally and vertically

### Visibility Requirements:
Masks will be displayed:
- On a **shared 1920x1080 screen** (TV/projector)
- **3-5 masks at once** in a horizontal row
- Each mask approximately **400x400px** on screen
- Viewed from **10 feet away** by players

**Test**: Can you identify your specific mask among 5 similar ones from across the room?

---

## Design Guidelines

### DO:
✅ Make masks within a category **similar but distinguishable**
✅ Use bold, high-contrast colors
✅ Add unique details (patterns, accessories, expressions)
✅ Keep outlines thick and clear
✅ Test visibility by viewing from 10 feet away
✅ Add personality and character
✅ Make each mask memorable

### DON'T:
❌ Make masks photorealistic or too detailed
❌ Use subtle gradients or fine textures
❌ Make colors too similar within a category
❌ Add text or numbers to masks
❌ Make masks too small on canvas
❌ Use low-contrast color combinations
❌ Copy existing copyrighted designs exactly

---

## Deliverables Checklist

### Phase 1: Samples (Hour 12)
- [ ] 10 sample masks (2 from each category)
  - mask_001.png (Hannya/Tengu)
  - mask_011.png (Luchador)
  - mask_021.png (Theatre)
  - mask_031.png (Animal)
  - mask_041.png (Sports)
  - mask_051.png (Tech)
  - Plus 4 more for variety

**Deliverable**: Slack/email samples for approval

### Phase 2: Full Set (Hour 30)
- [ ] All 60 masks completed
- [ ] Proper naming (mask_001 to mask_060)
- [ ] All 512x512px PNG with transparency
- [ ] Organized by category

**Deliverable**: ZIP file or folder upload

### Phase 3: Revisions (Hour 36)
- [ ] Address feedback from playtesting
- [ ] Fix any masks that are too similar
- [ ] Adjust colors for visibility if needed

---

## Delivery Location

Place final mask files in:
```
/Users/nils/Dev/GGJ26/unity/Assets/Resources/Masks/
```

**Or** provide as ZIP file and developer will place them.

---

## Quality Checklist (Before Delivery)

For each mask, verify:
- [ ] 512x512px exactly
- [ ] PNG format with transparency
- [ ] Transparent background (no white/colored BG)
- [ ] Proper filename (mask_XXX.png with zero-padding)
- [ ] File size under 200KB (ideally 50-100KB)
- [ ] Mask centered on canvas
- [ ] Bold outlines visible
- [ ] High contrast colors
- [ ] Distinguishable from others in same category
- [ ] Looks good at 200px size (thumbnail test)

---

## Testing Your Work

### Thumbnail Test:
1. Place all 10 masks from one category in a row
2. Zoom out or step back 10 feet
3. Can you quickly identify each unique mask?
4. Are they distinct enough?

### Similarity Test:
- Hannya/Tengu: All should have pointed/sharp features
- Luchador: All should have geometric forehead patterns
- Theatre: All should have elegant/human faces
- Animal: All should have organic protrusions
- Sports: All should have grids/vents
- Tech: All should have glowing/emissive parts

### Contrast Test:
1. Convert to grayscale
2. Are masks still distinguishable?
3. If not, increase value contrast

---

## Reference Resources

### Style References:
- **Hannya/Tengu**: Google "traditional japanese demon masks"
- **Luchador**: Google "lucha libre masks" or "rey mysterio mask"
- **Theatre**: Google "venetian masquerade masks" or "guy fawkes mask"
- **Animal**: Google "hotline miami masks" or "tribal animal masks"
- **Sports**: Google "hockey goalie masks" or "paintball masks"
- **Tech**: Google "daft punk helmet" or "cyberpunk masks"

### Art Style References:
- Hand-drawn style: Similar to "Slay the Spire" art
- Bold outlines: Like "Hollow Knight" characters
- Colorful: Like "Enter the Gungeon" sprites

---

## Example Workflow (Per Mask)

1. **Sketch** (5 min):
   - Rough shape and key features
   - Decide on unique detail

2. **Outline** (5 min):
   - Bold 3-5px black strokes
   - Clean up sketch

3. **Color** (5 min):
   - Base colors (2-4 colors per mask)
   - Stay within category palette

4. **Details** (5 min):
   - Add patterns, shading, highlights
   - Add unique identifier (horns, pattern, etc.)

5. **Export** (1 min):
   - Save as PNG-24
   - Verify transparent background
   - Check filename

**Total per mask**: ~20 minutes
**Total for 60 masks**: ~20 hours (2-3 work days)

---

## Communication

### Questions During Development:
- Ask in team Slack/Discord
- Tag: @developer for technical questions
- Tag: @game-designer for aesthetic questions

### Feedback Loop:
- Upload samples to shared folder
- Team reviews in-game
- Note any masks that are too similar
- Iterate as needed

---

## Success Criteria

### Minimum Viable:
- ✅ 60 unique masks
- ✅ All properly named and formatted
- ✅ Distinguishable within categories
- ✅ Visible from 10 feet

### Target Quality:
- ✅ Cohesive style across all masks
- ✅ Personality in each design
- ✅ Instant category recognition
- ✅ Players remember "their" mask

### Stretch Goal:
- ✅ Easter eggs or hidden details
- ✅ Thematic variations (holidays, etc.)
- ✅ Animated versions (if time allows)
- ✅ Promotional art using masks

---

## Tips for Speed

### Work Smart:
1. **Create templates**: Make base shapes for each category
2. **Color palettes**: Pre-define colors for each category
3. **Reuse elements**: Horns, eyes, patterns can be modified/recolored
4. **Batch work**: Do all outlines, then all colors, then all details
5. **Don't overthink**: First idea is often best in a jam

### Time Management:
- **Hour 0-6**: Concept sketches, style tests
- **Hour 6-12**: First 10 samples, get approval
- **Hour 12-24**: Bulk production (40 more masks)
- **Hour 24-30**: Final 10 masks + polish
- **Hour 30-36**: Revisions based on playtesting

---

## Budget Constraints

**Time**: 48-hour game jam timeline
**File Size**: Keep under 200KB per mask (Unity performance)
**Complexity**: Favor clear readability over detail

**Remember**: "Done" beats "perfect" in a game jam!

---

## Questions?

Contact:
- **Technical specs**: Check `unity/STATUS.md`
- **File placement**: `/unity/Assets/Resources/Masks/`
- **Testing**: Developer will integrate and test in-game

---

**Good luck! Make these masks memorable! 🎭**

---

**Last Updated**: 2026-01-30
**Designer Assignment**: [Your Name]
**Estimated Hours**: 20-24 hours total
