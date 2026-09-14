# Valheim 1.0.12 API Notes

Verified against `assembly_valheim.dll` from Valheim **1.0.12** (Unity
6000.0.75, BepInEx 5.4.23.4) by decompiling with ilspycmd. Every item in
DESIGN.md §7 is answered here. Re-verify on each game update.

Line numbers refer to the decompiled output in `decompiled/assembly_valheim/`
(gitignored — regenerate with `ilspycmd -p -o decompiled/assembly_valheim
"<Valheim>/valheim_Data/Managed/assembly_valheim.dll"`).

---

## Build / toolchain

- Game assemblies target **netstandard 2.1**. The plugin targets
  `netstandard2.1` to match; a `net462` target also compiles but produces
  facade-version warnings. `MSB3277` is suppressed in the plugin csproj —
  the game references newer `System.Net.Http` / `System.IO.Compression`
  facades than the ref pack, and we use neither.
- Krafs.Publicizer 2.3.0 works on both game assemblies.
- BepInEx names the config file by GUID:
  `BepInEx/config/particlesector.WarbandSummoner.cfg`.
- The game ships **Newtonsoft.Json 13.0.2** (assembly `13.0.0.0`,
  public key `30ad4fe6b2a6aeed`) in `valheim_Data/Managed` and
  `assembly_valheim` references it, so it is always loaded. Core references
  the NuGet 13.0.3 package (same identity) and the plugin deploys no copy.
- Keyboard keys **not** bound by vanilla (`ZInput` defaults, keyboard
  layout): `B H I J K L N O P U Y Z`. Everything else on the main block is
  taken (`G` radial, `T` emote, `V` auto-pickup, `X` sit, `C` walk, `F`
  forsaken power, `R` hide weapon, `Q` auto-run, `E` use, `M` map,
  `1–8` hotbar, `Tab` inventory, `F5` console). Default mod keys are drawn
  from the free set.
- `UnityEngine.InputLegacyModule.dll` ships, so BepInEx's
  `KeyboardShortcut` (legacy `Input.GetKey`) is expected to work; confirmed
  at the first hotkey in Phase 3.
- `Terminal.ConsoleCommand` constructor gained parameters in 1.0
  (`remoteCommand`, `onlyAdmin`). Jötunn 2.28 fails on this. Signature:
  ```
  ConsoleCommand(string command, string description, ConsoleEvent action,
      bool isCheat = false, bool isNetwork = false, bool onlyServer = false,
      bool isSecret = false, bool allowInDevBuild = false,
      bool hideBehindDevCommands = false, ConsoleOptionsFetcher optionsFetcher = null,
      bool alwaysRefreshTabOptions = false, bool remoteCommand = false,
      bool onlyAdmin = false)
  ```
  Use named args for anything past `action`.

---

## Character

| Member | Notes |
|---|---|
| `void SetLevel(int level)` | 1-based. Writes `ZDOVars.s_level`, calls `SetupMaxHealth()`, fires `m_onLevelSet`. Ignores `level < 1`. |
| `int GetLevel()` | |
| `void SetTamed(bool)` | Goes through RPC `RPC_SetTamed`; only applies if `m_nview.IsOwner()`. Writes `ZDOVars.s_tamed`. |
| `bool IsTamed()` | Non-owners re-read the ZDO once per second. |
| `void Heal(float hp, bool showText = true)` | |
| `float GetHealth()` / `GetMaxHealth()` / `SetMaxHealth(float)` | |
| `bool IsDead()` | virtual |
| `ZDOID GetZDOID()` | |
| `BaseAI GetBaseAI()` | |
| `Vector3 GetCenterPoint()` | |
| `HitData.DamageModifiers m_damageModifiers` | Public struct field. Not persisted — recompute every Awake as DESIGN says. |
| `Faction m_faction` | Public. |
| `Action m_onDeath`, `Action<int> m_onLevelSet` | Public delegates. |
| `static List<Character> GetAllCharacters()` | Loaded (instantiated) characters only. |
| `static void GetCharactersInRange(Vector3, float, List<Character>)` | Use for heal radius. |
| `virtual void Message(MessageHud.MessageType, string, int amount = 0, Sprite icon = null, bool log = false)` | Player override shows on HUD only if `m_nview.IsOwner()`. |

Layers set up in `Character.Awake` statics: `character`, `character_net`,
`character_ghost`, mask `character`+`character_noenv`.

### Tamed vs enemy rules (`BaseAI.IsEnemy(Character a, Character b)`)

- Two tamed characters are **never** enemies.
- Tamed vs untamed: enemies **unless** the other is `Faction.Players`, or
  `Faction.Dverger` and not aggravated.
- So a tamed Greyling attacks wild Greylings. Good.

---

## Tameable — **not present on wild creature prefabs**

Wild `Greyling`, `Skeleton`, `Draugr` etc. have no `Tameable`. It must be
`AddComponent`ed to each clone. It provides:

- Follow/stay toggle on interact (`Command(Humanoid user, bool message)`),
  gated by `m_commandable`.
- **Follow persistence**: `RPC_Command` writes `ZDOVars.s_follow` =
  `player.GetPlayerName()` (a **name string**, not an ID).
  `UpdateSavedFollowTarget()` re-issues `Command` on reload for any loaded
  player with that name. Vanilla does our follow-restore for free if we set
  the key. Caveat: name-based, so two characters with the same name collide
  — acceptable for solo v1.
- Summon lifetime knobs used by vanilla's Dead Raiser skeletons:
  `m_unsummonDistance` (0 = never), `m_unsummonOnOwnerLogoutSeconds`
  (0 = never), and ZDO `s_maxInstances` (set by `SpawnAbility`, 0 = no cap).
  **These are the "timed destruction" for vanilla summons** — leave all at
  zero on our clones. `TimedDestruction` is a separate component; strip it
  if a base prefab happens to carry one.
- `Interact(Humanoid user, bool hold, bool alt)`: `alt` (Shift+E on
  keyboard, `AltPlace` button) → vanilla rename. `hold` → returns false.
  Plain → pet/command.
- `GetHoverText()` / `GetHoverName()`.
- `Awake` registers RPCs `Command`, `SetName`, `RPC_UnSummon` and starts
  `TamingUpdate` every 3s. Wires `m_character.m_onDeath += OnDeath`.

---

## MonsterAI / BaseAI

| Member | Notes |
|---|---|
| `void SetFollowTarget(GameObject go)` | Sets private `m_follow`. Public. |
| `GameObject GetFollowTarget()` | |
| `void MakeTame()` | `SetTamed(true)`, clears alert and targets. |
| `void SetPatrolPoint()` / `ResetPatrolPoint()` | |
| `private Character m_targetCreature` | Publicized. |
| `private float m_updateTargetTimer` | Re-evaluates via `FindEnemy()` every **2s** if a player is within 50m, else 6s, and only when `!InAttack()`. **Overwrites `m_targetCreature` if `FindEnemy()` returns non-null.** |
| `float m_alertRange` | Tamed minions drop their target if it is farther than this from the follow object (or patrol point). |

`BaseAI.Follow(GameObject go, float dt)` (protected) — **hardcoded**: stop
at `< 3f`, run when `> 10f`. There is no follow-distance field. Options for
DESIGN §5.1/5.2: substitute a per-slot anchor GameObject as the follow
target (formation offsets then come free and the 3m applies to the anchor),
or Harmony-prefix `Follow` to use a configurable stop distance. Anchor
approach preferred; it satisfies both requirements at once.

Attack-my-target (DESIGN §4.4): set `m_targetCreature` directly and
re-assert every < 2s while the target is alive and `IsEnemy`. `UpdateTarget`
also nulls the target if `IsDead()` or `!IsEnemy(target)`, so only hostile
targets can be assigned.

---

## Humanoid

- `GameObject[] m_defaultItems`, `m_randomWeapon`, `m_randomArmor`,
  `m_randomShield`, `ItemSet[] m_randomSets`, `m_randomItems`.
- `void GiveDefaultItems()` — called from `Humanoid.Awake` path; picks
  random entries. For a deterministic loadout, set `m_defaultItems` on the
  **clone prefab** and empty the random arrays, then equipment is applied by
  vanilla on every spawn/reload (it is not persisted — it is re-given).
- `Inventory GetInventory()`.

---

## Inventory — item matching is by shared name, not prefab name

`CountItems(string name, ...)`, `RemoveItem(string name, int amount, ...)`,
`HaveItem(string name, ...)` all compare `item.m_shared.m_name` (the
`$item_...` token). Resolve config prefab names through
`ObjectDB.instance.GetItemPrefab(prefabName).GetComponent<ItemDrop>()
.m_itemData.m_shared.m_name` once at startup.

Consequence worth knowing: `TrophyDraugr` and `TrophyDraugrFem` both have
shared name `$item_trophy_draugr`; `TrophyForestTroll` and `TrophyFrostTroll`
share `$item_trophy_troll`. Counting by shared name pools them — desirable.

---

## Player

| Member | Notes |
|---|---|
| `static Player m_localPlayer` | |
| `long GetPlayerID()`, `string GetPlayerName()` | |
| `Dictionary<string,string> m_customData` | **Exists.** Serialised with the character (`Save`/`Load` pkg). Use for slot state. |
| `override void UseStamina(float)`, `override bool HaveStamina(float = 0)`, `float GetStamina()` | |
| `override void Message(...)` | HUD message. |

Hover resolution (`FindHoverObject`, Player.cs:4270): raycast from camera
along `m_interactMask`, hits sorted by distance, **first** hit within
`m_maxInteractDistance` wins and the loop breaks. For that hit: if the
collider's own GameObject has a `Hoverable` → that object; else if it has an
`attachedRigidbody` → the rigidbody's GameObject (the Character root); else
the collider's GameObject.

