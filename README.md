# RimworldCloneForEducation

# Prompt: Simple RimWorld-Style Colony Sim in Godot 4

> Copy everything below the line into Claude Code / your AI agent.
> Adjust the **Tech Choices** section first if you prefer GDScript over C#.

---

## Role

You are an experienced Godot 4 game developer. Build a **minimal colony simulation game** inspired by RimWorld's core loop. Work in small, testable increments — after every phase the project must run without errors.

## Tech Choices

- **Engine**: Godot 4.3+ (Mono/.NET build)
- **Language**: C# (.NET 8), idiomatic modern C# (pattern matching, records for data)
- **Rendering**: 2D top-down, `TileMapLayer` for terrain, simple colored `Sprite2D` placeholders (no art assets — use `ColorRect`/generated textures)
- **Architecture**:
  - Composition over inheritance; Godot nodes for presentation, plain C# classes for simulation logic
  - A central `GameManager` autoload (singleton) owning the tick loop and job dispatcher
  - Simulation runs on a fixed tick (e.g. 10 ticks/sec), decoupled from render framerate
- **No external dependencies** beyond Godot itself

## Scope — What to Build (and nothing more)

### In scope
1. **Map**: 64×64 tile grid. Tile types: grass, dirt, water (impassable), tree (resource), rock (resource). Randomly generated with simple noise.
2. **Colonists**: 3 pawns. Each has:
   - Needs: **Hunger** and **Rest** (0–100, decay over time)
   - A simple state machine: `Idle → SeekJob → MovingToTarget → Working → Idle`
   - A* pathfinding on the tile grid (use Godot's built-in `AStarGrid2D`)
3. **Job system** (the heart of the game):
   - Global priority-ordered job queue: `Eat` > `Sleep` > `Build` > `Chop/Mine` > `Haul`
   - Jobs are claimed by one colonist, released on failure/completion
   - Needs above a threshold auto-generate personal jobs (eat when hunger > 70)
4. **Resources & hauling**: Chopping trees drops **Wood**, mining rocks drops **Stone**. Items lie on the ground; haul jobs bring them to a stockpile zone.
5. **Building**: Player designates blueprints (wall, bed, table) via a build menu. Blueprints require wood delivered to them, then a colonist performs work to complete them.
6. **Player interaction**:
   - Left-click drag: designate trees to chop / rocks to mine / stockpile zone
   - Build menu (simple UI bar) to place blueprints
   - No direct pawn control — pawns act only through the job system (RimWorld-style)
7. **UI**: Per-colonist need bars, resource counters (wood/stone), current-job label, game speed controls (pause / 1x / 3x).
8. **Food loop**: Berry bushes on the map; harvesting gives food items; colonists eat from stockpile or ground.

### Explicitly out of scope (do not build these)
Combat, raids, animals, temperature, mood/mental breaks, medical, skills/traits, research, save/load, sound, art, multiplayer, mods, z-levels.

## Phased Plan — implement in this order, verify each phase runs

1. **Phase 1 — World**: Project setup, tile map generation with noise, camera pan/zoom (WASD + scroll).
2. **Phase 2 — Pawns & movement**: Spawn 3 colonists, `AStarGrid2D` pathfinding, click-to-move debug command (removed later).
3. **Phase 3 — Job system core**: Job queue, claiming, state machine. First job type: `ChopTree` via drag-designation. Tree disappears, wood item spawns.
4. **Phase 4 — Needs**: Hunger/rest decay, need bars UI, `Eat` (berry bushes) and `Sleep` (sleep on ground for now) jobs auto-generated from thresholds.
5. **Phase 5 — Stockpiles & hauling**: Stockpile zone designation, haul jobs, resource counter UI.
6. **Phase 6 — Building**: Blueprint placement, material delivery, construction work, walls block pathfinding, beds improve rest recovery.
7. **Phase 7 — Polish**: Game speed controls, mining, tables (eat at table = faster), small bug pass.

## Quality Rules

- Every phase ends with a **runnable game** — never leave the project broken between phases.
- Simulation state (needs, jobs, inventory) lives in **plain C# classes**, testable without the scene tree.
- Keep files under ~300 lines; split by responsibility (`JobSystem.cs`, `Pawn.cs`, `MapGenerator.cs`, …).
- Prefer Godot built-ins (`AStarGrid2D`, `TileMapLayer`, signals) over hand-rolled systems.
- Comment the *why* on non-obvious decisions (tick loop, job claiming), not the *what*.
- After each phase, summarize: what was built, how to test it manually, known limitations.

## Start

Begin with Phase 1. Before writing code, briefly confirm the project structure (folders, autoloads, main scene) you intend to use.
