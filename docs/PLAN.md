# WarbandSummoner — Implementation Plan

Companion to [DESIGN.md](DESIGN.md). The design says *what*; this says *in
what order, with what structure, and when each piece is done*. Section 8 of
the design is the source of the ordering; this document adds the scaffolding
phase, the up-front decisions, and exit criteria.

Update this file as phases complete. Tick boxes are the status tracker.

---

## Milestones

| Milestone | Phases | Meaning |
|---|---|---|
| **M0 Foundation** | 0–1 | Builds, loads, pure logic tested. Nothing in-game yet. |
| **M1 First minion** | 2–4 | Summon a greyling, quit, reload, it's still there. |
| **MVP** | 5–7 | The class loop is playable: summon, heal, recall, upgrade, persist. |
| **v1** | 8–12 | Everything in DESIGN.md. Balanced enough to ship. |

MVP is deliberately narrower than v1 so there is a playable build early.
Pack mule, formation, drop-rate multiplier, and attack-my-target are all v1
features but none are needed to know whether the class is fun.

---

## Decisions to make before phase 1

These are not in DESIGN.md and block code. Proposed answers given; change
them here if you disagree.

| Decision | Proposal | Why |
|---|---|---|
| Plugin GUID | `particlesector.WarbandSummoner` | GitHub org + mod name; conventional and collision-free |
| Root namespace | `WarbandSummoner` | |
| Project layout | Three projects: `WarbandSummoner.Core` (netstandard2.0, zero Unity/Valheim refs), `WarbandSummoner` (netstandard2.1 BepInEx plugin), `WarbandSummoner.Tests` (xunit, refs Core only) | Phase 1 logic is testable without launching the game — the single biggest time-saver in this project given every in-game test is a restart |
| Tier table config format | Separate JSON file `BepInEx/config/WarbandSummoner.tiers.json`, default written on first run. Scalars stay in the normal `.cfg`. | BepInEx config is flat key/value; a list of structured records with nested loadouts and modifier maps does not fit it. JSON via Unity's `JsonUtility` needs no dependency. |
| Player slot storage | `Player.m_customData` (string dictionary persisted with the character) | No Harmony patch needed; survives character transfer between worlds. Verified present in 1.0.12. |
| Game path for builds | `VALHEIM_INSTALL` env var, or `Directory.Build.props.user` (gitignored) | Nobody's Steam path belongs in the repo |
| Decompiled reference source | `ilspycmd` output into `decompiled/` (gitignored) | Section 7 verification needs the real 1.0 source in front of us, greppable |

---

## Phase 0 — Scaffold and verify

Goal: a plugin that loads, logs its version, and a written answer to every
"verify against 1.0" note in DESIGN.md §7.

- [x] Confirm the local install is 1.0.12. Recorded in README.
- [x] Solution + three projects per the layout decision. `.gitignore` for
      `bin/`, `obj/`, `decompiled/`, `*.user`.
- [x] Reference game/BepInEx assemblies from the game path. Krafs.Publicizer
      on the two game assemblies. Plugin targets `netstandard2.1` to match
      the 1.0 game assemblies.
- [x] Post-build target copies the plugin DLLs into `BepInEx/plugins/WarbandSummoner/`.
- [x] Hello-world plugin loads on 1.0.12 and logs.
- [x] Decompiled both game assemblies with `ilspycmd`.
- [x] `docs/API-NOTES.md` written — every DESIGN §7 item answered.
- [x] `docs/PREFABS.md` — full creature/trophy catalogue from a runtime
      dump (`[Debug] DumpPrefabCatalogue`), melee and ranged ladders filled.
      Discovery: greylings drop no trophy; tier 0 became material-only (resin).
- [x] `ZNetScene.Awake` postfix confirmed to fire (catalogue dump uses it).

**Done.** Phase 0 complete.

---

## Phase 1 — Core progression logic (pure C#)

DESIGN §2. Lives entirely in `WarbandSummoner.Core`. No game types.

- [x] `TierDefinition` record (all fields from §2.3) and `TierTable` (ordered,
      validates ids unique, indices stable). A tier may be trophy-only,
      material-only, or both; neither is a validation error.
      `TierTable.Validate` returns every problem tagged with the tier id so
      Phase 2 can log them all at once; the constructor throws
      `TierTableException` carrying the same list.
- [x] `SlotState` — `(tierIndex, rank)` or unowned. `SlotGroup` = a
      `TierTable` plus N slots. Two groups: melee (4 slots, slot 0 pre-owned
      at (0, 1)) and ranged (1 slot, unowned). `SlotGroup.SetSlot` rejects
      tier indices outside the ladder and ranks above the configured max, so
      persisted state that no longer fits the config fails at load time.
- [x] `IInventoryView` — abstract "how many of item X do I have" so the
      resolver is testable with a dictionary.
