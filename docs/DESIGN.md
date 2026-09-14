# WarbandSummoner — Design Specification

A Valheim mod that turns the player into a summoner: a squad of up to four
persistent minions that fight and haul cargo, unlocked and upgraded by
spending creature trophies.

**Target game version:** Valheim 1.0.12
**Framework:** BepInEx 5.4.2350+ / HarmonyX
**Language:** C# (netstandard2.1 class library; the 1.0 game assemblies target netstandard 2.1)
**Unity editor required:** No

---

## 1. Design intent

The player installs this mod because they want to play a summoner. There is
no in-game unlock, no craftable item, no quest. Installing the mod *is* the
class choice.

The squad reduces the player's need to fight without removing the option.
The player remains a participant — directing focus fire, healing, repositioning
the tank — rather than a spectator behind an invulnerable wall.

### Non-goals

- No custom 3D assets, meshes, animations, or AssetBundles. All minions are
  clones of existing game prefabs.
- No custom items, recipes, or crafting stations.
- No Jötunn dependency.
- No patches against hostile AI target selection.
- No kill/damage attribution hooks. Progression is driven by inventory
  arithmetic only. (This was a deliberate choice — see 2.4.)
- No multiplayer/dedicated-server support in v1. Solo/local worlds only.
  Design should avoid decisions that make MP impossible later, but MP
  correctness is explicitly out of scope.
- No custom skill registration in v1.

---

## 2. Core model

Two independent axes:

- **Tier** — which creature the slot summons. Bought with trophies.
- **Rank** — how strong that creature is, expressed as vanilla star level.
  Bought with more trophies of the same tier.

### 2.1 Slots

The player has exactly **four minion slots**, indexed 0–3.

Each slot is either unowned, or owned at a specific (tier, rank) pair.

- Slot 0 is owned at tier 0 (Greyling), rank 1, from the moment the mod
  loads, at no cost. A new character can summon immediately.
- Slots 1–3 begin unowned.

Slot state persists on the player: four (tier, rank) pairs. That is the
entire progression state.

### 2.2 Ranks

Rank is 1 or 2. There is no rank 0 and no rank 3+.

| Rank | Stars shown | `SetLevel()` value |
|---|---|---|
| 1 | 1 star | 2 |
| 2 | 2 stars | 3 |

**Note on `SetLevel()`:** the method is 1-based. Level 1 is a plain,
star-less creature; level 2 displays one star; level 3 displays two stars.
Summoner minions are never plain — the baseline is one star, on the premise
that a summoned creature is better than a wild one.

Capping at 2 stars keeps the mod entirely inside the range vanilla produces
naturally, so star rendering, stat scaling, and animation are all already
balanced and exercised by the base game. Do not raise this cap without
verifying that 1.0 renders higher star counts correctly.

Star scaling supplies all intra-tier power progression. There is no separate
health multiplier field — vanilla's own curve is the curve.

### 2.3 Tier table

Tiers are an ordered list defined in config. Each entry:

| Field | Purpose |
|---|---|
| `id` | Stable string key, used in config and logs |
| `displayName` | Shown in HUD and messages |
| `basePrefab` | Creature prefab to clone |
| `trophyPrefab` | Item prefab name of the trophy that buys this tier |
| `trophyCount` | Trophies per purchase (default 1) |
| `fallbackMaterial` | Item prefab name of the material fallback (see 2.5). Empty means no fallback exists for this tier. |
| `fallbackCount` | Quantity of `fallbackMaterial` per purchase. **Per-tier, not global.** Initial value 2 for all tiers; expected to be tuned heavily. |
| `containerSlots` | Pack-mule inventory size at this tier |
| `damageModifierOverrides` | Optional per-damage-type resistance changes |
| `equipmentLoadout` | Items to equip on spawn (drives damage output) |
| `summonStaminaCost` | Stamina cost to summon at this tier |

Proposed initial ladder (starting points for tuning, not balance decisions):

