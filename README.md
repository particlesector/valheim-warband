# WarbandSummoner

A [Valheim](https://www.valheimgame.com/) mod that turns the player into a summoner. Command a squad of up to five persistent minions — four melee and one archer — that fight alongside you and carry your loot, unlocked and upgraded by spending creature trophies.

> **Status:** pre-alpha. Design is complete; implementation has not started. Nothing here is playable yet.

## Features

- **Five minion slots.** Four melee, one archer. Your first greyling is yours from the moment the mod loads; earn the rest.
- **Trophy-driven progression.** Spend a creature's trophy to unlock it as a minion; spend another to rank it up. No boss gates, no grinding stats — just what you carry home.
- **A ladder for every biome.** Greyling to Jötun Warrior across thirteen melee tiers, with a separate archer ladder for the ranged slot.
- **Two ranks per tier**, expressed as vanilla 1- and 2-star creatures. Tiers can be skipped; rank resets on a tier change.
- **Material fallback.** Each tier can accept a common material (bone fragments, surtling cores, …) in place of its trophy, so a bad drop streak never stalls you.
- **Abilities on hotkeys:** summon, AoE heal, recall (tap for one, hold for all), attack my target, upgrade slot.
- **Pack mules.** Every minion carries a container sized to its tier.
- **Formation following** with a raised follow distance so four bodies don't trap you in a doorway.
- **Config-first.** The tier ladder, costs, keybinds, cooldowns, radii, offsets and slot count all live in the BepInEx config file.

Minions are clones of existing game creatures — no custom models, items, or asset bundles.

## Requirements

- Valheim **1.0.12** (the build this is compiled against; other 1.0.x builds may work)
- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) 5.4.2350 or later
- **Crossplay must be disabled.** Enabling crossplay disables BepInEx, and with it every mod.

Solo and local worlds only. Multiplayer and dedicated servers are not supported in v1.

## Installation

1. Install BepInExPack Valheim.
2. Drop `WarbandSummoner.dll` into `BepInEx/plugins/`.
3. Launch the game once to generate `BepInEx/config/particlesector.WarbandSummoner.cfg`, then edit to taste.

## Building

Requires the .NET SDK (8.0 or later) and a Valheim install with BepInEx.

1. Copy `Directory.Build.props.user.example` to `Directory.Build.props.user` and set `ValheimInstall` to your game folder (or set the `VALHEIM_INSTALL` environment variable).
2. `dotnet build` — the plugin is copied into `BepInEx/plugins/WarbandSummoner/` automatically.
3. `dotnet test` runs the game-independent unit tests.

Game assemblies are referenced from your install and publicized at build time via Krafs.Publicizer; no reflection is needed to reach private members.

## Documentation

- [docs/DESIGN.md](docs/DESIGN.md) — design specification
- [docs/PLAN.md](docs/PLAN.md) — implementation phases and status
- [docs/API-NOTES.md](docs/API-NOTES.md) — verified 1.0.12 game API surface
- [docs/PREFABS.md](docs/PREFABS.md) — creature and trophy prefab reference

## Acknowledgements

[Cheb's Necromancy](https://github.com/jpw1991/chebs-necromancy) (MIT) was studied for its minion persistence and container patterns. Any code adapted from it is attributed inline.

## License

[MIT](LICENSE)