Interact (`Player.Update`): `alt` = `ZInput.GetButton("AltPlace")` (Shift on
keyboard) and is passed to `Interactable.Interact(user, hold, alt)`.

**DESIGN §3.4 resolution:** prefer the modifier-key route. Harmony-prefix
`Tameable.Interact` on our minions: `alt == true` → open the container and
return true (suppressing vanilla rename); postfix `GetHoverText` to say so.
No collider work, no raycast-order gamble. The child-collider approach is
also viable (Container has `m_rootObjectOverride` exactly for a container
on a child of the ZNetView holder) but depends on the pack collider being
the first hit.

---

## Container

- `string m_name`, `int m_width`, `int m_height`, `ZNetView
  m_rootObjectOverride`, `bool m_checkGuardStone`, `PrivacySetting m_privacy`.
- Awake: `m_nview = m_rootObjectOverride ? ... : GetComponent<ZNetView>()`;
  requires `m_nview.GetZDO() != null` at Awake, so **add the component after
  the ZNetView has a ZDO** or add it to the clone prefab so it initialises
  with the object. Inventory persists in the same ZDO under `s_items`.
- One Container per ZDO — fine, one pack per minion.
- Registers RPCs `RPC_RequestOpen`, `RPC_OpenResponse`, etc.

---

## ZNetScene / ZDO / ZNetView

- `ZNetScene.m_prefabs` (public List) is copied into private
  `m_namedPrefabs` (Dictionary<int, GameObject>, keyed by
  `name.GetStableHashCode()`) in **`Awake`**. Register clones in an `Awake`
  **postfix** by adding to `m_namedPrefabs` (publicized) — also add to
  `m_prefabs` for tooling that enumerates it. Confirmed: our
  `ZNetScene.Awake` postfix fires (used by the prefab catalogue dump).
- `GetPrefab(string)` / `GetPrefab(int hash)` / `HasPrefab(int)`.
- `ZDO.Set(int hash, int|long|float|bool|string|Vector3|Quaternion|byte[])`
  and matching `GetX(int hash, default)`. Use `"key".GetStableHashCode()`
  once into static readonly ints, as `ZDOVars` does.
- `ZDO.GetPrefab()` (hash), `GetPosition()`, `SetPosition(Vector3)`,
  `GetOwner()`, `IsOwner()`, `SetOwner(long)`, `Persistent` property.
- `ZDOMan.GetAllZDOsWithPrefabIterative(string prefab, List<ZDO>, ref int
  index)` — the only "all ZDOs of prefab" query; spread across frames, returns
  true when complete. Finds minions in **unloaded** zones. Recall of a
  far-away minion = `zdo.SetPosition(...)` and it instantiates when the zone
  loads.
- `ZNetView.GetZDO()`, `IsOwner()`, `IsValid()`, `ClaimOwnership()`,
  `Destroy()`.

Relevant `ZDOVars`: `s_tamed`, `s_follow` (string player name), `s_level`,
`s_tamedName`, `s_health`, `s_maxHealth`, `s_items`, `s_maxInstances`,
`s_despawnInDay`, `s_eventCreature`, `s_patrol`, `s_patrolPoint`,
`s_spawnTime`.

---

## Trophy drop path (DESIGN §2.5 multiplier)

`CharacterDrop.GenerateDropList()` (CharacterDrop.cs:70). Per `Drop`:
`m_prefab`, `m_chance`, `m_amountMin/Max`, `m_levelMultiplier` (chance and
amount scale by `2^(level-1)`), `m_onePerPlayer`, `m_dontScale`.

1.0 adds a **pseudo-random** path for `chance <= 0.3` (unless global key
`NoPseudoDrops`): a per-prefab-name counter in `s_pseudoCounter` guarantees a
drop within `~2/chance` kills. Keyed on `(prefab name, chance)`; a changed
chance resets the counter.

Patch: **prefix `GenerateDropList`**, and for each drop whose
`m_prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_itemType ==
ItemType.Trophy`, multiply `m_chance` by the config value. `m_drops` is a
per-instance list on a dying creature, so no restore is needed; guard with a
per-instance flag anyway in case it runs twice.

---

## Player collision (DESIGN §5.4)

Vanilla uses `Physics.IgnoreCollision(m_collider, otherCollider, true)` for
attach points (Player.cs:6404). For minions: on the local player's side,
`Physics.IgnoreCollision(Player.m_localPlayer.m_collider, minion.m_collider,
true)` when a minion spawns or loads, and the reverse is automatic. Layer
changes (`character_ghost`) would also stop hostiles hitting them, so do
**not** change layer.

---

## Prefab names for the tier ladder

See `docs/PREFABS.md` (generated from the in-game catalogue dump; enable
`[Debug] DumpPrefabCatalogue = true` and load a world to regenerate).