0. Greyling — **greylings drop no trophy**; bought with the Greydwarf trophy
   (same biome and species family), resin fallback
1. Skeleton
2. Draugr
3. Draugr Elite
4. Fenring
5. Seeker
6. Charred Warrior (Ashlands)
7. Jötun Warrior (Deep North)

Prefab names, drop rates, and alternatives for every tier are in
[PREFABS.md](PREFABS.md).

The ladder is data. Adding, removing, or reordering tiers must require no
code change.

### 2.4 Progression

Progression is driven entirely by spending items from inventory. There are no
boss gates, and no tracking of kills, damage, or time played.

An earlier design considered use-based ranking (the more you field a creature
type, the stronger it gets). It was rejected because attributing kills or
damage to a specific minion requires patching the combat death/damage path —
the only place in this design that would touch hostile combat internals.
Trophy-based ranking delivers a similar progression feel with zero patch
surface.

**Full cost of maxing one tier:** 4 slots × 2 ranks = 8 trophies. Slot 0 is
free at tier 0 rank 1, so tier 0 costs 7.

#### The upgrade action

1. Player triggers the upgrade action (see 4.5).
2. Determine the highest tier `T` the player can currently afford, by trophy
   or by material fallback (see 2.5).
3. Determine the target slot by the priority order below.
4. If no slot would benefit, inform the player and do nothing. Consume
   nothing.
5. Consume the cost.
6. Apply the change to the target slot.

#### Target slot priority

Given an affordable tier `T`, in strict order:

1. **Any unowned slot** (lowest index first) → set to tier `T`, rank 1.
2. **Any owned slot below tier `T`** (lowest tier first, then lowest index)
   → set to tier `T`, rank 1.
3. **Any slot at exactly tier `T`, rank 1** (lowest index first) → raise to
   rank 2.
4. Otherwise, tier `T` offers nothing. Fall through to check tier `T-1`,
   and so on.

This is deliberately fully automatic with no player choice of destination.
Consequence: breadth is always preferred over depth. A player holding three
greylings and one 1-star skeleton will always see their next skeleton trophy
expand rather than rank up. This is usually the stronger play, but it does
remove a decision. Accepted in exchange for requiring no UI.

#### Rules

- **Tiers may be skipped.** A slot may go straight to any tier the player can
  afford; it need not pass through lower tiers. A player with three greyling
  slots who finds a skeleton trophy ends up with three greylings and one
  skeleton, in a slot that never held a greyling.
- **Rank resets to 1 on any tier change.** A tier-2 rank-2 slot upgraded to
  tier 3 becomes tier 3 rank 1. This keeps tier upgrades a real commitment
  and prevents cheap low-tier trophies from buying rank that rides up the
  ladder.
- Because of the reset, upgrading can in principle be a net power loss if two
  adjacent tiers are close together. The lowest-tier-first priority makes
  this rare. It is intentional, not a bug.
- Purchases are permanent. There is no downgrade path and no refund.
- Resummoning a dead minion costs only stamina — never a trophy or material.

### 2.5 Scarcity handling

Trophy drop rates are low, and the problem is worst at tier 0: a new
character may run several Meadows sessions before affording slot 1, during
the emptiest stretch of the game.

Two independent mitigations, both config-only:

**Drop-rate multiplier.** A single global config multiplier applied to
vanilla trophy drop rolls via one Harmony patch on the drop path. Default
should be 1.0 (vanilla) or modestly above; players tune to taste. Note in the
README that this affects trophy drops generally, not only tiers used by this
mod.

**Material fallback.** Each tier may specify `fallbackMaterial` and
`fallbackCount` as an alternative to its trophy. Bone fragments for
skeletons, surtling cores, black metal, and so on.

- Per-tier quantities, because material abundance varies enormously —
  bone fragments are near-free while surtling cores are genuinely scarce, so
  one global number would produce wildly different difficulty per tier.
