# WarbandSummoner

A [Valheim](https://www.valheimgame.com/) mod that turns the player into a summoner. Command a squad of up to five persistent minions — four melee and one archer — that fight alongside you and carry your loot, unlocked and upgraded by spending creature trophies.

> **Status:** pre-alpha. Design is complete; the game-independent progression logic (tiers, slots, ranks, upgrade resolution) and the configuration layer (`.cfg` settings, `tiers.json` ladders) are implemented and unit-tested. Nothing in-game yet; see [docs/PLAN.md](docs/PLAN.md) for progress.

## Features

- **Five minion slots.** Four melee, one archer. Your first greyling is yours from the moment the mod loads; earn the rest.
- **Trophy-driven progression.** Spend a creature's trophy to unlock it as a minion; spend another to rank it up. No boss gates, no grinding stats — just what you carry home.
- **A ladder for every biome.** Greyling to Jötun Warrior across thirteen melee tiers, with a separate archer ladder for the ranged slot.
- **Two ranks per tier**, expressed as vanilla 1- and 2-star creatures. Tiers can be skipped; rank resets on a tier change.
- **Material fallback.** Each tier can accept a common material (bone fragments, surtling cores, …) in place of its trophy, so a bad drop streak never stalls you.
- **Abilities on hotkeys:** summon, AoE heal, recall (tap for one, hold for all), attack my target, upgrade slot.
- **Pack mules.** Every minion carries a container sized to its tier.
- **Formation following** with a raised follow distance so four bodies don't trap you in a doorway.
- **Config-first.** The tier ladders, costs, keybinds, cooldowns, radii, offsets and slot counts all live in two config files; nothing is hard-coded.

Minions are clones of existing game creatures — no custom models, items, or asset bundles.

## Requirements

- Valheim **1.0.12** (the build this is compiled against; other 1.0.x builds may work)
- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) 5.4.2350 or later
- **Crossplay must be disabled.** Enabling crossplay disables BepInEx, and with it every mod.

Solo and local worlds only. Multiplayer and dedicated servers are not supported in v1.

## Installation

1. Install BepInExPack Valheim.
2. Drop the `WarbandSummoner` folder (`WarbandSummoner.dll` and `WarbandSummoner.Core.dll`) into `BepInEx/plugins/`.
3. Launch the game once to generate the two config files, then edit to taste.

## Configuration

Two files in `BepInEx/config/`, both written with defaults on first launch:

- **`particlesector.WarbandSummoner.cfg`** — every scalar setting: keybinds, recall hold time and cooldowns, heal radius / amount / cost, follow distance, one formation offset per slot, slot counts, max rank, trophy drop-rate multiplier, spend priority. Each entry is documented in the file. Most are read live, so edits through ConfigurationManager apply immediately; the slot counts are read at startup.
- **`WarbandSummoner.tiers.json`** — the melee and ranged tier ladders: which creature each tier clones, what it costs (trophy and/or fallback material, with per-tier counts), pack size, summon stamina, equipment loadout and damage-modifier overrides. Read **at startup only** — a tier's position in its list is what gets saved on your character and your minions, so reordering while a world is loaded is unsafe. Delete the file to regenerate the defaults. A file that will not parse, or a ladder that fails validation, is logged tier-by-tier in `BepInEx/LogOutput.log` and replaced by the built-in default for that ladder; the mod never runs with zero tiers. Prefab names are checked against the game once it has loaded; a misspelling is logged with its tier id. See [docs/PREFABS.md](docs/PREFABS.md) for valid names.

Default keys — all rebindable, none used by vanilla:

| Key | Action |
|---|---|
| `Z` | Summon into the lowest empty owned slot |
| `H` | Heal minions in range |
| `B` | Recall — tap for the strongest living minion, hold for all |
| `N` | Attack the creature under the crosshair |
| `U` | Upgrade a melee slot (spends a trophy or its fallback) |
| `Shift+U` | Upgrade the ranged slot |

## Building

Requires the .NET SDK (8.0 or later) and a Valheim install with BepInEx.

1. Copy `Directory.Build.props.user.example` to `Directory.Build.props.user` and set `ValheimInstall` to your game folder (or set the `VALHEIM_INSTALL` environment variable).
2. `dotnet build` — the plugin is copied into `BepInEx/plugins/WarbandSummoner/` automatically.
3. `dotnet test` runs the game-independent unit tests.

Game assemblies are referenced from your install and publicized at build time via Krafs.Publicizer; no reflection is needed to reach private members. `tiers.json` is read with Newtonsoft.Json 13, which the game ships in `valheim_Data/Managed` — the build references the matching NuGet package for the tests but deploys nothing extra.

## Documentation

- [docs/DESIGN.md](docs/DESIGN.md) — design specification
- [docs/PLAN.md](docs/PLAN.md) — implementation phases and status
- [docs/API-NOTES.md](docs/API-NOTES.md) — verified 1.0.12 game API surface
- [docs/PREFABS.md](docs/PREFABS.md) — creature and trophy prefab reference

## Acknowledgements

[Cheb's Necromancy](https://github.com/jpw1991/chebs-necromancy) (MIT) was studied for its minion persistence and container patterns. Any code adapted from it is attributed inline.

## License

[MIT](LICENSE)