- [x] `UpgradeResolver.Resolve(group, inventory, spendPriority)` →
      `UpgradeResult`: either `NoOp(reason)` or a `Purchase(slotIndex,
      before, after, tier, cost)`. Implements: highest affordable tier first;
      target-slot priority (unowned → lower tier → same tier below max rank →
      walk down); rank reset; trophy-vs-fallback spend priority; per-tier
      trophyCount/fallbackCount. The resolver never touches the inventory;
      the plugin consumes `Purchase.Cost` and then calls `SlotGroup.Apply`.
- [x] Tests: skip-tier, rank reset on tier change, fallback used only when
      trophy unaffordable (and inverted flag), material-only tier, walk-down
      past unaffordable tiers, no-op consumes nothing, slot 0 free, ranged
      slot not free, max rank cap, full-maxed group → no-op, groups never
      share a purchase, every example sentence in DESIGN §2.4 (including the
      7 / 8 / 2 purchase counts).
- [x] Rank → `SetLevel` value mapping (`rank + 1`) as a single function with a
      test, so the off-by-one lives in exactly one place (`Ranks.ToCharacterLevel`).

**Done.** Phase 1 complete; 48 tests green.

Finding: DESIGN §2.4 step 4 ("fall through to T−1 when T offers nothing")
is provably a no-op. Rule 2 means any slot below T benefits from T, so if T
offers nothing every slot already sits at or above T and no lower tier can
improve any of them. The resolver still walks down (cheap, and safe if the
priority rules ever change), and the only real fall-through is past tiers
the player cannot afford. Tested as such.

Generalisation for `maxRank > 2`: rule 3 ranks up the lowest-ranked slot on
tier T first, then lowest index — breadth over depth, consistent with the
rest of the priority order.

---

## Phase 2 — Configuration

DESIGN §6.

- [ ] All scalar settings bound in the `.cfg`: keybinds, hold duration, both
      recall cooldowns, heal radius/amount/cost, follow distance, four
      formation offsets, slot count, max rank, drop-rate multiplier, spend
      priority flag.
- [ ] `tiers.json` loader: write default melee and ranged ladders if missing,
      parse, validate (basePrefab non-empty, at least one of trophy/fallback,
      counts ≥ 1, no duplicate ids), log every problem with the tier id, fall
      back to defaults on fatal error rather than loading with zero tiers.
- [ ] Config reload on `.cfg` change is nice-to-have; tiers.json reload is
      **not** — tier index is persisted in ZDOs, so reordering at runtime is
      unsafe. Document that tiers.json is read at startup only.

**Done when:** deleting both config files and launching regenerates them
with sane defaults; a deliberately broken tiers.json logs a clear error.

---

## Phase 3 — Player slot persistence

DESIGN §2.1, §3.2 (player half).

- [ ] Serialise `SlotSet` to/from a compact string in `Player.m_customData`.
- [ ] Load on local player spawn; save on every purchase.
- [ ] Version prefix in the string so a future format change can migrate.
- [ ] Upgrade hotkey (§4.5) wired end-to-end: hotkey → resolver → consume
      items from `Player.m_inventory` → apply → message centre text stating
      slot, tier, rank, and what was spent. Plain press = melee group,
      modifier + press = ranged group.
- [ ] Dev console command `warband` (config-gated) that dumps slot state,
      living minion ZDO keys, and cooldowns to the log.

**Done when:** buy a slot with a trophy, log out and back in, slot is still
owned. Trophy is gone. Message was unambiguous.

---

## Phase 4 — Summon manager

DESIGN §3.1, §3.3, §7 critical note. Highest 1.0 risk; budget time.

- [ ] `ZNetScene.Awake` postfix: for each tier, clone `basePrefab` into a
      prefab named `WarbandSummoner_<tierId>`, strip timed-destruction and
      any spawn-effect components, register in `m_namedPrefabs`. Never touch
      the source prefab.
- [ ] Clone setup: add `Tameable` (`m_commandable = true`, unsummon knobs at
      zero), faction `Players`, `m_defaultItems` from the loadout with the
      random weapon/armour arrays emptied (the `Skeleton` prefab is used by
      both ladders and must be forced melee or bow). Strip `Procreation` if
      present. Compare against vanilla's `Wolf_spiritcaller` / `Skeleton_Friendly`.
- [ ] `MinionSetup` component on the clone: on `Awake`, read tier/rank from
      ZDO and apply tame, follow (via `s_follow` = player name), `SetLevel`,
      damage modifiers, `Physics.IgnoreCollision` with the local player.
      Idempotent — safe to run on every load.
- [ ] Summon hotkey: find lowest owned slot without a living minion,
      stamina check, instantiate at offset, write ZDO keys, deduct stamina.
- [ ] `MinionQuery.LivingFor(playerId)` — live ZDO scan by owner key. Used by
      cap, heal, recall, and the slot-occupancy check.
- [ ] Cap enforced as soft state (§3.3): block summon, never despawn.

**Done when:** four greylings and a skeleton archer follow the player, each
with one star, none collide with the player, the archer's arrows pass
through the player, and pressing summon with every slot filled is refused.

---