- Initial value 2 everywhere, to be tuned in play.
- Calibration target: the fallback should be clearly *worse* than the trophy,
  so a player always prefers to find the trophy, but reachable enough that a
  bad RNG streak never stalls progression. If players route around trophies
  entirely, the count is too low.
- An empty `fallbackMaterial` means no fallback exists for that tier. Tier 0
  is a likely candidate — greylings drop resin and wood, neither scarce
  enough to be a meaningful cost at any quantity. The drop-rate multiplier is
  the better fix there.

**Spend priority:** when the player can afford a purchase both ways, spend
the trophy by default (it is the scarcer resource and players would rather
bank materials). Expose a config flag to invert this.

---

## 3. Minions

### 3.1 Spawning

For each owned slot with no living minion, the player may summon.

1. Clone the tier's `basePrefab`. **Never mutate the vanilla prefab** —
   modifying the shared prefab affects every naturally-spawned creature of
   that type in the world and will corrupt the player's game.
2. Instantiate at a position offset from the player (see 5.2).
3. Mark as tamed.
4. Set follow target to the player.
5. Remove any timed-destruction component (vanilla summons are temporary;
   these are not).
6. Apply `SetLevel()` from the slot's rank (2 or 3 per the table in 2.2).
7. Apply tier data: damage modifier overrides, equipment loadout.
8. Attach a `Container` sized to the tier.
9. Disable collision with the player (see 5.4).
10. Write ownership, slot index, tier, and rank to the minion's ZDO.
11. Deduct stamina.

### 3.2 Persistence

**Store on the minion's ZDO:**
- Owner player ID
- Slot index
- Tier index
- Rank
- A marker key identifying it as a mod-created minion

**Do not store derived stats.** Star level, damage modifiers, and equipment
must be recomputed from tier and rank on every `Awake`.

Rationale: equipment on persistent tamed Humanoids is known to be unreliable
across reloads in the vanilla serialization path, and damage modifiers do not
persist at all. Storing inputs and deriving outputs makes reload correctness
free and turns rebalancing into a config edit rather than a save migration.

**Store on the player:** the four (tier, rank) slot pairs.

### 3.3 Counting and cap

Enforce the cap by querying live minions whose ZDO owner matches the player,
not by maintaining a separate registry. This survives reload, crash, and zone
churn without reconciliation logic.

If the live count somehow exceeds the cap, treat it as a soft state: block
further summoning but do not despawn anything. Never destroy a player's
minions to enforce an invariant.

### 3.4 Pack mule

Each minion carries a `Container` sized by tier.

**Interaction conflict:** `Character` and `Container` both implement
`Hoverable`/`Interactable`, and the player's hover raycast resolves to the
first it finds. Resolve by either:

- (Preferred) placing the `Container` on a child GameObject with its own
  collider, or
- routing interaction by modifier key — plain interact commands the minion,
  modified interact opens the pack.

Pick one and be consistent. Hover text must make the available action clear.

---

## 4. Abilities

All abilities are hotkeys. All keybinds are config-bound.

### 4.1 Summon

Summons into the lowest-index owned slot that has no living minion. Costs
stamina per the tier. Instant cast — no channel.

**Stamina, not eitr.** Eitr is Mistlands-gated and does not exist for most of
a playthrough, so it cannot gate a class that must work from minute one.
Stamina is available immediately, scales with food like everything else in
Valheim, and is shared with dodging and blocking — so a panic resummon
mid-fight leaves the player unable to roll. That is the intended tension:
resummoning in combat is possible but costly, rather than forbidden.

If a later version wants Mistlands progression to matter to the class, eitr
should act as an optional *discount* on these costs, never as a requirement.

### 4.2 AoE heal

Heals all owned living minions within a radius. Costs stamina. Instant cast.

Heal must cost **substantially less** than summoning at the same tier. The
class should play as keeping the squad alive, because replacing a minion is
expensive; healing is the cheap repeatable action and resummoning is the
failure case.

