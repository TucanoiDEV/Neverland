# Neverland (*A Terra do Nunca*)

> *"All children grow up. Except one."*
> The place where children never grow up is, here, the place children never leave.

**First-person survival horror · Unity 3D · PS1/VHS aesthetic**

Internal codename: **NEVERLAND** · Status: **pre-production**

---

## About

A boy named Wendy escapes the violence of his home by closing his eyes — and wakes up in Neverland. A children's paradise that, come nightfall, reveals itself as a trap no child has ever escaped.

No weapons. No combat. Only items, hiding places, puzzles, and the courage of a child who just wants to go home.

| | |
|---|---|
| **Genre** | Survival horror · Stealth · Escape room |
| **Perspective** | First person, child height (~1.20 m) |
| **Target length** | ~40 min per run |
| **Structure** | Prologue + one day of false peace + a single night of escape + climax and epilogue |
| **Platforms** | PC (Steam / itch.io); consoles under evaluation |
| **Rating** | 18+ (explicit gore, sensitive themes) |
| **Model** | Low-cost premium (US$ 4.99–7.99) |
| **Engine** | Unity 2022.3 LTS · 3D URP · C# |

---

## Design pillars

Four pillars drive every decision. If a feature doesn't support at least one of them, it doesn't ship.

1. **Absolute vulnerability** — Wendy is a child. All player power comes from knowledge and items, never force. Fear is born of physical helplessness.
2. **The paradise is the prison** — The island is beautiful by day and monstrous by night. Contrast is the game's primary horror weapon.
3. **Tension through noise and routine** — Every creaking floorboard calls the hunter. The player learns the house, and the house learns the player.
4. **Corrupted nostalgia (PS1/VHS)** — The aesthetic isn't a filter: it's a childhood memory degraded like a worn-out tape.

---

## Core systems

### Close Your Eyes (signature mechanic)
In the prologue, closing his eyes is how Wendy escapes. In the game, it's how he faces things. Holding the key darkens the screen and sharpens hearing — Tinker Bell's toll becomes precise sonar through walls. The cost is real blindness.

### Tinker Bell — the hunter
A sweet guide by day, a 2.4 m huntress by night. Full FSM (Patrol → Investigate → Chase → Return → Capture) with a noise heat map, an honest vision cone (110°, 12 m), and strict fairness rules: never teleports, never spawns in the player's room, every movement audible through her bell.

### Lucidity
In dialogue, the player chooses between lines of **surrender** (accepting the fantasy) and lines of **Lucidity** (resisting it). A hidden counter — no number, no bar — changes how Peter Pan and Tinker Bell treat the boy: sweetness turns to surveillance, the guide turns to shadow. It **never** alters difficulty: it changes the framing, not the rules.

### Noise and hiding
Every action emits a noise radius in meters (crawl 3 m · walk 9 m · run 16 m · broken glass 22 m), modulated by surface. Hiding three times in the same spot teaches Tinker Bell to check that room.

### Puzzles
A linear chain of five prison-house puzzles, each opening roughly 25% more of the house, until the exit's **triple lock** is assembled. Items have 3 possible spawn points per run.

---

## Structure

| Block | Player objective | Duration |
|---|---|---|
| Prologue — the real bedroom | None (playable ceremony) | 4–5 min |
| The Day | Complete the list of toys (diegetic tutorial) | 8–10 min |
| The Night · Revelation | Survive the first encounter | 2–3 min |
| The Night · Escape through the house | Assemble the triple lock and open the exit | 16–20 min |
| The Night · Climax | Reach the sea | 5–6 min |
| Epilogue | — | 2–3 min |

### Two endings
Decided by a single factor: whether Wendy is carrying the **machete** from the butchery when he reaches the cliff. No warning, no meter.

- **The Waters** (canonical) — with the machete, he cuts off his own hand to break free.
- **Devoured** — without it, there is no instrument, no prompt, no salvation.

The bad ending isn't a punishment for skill, but for curiosity not exercised: those who avoid the butchery avoid the truth, and the island collects.

