# WarbandSummoner

A [Valheim](https://www.valheimgame.com/) mod that turns the player into a summoner. Command a squad of up to four persistent minions that fight alongside you and carry your loot, unlocked and upgraded by spending creature trophies.

> **Status:** pre-alpha. Design is complete; implementation has not started. Nothing here is playable yet.

## Features

- **Four minion slots.** Slot 0 is yours from the moment the mod loads. Earn the rest.
- **Trophy-driven progression.** Spend a creature's trophy to unlock it as a minion; spend another to rank it up. No boss gates, no grinding stats — just what you carry home.
- **Two ranks per tier**, expressed as vanilla 1- and 2-star creatures. Tiers can be skipped; rank resets on a tier change.
- **Material fallback.** Each tier can accept a common material (bone fragments, surtling cores, …) in place of its trophy, so a bad drop streak never stalls you.
- **Abilities on hotkeys:** summon, AoE heal, recall (tap for one, hold for all), attack my target, upgrade slot.
- **Pack mules.** Every minion carries a container sized to its tier.
- **Formation following** with a raised follow distance so four bodies don't trap you in a doorway.
- **Config-first.** The tier ladder, costs, keybinds, cooldowns, radii, offsets and slot count all live in the BepInEx config file.

Minions are clones of existing game creatures — no custom models, items, or asset bundles.

## Requirements

- Valheim **1.0.x** (exact build will be listed here once compiled against)
- [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) 5.4.2350 or later
- **Crossplay must be disabled.** Enabling crossplay disables BepInEx, and with it every mod.

Solo and local worlds only. Multiplayer and dedicated servers are not supported in v1.

## Installation

1. Install BepInExPack Valheim.
2. Drop `WarbandSummoner.dll` into `BepInEx/plugins/`.
3. Launch the game once to generate `BepInEx/config/WarbandSummoner.cfg`, then edit to taste.

## Building

Requires the .NET Framework 4.6.2 targeting pack and a Valheim install.

1. Point the build at your game's `Valheim_Data/Managed` directory (`assembly_valheim.dll`, `assembly_utils.dll`).
2. `dotnet build`

The referenced assemblies are publicized at build time via Krafs.Publicizer; no reflection is needed to reach private members.

## Documentation

- [docs/DESIGN.md](docs/DESIGN.md) — full design specification, API notes, and implementation order.

## Acknowledgements

[Cheb's Necromancy](https://github.com/jpw1991/chebs-necromancy) (MIT) was studied for its minion persistence and container patterns. Any code adapted from it is attributed inline.

## License

[MIT](LICENSE)