Minions cannot be resurrected. Death costs the body and the stamina to
replace it; the slot, its tier, and its rank survive.

### 4.3 Recall

A single key with two behaviours, **both resolving on key release**:

- **Tap:** teleport the highest-tier *living* minion to a point ahead of the
  player's facing direction.
- **Hold ≥ 1.5s (config):** teleport all living minions.

Both are free. Each has its own cooldown (suggested: ~8s tap, ~30s hold —
tune in play).

Placement: a short distance ahead of the player along facing, not on top of
the player. Never teleport a minion into the player's collision volume.

**Fallback:** if the highest-tier slot has no living minion, the tap must
fall through to the next-highest living minion. Doing nothing here is a bug —
it is precisely the situation in which the player presses the key. Where two
slots share a tier, prefer the higher rank.

This ability doubles as the general get-unstuck action for minions lost to
terrain, doorways, portals, and boats.

### 4.4 Attack my target

Raycast from the camera. If it hits a `Character`, assign that character as
the target for all owned living minions.

**Implementation risk:** `MonsterAI` re-evaluates targets on its own timer
and may overwrite a directly assigned target. The assignment likely needs to
be re-asserted for a duration, and/or paired with setting the AI's alert or
pursuit state. **Verify the exact mechanism in the 1.0 assembly before
committing to an approach.**

### 4.5 Upgrade slot

Spends a trophy or material fallback per section 2.4/2.5. Must report clearly
what was bought, what was spent, and which slot changed — the auto-selection
is invisible otherwise.

---

## 5. Behaviour

### 5.1 Follow distance

Vanilla tamed follow uses a stop distance tuned for a single companion; four
minions converging on one point crowd the player badly.

Raise the base follow distance (suggested 4–5m, config-bound).

### 5.2 Formation

Each slot has a fixed local-space offset from the player. The minion follows
a point derived from the player's transform rotated by facing, not the
player's transform directly.

Four offsets in config, forming a loose arc behind and beside the player.
Recall placement (4.3) drops minions ahead of facing; the formation holds the
rest back.

Valheim's tamed AI has no separation steering, so minions will still bunch
when pathing around obstacles. Offsets reduce crowding; they do not eliminate
it. A true minimum-distance behaviour (actively backing away from the player)
would require driving movement directly and is out of scope.

### 5.3 Combat posture

Minions use vanilla tamed AI: they engage hostiles by proximity. There is no
passive mode in v1 and no aggro/threat manipulation of hostile AI.

### 5.4 Player collision

Minions must not collide with the player. Without this, four following
creatures will trap the player in doorways, on stairs, and on boats. Apply at
spawn via collision layer change.

---

## 6. Configuration

Everything tunable lives in BepInEx config. Nothing below should require a
rebuild to change:

- Full tier table (all fields in 2.3), including per-tier `fallbackCount`
- Global trophy drop-rate multiplier
- Trophy-vs-material spend priority flag
- All keybinds
- Recall hold duration, both recall cooldowns
- Heal radius, heal amount, heal stamina cost
- Base follow distance
- Four formation offsets
- Summon stamina costs (per tier)
- Slot count (default 4 — allow raising it)
- Max rank (default 2 — raising it is unsupported without verifying star
  rendering, but expose it)

---

## 7. Valheim 1.0 API notes

**Verified against 1.0.12 — see [API-NOTES.md](API-NOTES.md) for the
actual member names, signatures, and behaviours.** Valheim has no official
modding API; all of it is Harmony patching against decompiled internals.
Re-verify on every game update.

Types involved (details in API-NOTES.md):

- `Character` — `SetTamed(bool)`, `SetLevel(int)` (1-based), `Heal(...)`,
  `m_damageModifiers`
- `Humanoid` — equipment and default item sets
- `MonsterAI` — `SetFollowTarget(GameObject)`, follow distance, internal
  target field