---

## Art and sound direction

**Visual** — Honest low poly (500–1,500 tris), 64–256 px textures with point filtering, vertex snapping, affine mapping, 320×240 internal resolution upscaled, 4×4 dithering, and VHS post-processing. NPCs animate at 12 fps; monster Tinker Bell at 24 fps — she is the only fluid thing in the world, which is what makes her wrong.

**Sound** — Half the horror is audio. Four adaptive music states driven by stems (Calm · Vigil · Hunt · Finale) with a single leitmotif: the music-box theme, introduced as comfort, corrupted into threat, redeemed in the finale. At least 20% of the night runs with no music at all — silence is budgeted like an asset.

**HUD** — As little screen as possible. No health bar, no stamina bar, no radar. Threat is communicated 100% through sound and light; the quest log is Wendy's pencil notebook.

---

## Technical specs

| Layer | Choice |
|---|---|
| PS1 render | Shader Graph (vertex snap + affine + per-vertex fog), 480×270 RenderTexture, CRT/dither post |
| AI | NavMesh + custom FSM; waypoints as ScriptableObjects |
| Dialogue | Node graph in ScriptableObject; `lucidity` (int) persisted in the save |
| Audio | Unity Audio Mixer with per-state snapshots; FMOD under evaluation |
| Save | Lightweight encrypted JSON in `persistentDataPath`; automatic checkpoints |
| Input | Input System, with rebinding and controller support from day one |
| Version control | Git + GitHub · Git LFS for binaries · feature branches |

**Targets:** 60 fps on a modern integrated GPU · < 300 draw calls · < 2 GB RAM · build < 1.5 GB · loading < 5 s.

---

## Roadmap

| Phase | Deliverable | Estimate |
|---|---|---|
| 0 · Pre-production | Approved GDD, paper prototype of the puzzles, house blockout | 2–3 weeks |
| 1 · Prototype | Ground-floor greybox + Tinker Bell FSM + noise/hiding + positional bell. **No art.** Goal: "is it scary yet?" | 4–6 weeks |
| 2 · Vertical slice | Revelation + puzzles A and B with final art, save, and menu. Basis for the first trailer | 6–8 weeks |
| 3 · Production | Full night, day, island, climax, soundtrack, collectibles | 4–6 months |
| 4 · Alpha → Beta | Closed playtests, accessibility pass, PT-BR/EN localization | 6–8 weeks |
| 5 · Launch | Steam page 3+ months ahead; demo at Next Fest; keys for horror streamers | — |

**Never cut:** the prologue, Tinker Bell's transformation, the close-your-eyes mechanic, the cliff scene, and the epilogue.

---

## Accessibility

Subtitles with directional sound indicators · colorblind mode for color-coded lights and items · adjustable CRT/dithering intensity (0–100%) · FOV 60–90° · head-bob reduction · hold→toggle on all inputs · "Story" mode (Tinker Bell 20% slower, cooldown period doubled).

---

## Content note

The game handles domestic violence implicitly — sounds behind a door, never on screen. The goal is empathy, not shock. A card with support resources (Brazil: Disque 100 / 180) is shown in the credits.

---

## Declared references

Puppet Combo (aesthetic and pacing) · Granny (puzzle logic) · Outlast (total helplessness) · Silent Hill 2 (horror as metaphor) · Resident Evil 2/3 Remake (persistent stalker) · *Peter Pan – La Obscura Verdad* (tone and climax framing) · J. M. Barrie, *Peter and Wendy* (1911, public domain).

No third-party assets are used — the references are matters of language.

---

## Documentation

The full breakdown of every system lives in the **GDD** (`GDD_A_Terra_do_Nunca.docx`, in Brazilian Portuguese), which is the canonical source: story and script, character sheets, house level design, noise and AI tables, items and puzzles, interface, art and sound direction, technical specs, and the production plan.

This is a living document: every system must be validated in prototype before it becomes truth.

---

**Tucano (TucanoiDEV) · TEREJACKS · 2026**