## Phase 5 — Persistence hardening

DESIGN §3.2. Slow to test; do it before adding more features on top.

- [ ] Quit to menu and reload: minions present, correct stars, still tamed
      and following, equipment present, damage modifiers reapplied.
- [ ] Walk far enough to unload the zone and return.
- [ ] Kill a minion; slot shows free; resummon costs stamina only.
- [ ] Player death and respawn: minions still owned, still bound.
- [ ] Change rank in config after minions exist: on reload, stars reflect
      new rank (proves derived state is recomputed, not stored).
- [ ] Write the manual test script for the above into `docs/TESTING.md` so it
      is repeatable every later phase.

**Done when:** the TESTING.md script passes twice in a row from a fresh
launch.

---

## Phase 6 — AoE heal

DESIGN §4.2.

- [ ] Hotkey → stamina check → `MinionQuery` filtered by radius → `Heal`.
- [ ] Cost is a fraction of the *lowest* tier's summon cost by default; call
      that out in the config description.

**Done when:** damaged minions in range heal, out of range don't, stamina
drops by the configured amount.

---

## Phase 7 — Recall  ← **MVP complete**

DESIGN §4.3.

- [ ] Key-down starts a timer; key-up resolves tap vs hold against the config
      threshold.
- [ ] Tap: highest-tier living melee minion, ties broken by rank; falls
      through to next-highest, then to the ranged minion. Hold: all living.
- [ ] Placement: ahead of player facing at configurable distance, raycast to
      ground, never inside player collider.
- [ ] Independent cooldowns; refused-for-cooldown message shows remaining
      seconds.

**Done when:** minion stuck behind a door is recovered by tap; all four are
recovered by hold; both refuse during cooldown.

---

## Phase 8 — Pack mule

DESIGN §3.4.

- [ ] Resolve the `Hoverable` conflict. Try child-GameObject-with-collider
      first; fall back to modifier-key routing if the raycast ordering can't
      be made reliable.
- [ ] `Container` sized from tier. Contents persist via the vanilla container
      ZDO path — confirm across reload.
- [ ] Hover text states both actions.

---

## Phase 9 — Formation and follow distance

DESIGN §5.1, §5.2.

- [ ] Raised follow stop distance, config-bound.
- [ ] Per-slot follow target: an invisible GameObject per slot positioned at
      the rotated offset each frame; minion follows that instead of the player.
      Ranged slot's offset sits furthest back.
- [ ] Confirm combat still overrides follow (they should break formation to
      engage).

---

## Phase 10 — Drop-rate multiplier

DESIGN §2.5.

- [ ] Single Harmony patch at the drop roll site identified in API-NOTES.md.
      Trophy items only (filter by `ItemType.Trophy`).
- [ ] README note that it affects all trophies, not just this mod's tiers.

---

## Phase 11 — Attack my target

DESIGN §4.4. Last because it has the most unknowns.

- [ ] Camera raycast → `Character` hit → set target on every living minion's
      `MonsterAI`.
- [ ] Re-assert for a configurable duration (or until target dead) via a
      coroutine, since `MonsterAI` re-evaluates on its own timer.
- [ ] Ignore hits on the player's own minions and other tamed creatures.

---

## Phase 12 — Balance and release readiness

DESIGN §8 step 10, §10.

- [ ] Play through Meadows → Swamp at minimum with default config. Tune
      `fallbackCount` per tier, stamina costs, heal cost, cooldowns.
- [ ] Defaults recorded in tiers.json with a `_notes` field per tier
      explaining the reasoning for its fallback.
- [ ] README: exact build string, config walkthrough, known issues.
- [ ] `CHANGELOG.md`.
- [ ] Tag `v1.0.0`. (Thunderstore packaging is deferred per DESIGN §9.)

---

## Testing workflow (applies to every in-game phase)

Each in-game test is a full game launch. Make each launch count:

- Dedicated test world and character, kept out of normal play.
- Enable `devcommands` in the F5 console; `spawn <prefab>` for trophies
  instead of farming them.
- Move `Jotunn` and `ValheimCompanionMod` out of `BepInEx/plugins/` while
  testing to rule out interference; restore afterwards.
- Use the `warband` dev command (Phase 3) to dump state rather than
  inferring it from behaviour.
- Read `BepInEx/LogOutput.log` after every launch, not just when something
  looks wrong.

## Risks

| Risk | Phase | Mitigation |
|---|---|---|
| 1.0 renamed/moved something in DESIGN §7 | 0, 4 | API-NOTES.md before any game code; phase 1 is game-independent so work continues regardless |
| Cloned prefab not registered before ZDO instantiation → minions vanish | 4, 5 | `ZNetScene.Awake` postfix, verified by the reload test in Phase 5 |
| `MonsterAI` overwrites assigned target | 11 | Re-assert loop; if still unreliable, ship v1 without it and note it |
| Follow-target-object approach fights vanilla tame AI | 9 | Fallback: patch follow-target position read instead of substituting the object |