- `Tameable` — follow/stay command interaction
- `Container` — inventory
- `ZNetScene` — prefab registration and lookup
- `ZDO` — per-object persistent key/value storage
- `Player` — local player, inventory access
- Trophy drop path — needed for the drop-rate multiplier patch; locate the
  actual roll site (likely on the character-drop component) in 1.0

**Critical:** custom cloned prefabs must be registered in `ZNetScene`'s lookup
*before* any saved ZDO referencing them is instantiated, or minions vanish on
world load. Register in a `ZNetScene.Awake` postfix, not later.

### Build setup

- Reference `assembly_valheim.dll` and `assembly_utils.dll` from
  `Valheim_Data/Managed`
- Run them through a publicizer (Krafs.Publicizer as an MSBuild task) so
  private members are reachable without reflection
- Confirm the referenced assemblies are from 1.0.x, not a pre-1.0 copy

### Known ecosystem state (September 2026)

- BepInEx 5.4.23.4 (BepInExPack_Valheim) loads on 1.0.12.
- Jötunn has no official 1.0 build; a community rebuild exists as a stopgap.
  This mod does not depend on Jötunn.
- Enabling crossplay disables BepInEx entirely — mods and crossplay are
  mutually exclusive. Document this for users.
- Most existing plugins broke at 1.0 and are awaiting rebuilds.

### Prior art

**Cheb's Necromancy** (MIT, github.com/jpw1991/chebs-necromancy) implements a
closely related feature set — commandable minions, container-carrying
gatherers, minion persistence. Latest release predates 1.0 and does not run
on it.

Worth reading for the persistence and container patterns. If any code is
adapted rather than merely referenced for concepts, attribute it in the
README per the MIT licence.

---

## 8. Implementation order

Ordered so the pieces with no API risk land first, and the API archaeology
happens on top of a working foundation.

1. **Tier table, slot logic, rank rules, spend rules.** Pure C#, no game API
   surface. Compiles regardless of what 1.0 changed. Includes config binding,
   skip-tier handling, rank reset, target-slot priority, and the material
   fallback path.
2. **Summon manager.** Prefab cloning, registration, tame, follow, star
   level, cap, ZDO persistence. This is where 1.0 drift will first bite —
   expect investigation time here.
3. **Persistence hardening.** Save/reload, zone unload/reload, death and
   resummon. Slow to test (each cycle is a game restart) and where the real
   bugs live.
4. **AoE heal.** Trivial once minions exist; validates the minion-lookup
   helper.
5. **Recall.** Tap/hold input, placement, cooldowns, fallback to
   next-highest.
6. **Container / pack mule.** Including the `Hoverable` conflict resolution.
7. **Formation offsets and follow distance.**
8. **Drop-rate multiplier patch.**
9. **Attack my target.** Late, because it carries the most 1.0 uncertainty
   and benefits from having a full squad to test against.
10. **Balance pass.** Requires play, not reasoning. Budget real time for it —
    particularly the per-tier `fallbackCount` values, which start at 2 purely
    as a placeholder.

## 9. Deferred

- Hold-progress indicator for recall
- HUD showing slot tiers, ranks, and minion health
- Player choice of upgrade destination (depth vs breadth)
- Passive/aggressive posture toggle
- Custom skill via SkillManager — a single overall Summoning skill affecting
  stamina efficiency or heal potency, *not* per-tier rank. Rejected for v1
  because vanilla skills decay on player death, which would punish the wrong
  event.
- Pity counter for trophy drops (needs kill attribution, which v1 avoids)
- Autonomous gathering (mining, woodcutting) — significant AI state machine
  work, explicitly out of scope for v1
- Multiplayer / dedicated server support
- Thunderstore release packaging

---

## 10. Repo conventions

- **Licence:** MIT
- **README must state** the exact Valheim build compiled against, and that
  crossplay must be disabled for any BepInEx mod to load
- **Config-first:** no magic numbers in code; every tunable is a config entry
- Attribute any adapted third-party code
