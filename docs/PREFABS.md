# Prefab Reference (Valheim 1.0.12)

Generated from the in-game catalogue dump (`[Debug] DumpPrefabCatalogue =
true`, then load a world). Regenerate on every game update. The curated
sections at the top are what the tier table needs; the raw dump is below
for anyone editing `tiers.json`.

Drop notation: `Prefab@chance` at level 1. Chance and quantity scale by
`2^(level-1)` for drops with `m_levelMultiplier` (trophies included), and
chances <= 0.3 use the pseudo-random guarantee (see API-NOTES.md).

---

## Proposed tier ladder

| # | id | basePrefab | Faction | trophyPrefab | Trophy rate | fallbackMaterial (candidate) | Notes |
|---|---|---|---|---|---|---|---|
| 0 | greyling | `Greyling` | ForestMonsters | `TrophyGreydwarf` | 0.05 (from Greydwarf) | `Resin` | **Greylings drop no trophy.** Greydwarf trophy is the same biome/species family. |
| 1 | skeleton | `Skeleton` | Undead | `TrophySkeleton` | 0.10 | `BoneFragments` | Also dropped by every `Skeleton_*` variant. |
| 2 | draugr | `Draugr` | Undead | `TrophyDraugr` | 0.10 | `Entrails` | `Draugr_Ranged` drops the same trophy. |
| 3 | draugr_elite | `Draugr_Elite` | Undead | `TrophyDraugrElite` | 0.10 | `Entrails` | |
| 4 | fenring | `Fenring` | MountainMonsters | `TrophyFenring` | 0.10 | `WolfFang` | |
| 5 | seeker | `Seeker` | MistlandsMonsters | `TrophySeeker` | 0.05 | `Carapace` | |
| 6 | charred | `Charred_Melee` | Demon | `TrophyCharredMelee` | 0.05 | `CharredBone` | Ashlands. `Charred_Archer` / `Charred_Mage` are alternatives with their own trophies. |
| 7 | jotun | `JotunWarrior` | DeepNorth | `TrophyJotunWarrior` | 0.10 | `Leatherstraps` | Deep North. `JotunWarriorDualWield` drops the same trophy. |

All eight base prefabs are `Humanoid + MonsterAI`, none has `Tameable`
(must be added), none has `TimedDestruction`.

### Other viable candidates

Meadows/Black Forest: `Greydwarf` (`TrophyGreydwarf` 0.05), `Greydwarf_Elite`
(`TrophyGreydwarfBrute` 0.10), `Greydwarf_Shaman` (`TrophyGreydwarfShaman`
0.10), `Neck` (`TrophyNeck` 0.05), `Troll` (`TrophyFrostTroll` 0.5 — note the
forest troll drops the *frost* trophy prefab; both share shared-name
`$item_trophy_troll`).
Swamp: `Blob` (`TrophyBlob` 0.10), `Wraith` (`TrophyWraith` 0.05), `Abomination`
(`TrophyAbomination` 0.5), `Skeleton_Poison` (`TrophySkeletonPoison` 0.10).
Mountain: `Ulv` (`TrophyUlv` 0.10), `Fenring_Cultist` (`TrophyCultist` 0.10),
`StoneGolem` (`TrophySGolem` 0.05), `Hatchling` (`TrophyHatchling` 0.10).
Plains: `Goblin` (`TrophyGoblin` 0.10), `GoblinBrute` (`TrophyGoblinBrute`
0.05), `GoblinShaman` (`TrophyGoblinShaman` 0.10), `Deathsquito`
(`TrophyDeathsquito` 0.05).
Mistlands: `SeekerBrute` (`TrophySeekerBrute` 0.05), `Gjall` (`TrophyGjall`
0.3), `Tick` (`TrophyTick` 0.05), `Dverger*` (`TrophyDvergr` 0.05).
Ashlands: `Charred_Archer` (`TrophyCharredArcher` 0.05), `Charred_Mage`
(`TrophyCharredMage` 0.05), `Morgen` (`TrophyMorgen` 0.05), `FallenValkyrie`
(`TrophyFallenValkyrie` 0.05), `Volture` (`TrophyVolture` 0.10), `Asksvin`
(`TrophyAsksvin` 0.10, already Tameable).
Deep North: `JotunWitch` (`TrophyJotunWitch` 0.10), `Elaking` (`TrophyElaking`
0.10), `ElakingMole` (`TrophyMole` 0.10), `Barka` (`TrophyBarka` 0.10),
`BlobMork` (`TrophyBlob_Morkhalla` 0.10), `Writhan` (`TrophyWrithan` 0.10),
`Greydwarf_Frozen` (`TrophyGreydwarf` 0.05), `Skeleton_DeepNorth`
(`TrophySkeleton` 0.10), `Moose` (`TrophyMoose` 0.10, already Tameable),
`Bjorn` (`TrophyBjorn` 0.10), `Unbjorn` (`TrophyBjornUndead` 0.10).

### Vanilla summoned-clone prefabs (reference pattern)

These are the game's own tamed summons: `Skeleton_Friendly` (Dead Raiser),
`Troll_Summoned` (faction `PlayerSpawned`), and the 1.0 spirit-caller set
`Wolf_spiritcaller`, `Boar_spiritcaller`, `Bjorn_spiritcaller`,
`Moose_spiritcaller` (faction `Players`, `Tameable`). Inspect their
`Tameable`/`MonsterAI` settings in-game when building the clone setup in
Phase 4; they are the closest thing to an official recipe.

---

## Raw catalogue

# Trophies (prefab name | shared name token)
TrophyAbomination | $item_trophy_abomination
TrophyAsksvin | $item_trophy_asksvin
TrophyBarka | $item_trophy_barka
TrophyBjorn | $item_trophy_bjorn
TrophyBjornUndead | $item_trophy_bjorn_undead
TrophyBlob | $item_trophy_blob
TrophyBlob_Frost | $item_trophy_blob_frost
TrophyBlob_Lava | $item_trophy_blob_lava
TrophyBlob_Morkhalla | $item_trophy_blob_morkhalla
TrophyBoar | $item_trophy_boar
TrophyBonemass | $item_trophy_bonemass
TrophyBonemawSerpent | $item_trophy_bonemaw
TrophyCharredArcher | $item_trophy_charredarcher
TrophyCharredMage | $item_trophy_charredmage
TrophyCharredMelee | $item_trophy_charredmelee
TrophyCultist | $item_trophy_cultist
TrophyCultist_Hildir | $item_trophy_cultist_hildir
TrophyDeathsquito | $item_trophy_deathsquito
TrophyDeer | $item_trophy_deer
TrophyDeerWhite | $item_trophy_deer_white
TrophyDragonQueen | $item_trophy_dragonqueen
TrophyDraugr | $item_trophy_draugr
TrophyDraugrElite | $item_trophy_draugrelite
TrophyDraugrFem | $item_trophy_draugr
TrophyDvergr | $item_trophy_dvergr
TrophyEikthyr | $item_trophy_eikthyr
TrophyElaking | $item_trophy_elaking
TrophyFader | $item_trophy_fader
TrophyFallenValkyrie | $item_trophy_fallenvalkyrie
TrophyFenring | $item_trophy_fenring
TrophyForestTroll | $item_trophy_troll
TrophyFrostTroll | $item_trophy_troll
TrophyGhost | $item_trophy_ghost
TrophyGjall | $item_trophy_gjall
TrophyGoblin | $item_trophy_goblin
TrophyGoblinBrute | $item_trophy_goblinbrute
TrophyGoblinBruteBrosBrute | $item_trophy_brutebro
TrophyGoblinBruteBrosShaman | $item_trophy_shamanbro
TrophyGoblinKing | $item_trophy_goblinking
TrophyGoblinShaman | $item_trophy_goblinshaman
TrophyGreydwarf | $item_trophy_greydwarf
TrophyGreydwarfBrute | $item_trophy_greydwarfbrute
TrophyGreydwarfShaman | $item_trophy_greydwarfshaman
TrophyGrowth | $item_trophy_growth
TrophyHare | $item_trophy_hare
TrophyHatchling | $item_trophy_hatchling
TrophyJotunWarrior | $item_trophy_jotunwarrior
TrophyJotunWitch | $item_trophy_jotunwitch
TrophyKvastur | $enemy_kvastur
TrophyLeech | $item_trophy_leech
TrophyLox | $item_trophy_lox
TrophyMole | $item_trophy_mole
TrophyMoose | $item_trophy_moose
TrophyMorgen | $item_trophy_morgen
TrophyNeck | $item_trophy_neck
TrophySeal | $item_trophy_seal
TrophySeeker | $item_trophy_seeker
TrophySeekerBrute | $item_trophy_seeker_brute
TrophySeekerQueen | $item_trophy_seekerqueen
TrophySerpent | $item_trophy_serpent
TrophySGolem | $item_trophy_sgolem
TrophySkeleton | $item_trophy_skeleton
TrophySkeletonHildir | $item_trophy_skeleton_hildir
TrophySkeletonPoison | $item_trophy_skeletonpoison
TrophySurtling | $item_trophy_surtling
TrophyTheElder | $item_trophy_elder
TrophyTick | $item_trophy_tick
TrophyUlv | $item_trophy_ulv
TrophyVolture | $item_trophy_volture
TrophyWolf | $item_trophy_wolf
TrophyWraith | $item_trophy_wraith
TrophyWrithan | $item_trophy_writhan

# Creatures (prefab | faction | components | drops)
Abomination | Undead | Humanoid+MonsterAI | TrophyAbomination@0.5, Root@1, Guck@1
Asksvin | Demon | Humanoid+MonsterAI+Tameable+Procreation | TrophyAsksvin@0.1, AskBladder@1, AskHide@1, AsksvinMeat@1
Asksvin_hatchling | Undead | Humanoid+MonsterAI | AskBladder@0.2, AskHide@0.2, AsksvinMeat@0.2
Aspect_Bonemass | Boss | Humanoid+MonsterAI | 
Aspect_Eikthyr | Boss | Humanoid+MonsterAI | 
Aspect_Elder | Boss | Humanoid+MonsterAI | 
Aspect_Fader | Boss | Humanoid+MonsterAI | 
Aspect_Moder | Boss | Humanoid+MonsterAI | 
Aspect_SeekerQueen | Boss | Humanoid+MonsterAI | 
Aspect_TentaRoot | Boss | Humanoid+MonsterAI | 
Aspect_Yagluth | Boss | Humanoid+MonsterAI | 
Barka | DeepNorth | Humanoid+MonsterAI | TrophyBarka@0.1, BarkaBranch@1
Bat | MountainMonsters | Humanoid+MonsterAI | LeatherScraps@0.5
Bat_Swamp | Undead | Humanoid+MonsterAI | LeatherScraps@0.5
Bjorn | ForestMonsters | Humanoid+MonsterAI | BjornPaw@1, BjornMeat@1, BjornHide@1, TrophyBjorn@0.1
Bjorn_sleeping | ForestMonsters | Humanoid+MonsterAI | BjornPaw@1, BjornMeat@1, BjornHide@1, TrophyBjorn@0.1
Bjorn_spiritcaller | Players | Humanoid+MonsterAI+Tameable | 
Blob | Undead | Humanoid+MonsterAI | TrophyBlob@0.1, Ooze@1
BlobAspect | Undead | Humanoid+MonsterAI | TrophyBlob@0.1, Ooze@1
BlobElite | Undead | Humanoid+MonsterAI | Ooze@1, IronScrap@0.33, TrophyBlob@0.1, Blob@1
BlobFrost | MountainMonsters | Humanoid+MonsterAI | Crystal@1, TrophyBlob_Frost@0.1
BlobLava | Demon | Humanoid+MonsterAI | ProustitePowder@1, SulfurStone@1, TrophyBlob_Lava@0.1
BlobMork | DeepNorth | Humanoid+MonsterAI | TrophyBlob_Morkhalla@0.1, BlobMorkMini@1, OozeMork@0.5
BlobMorkMini | DeepNorth | Humanoid+MonsterAI | OozeMork@0.25
BlobTar | Undead | Humanoid+MonsterAI | TrophyGrowth@0.1, Tar@1
Boar | ForestMonsters | Humanoid+MonsterAI+Tameable+Procreation | RawMeat@1, LeatherScraps@1, TrophyBoar@0.15
Boar_piggy | ForestMonsters | Humanoid+AnimalAI | 
Boar_spiritcaller | Players | Humanoid+MonsterAI+Tameable | 
BogWitchKvastur | Dverger | Humanoid+MonsterAI | Wood@1, Resin@1, TrophyKvastur@1
Bonemass | Boss | Humanoid+MonsterAI | TrophyBonemass@1, Wishbone@1
BonemawSerpent | Demon | Humanoid+MonsterAI | TrophyBonemawSerpent@0.33, BoneMawSerpentMeat@1, BonemawSerpentTooth@1
Charred_Archer | Demon | Humanoid+MonsterAI | CharredBone@1, TrophyCharredArcher@0.05
Charred_Archer_Fader | Demon | Humanoid+MonsterAI | 
Charred_Mage | Demon | Humanoid+MonsterAI | CharredBone@1, TrophyCharredMage@0.05
Charred_Melee | Demon | Humanoid+MonsterAI | CharredBone@1, TrophyCharredMelee@0.05
Charred_Melee_Dyrnwyn | Demon | Humanoid+MonsterAI | DyrnwynHiltFragment@1
Charred_Melee_Fader | Demon | Humanoid+MonsterAI | 
Charred_Twitcher | Demon | Humanoid+MonsterAI | CharredBone@1
Charred_Twitcher_Summoned | Demon | Humanoid+MonsterAI | CharredBone@1
Chicken | ForestMonsters | Humanoid+AnimalAI | ChickenMeat@0.25, Feathers@0.5
Deathsquito | PlainsMonsters | Humanoid+MonsterAI | Needle@1, TrophyDeathsquito@0.05
Deer | ForestMonsters | AnimalAI | DeerMeat@1, DeerHide@1, TrophyDeer@0.5
Deer_White | ForestMonsters | AnimalAI | DeerMeat@1, TrophyDeerWhite@1
Dragon | Boss | Humanoid+MonsterAI | TrophyDragonQueen@1, DragonTear@1
Draugr | Undead | Humanoid+MonsterAI | Entrails@1, TrophyDraugr@0.1
Draugr_Elite | Undead | Humanoid+MonsterAI | Entrails@1, TrophyDraugrElite@0.1
Draugr_Elite_sleeping | Undead | Humanoid+MonsterAI | Entrails@1, TrophyDraugrElite@0.1
Draugr_Ranged | Undead | Humanoid+MonsterAI | Entrails@1, TrophyDraugr@0.1
Draugr_Ranged_sleeping | Undead | Humanoid+MonsterAI | Entrails@1, TrophyDraugr@0.1
Draugr_sleeping | Undead | Humanoid+MonsterAI | Entrails@1, TrophyDraugr@0.1
Dverger | Dverger | Humanoid+MonsterAI | Softtissue@0.25, BlackMarble@0.5, Coins@1, TrophyDvergr@0.05
DvergerAshlands | Dverger | Humanoid+MonsterAI | Softtissue@0.25, BlackMarble@0.5, Coins@1, TrophyDvergr@0.05
DvergerDeepNorth | Dverger | Humanoid+MonsterAI | Coins@1, TrophyDvergr@0.05, AncientGemstoneBlack@0.1, AncientGemstoneGreen@0.1, AncientGemstoneOrange@0.1, AncientGemstonePurple@0.1
DvergerMage | Dverger | Humanoid+MonsterAI | Softtissue@0.25, BlackMarble@0.5, Coins@1, TrophyDvergr@0.05
DvergerMageFire | Dverger | Humanoid+MonsterAI | Softtissue@0.25, BlackMarble@0.5, Coins@1, TrophyDvergr@0.05
DvergerMageIce | Dverger | Humanoid+MonsterAI | Softtissue@0.25, BlackMarble@0.5, Coins@1, TrophyDvergr@0.05
DvergerMageSupport | Dverger | Humanoid+MonsterAI | Softtissue@0.25, BlackMarble@0.5, Coins@1, TrophyDvergr@0.05
DvergerTest | Dverger | Humanoid+MonsterAI | Coins@0.25, BlackMetalScrap@1, Pukeberries@1, TrophyGoblinShaman@0.1
Eikthyr | Boss | Humanoid+MonsterAI | TrophyEikthyr@1, HardAntler@1
Elaking | DeepNorth | Humanoid+MonsterAI | ElakingHairBundle@1, TrophyElaking@0.1, MoldKeys@0.2
ElakingLantern | DeepNorth | Humanoid+MonsterAI | ElakingHairBundle@1, TrophyElaking@0.1
ElakingMole | DeepNorth | Humanoid+MonsterAI | TrophyMole@0.1, MoleClaws@1, MoldKeys@0.5
Fader | Boss | Humanoid+MonsterAI | TrophyFader@1, FaderDrop@1
FallenValkyrie | Demon | Humanoid+MonsterAI | CelestialFeather@1, TrophyFallenValkyrie@0.05
FallenWarrior | Undead | Humanoid+MonsterAI | OrbFrostFire@0.5, OrbThunderBlood@0.5
Fenring | MountainMonsters | Humanoid+MonsterAI | WolfFang@1, TrophyFenring@0.1
Fenring_Cultist | MountainMonsters | Humanoid+MonsterAI | JuteRed@1, TrophyCultist@0.1
Fenring_Cultist_Hildir | MountainMonsters | Humanoid+MonsterAI | chest_hildir2@1, TrophyCultist_Hildir@1
Fenring_Cultist_Hildir_nochest | MountainMonsters | Humanoid+MonsterAI | TrophyCultist_Hildir@1
FrostWisp | TrainingDummy | Humanoid+MonsterAI | LeatherScraps@0.5
FrozenKing | Boss | Humanoid+MonsterAI | 
FrozenKing_p2 | Boss | Humanoid+MonsterAI | 
FrozenKing_p3 | Boss | Humanoid+MonsterAI | FrozenKingDrop@1, CrownJewel@1
Frysling | TrainingDummy | Humanoid+MonsterAI | FrostCore@1
gd_king | Boss | Humanoid+MonsterAI | TrophyTheElder@1, CryptKey@1
Ghost | Undead | Humanoid+MonsterAI | Ectoplasm@1, TrophyGhost@0.1
Ghost_old | Undead | Humanoid+MonsterAI | Ectoplasm@0.5
Ghost_sleeping | Undead | Humanoid+MonsterAI | Ectoplasm@1, TrophyGhost@0.1
Ghost_Void | Undead | Humanoid+MonsterAI | KnifeVoid@1
Gjall | MistlandsMonsters | Humanoid+MonsterAI | Bilebag@1, TrophyGjall@0.3
Goblin | PlainsMonsters | Humanoid+MonsterAI | Coins@0.25, BlackMetalScrap@1, TrophyGoblin@0.1
Goblin_Gem | ForestMonsters | AnimalAI | GemstoneBlue@1, GemstoneGreen@1, GemstoneRed@1
GoblinArcher | PlainsMonsters | Humanoid+MonsterAI | Coins@0.25, BlackMetalScrap@1, TrophyGoblin@0.1
GoblinBrute | PlainsMonsters | Humanoid+MonsterAI | Coins@1, BlackMetalScrap@1, GoblinTotem@0.1, TrophyGoblinBrute@0.05
GoblinBrute_Hildir | PlainsMonsters | Humanoid+MonsterAI | chest_hildir3@1
GoblinBruteBros | PlainsMonsters | Humanoid+MonsterAI | GoblinShaman_Hildir@1, TrophyGoblinBruteBrosBrute@1
GoblinBruteBros_nochest | PlainsMonsters | Humanoid+MonsterAI | GoblinShaman_Hildir_nochest@1, TrophyGoblinBruteBrosBrute@1
GoblinDeepNorth | PlainsMonsters | Humanoid+MonsterAI | Coins@0.25, AncientCoin@1, Lingonberry@0.2
GoblinKing | Boss | Humanoid+MonsterAI | TrophyGoblinKing@1, YagluthDrop@1
GoblinShaman | PlainsMonsters | Humanoid+MonsterAI | Coins@0.25, BlackMetalScrap@1, Pukeberries@1, TrophyGoblinShaman@0.1
GoblinShaman_Hildir | PlainsMonsters | Humanoid+MonsterAI | chest_hildir3@1, TrophyGoblinBruteBrosShaman@1
GoblinShaman_Hildir_nochest | PlainsMonsters | Humanoid+MonsterAI | TrophyGoblinBruteBrosShaman@1
Greydwarf | ForestMonsters | Humanoid+MonsterAI | GreydwarfEye@0.5, Stone@1, Wood@1, Resin@1, TrophyGreydwarf@0.05
Greydwarf_Elite | ForestMonsters | Humanoid+MonsterAI | GreydwarfEye@0.5, Stone@1, Wood@1, Dandelion@1, AncientSeed@0.33, TrophyGreydwarfBrute@0.1
Greydwarf_Frozen | DeepNorth | Humanoid+MonsterAI | GreydwarfEye@0.5, Wood@1, Resin@1, TrophyGreydwarf@0.05, Snowball@1, Ice@1
Greydwarf_Shaman | ForestMonsters | Humanoid+MonsterAI | GreydwarfEye@0.5, Wood@1, Resin@1, TrophyGreydwarfShaman@0.1, Pukeberries@1
Greydwarf_Shaman_Frozen | DeepNorth | Humanoid+MonsterAI | GreydwarfEye@0.5, Wood@1, Resin@1, TrophyGreydwarfShaman@0.1, Pukeberries@1, Ice@1
Greyling | ForestMonsters | Humanoid+MonsterAI | Resin@1
Hare | AnimalsVeg | AnimalAI | HareMeat@1, ScaleHide@1, TrophyHare@0.05
Hatchling | MountainMonsters | Humanoid+MonsterAI | TrophyHatchling@0.1, FreezeGland@1
Hen | ForestMonsters | Humanoid+MonsterAI+Tameable+Procreation | ChickenMeat@1, Feathers@1
Hive | Boss | Humanoid+MonsterAI | TrophySeekerQueen@1, QueenDrop@1
JotunWarrior | DeepNorth | Humanoid+MonsterAI | MoldArmormediumChest@0.05, MoldArmorMediumHelmet@0.05, MoldArmorMediumLegs@0.05, MemorialCoal@0.2, TrophyJotunWarrior@0.1, Leatherstraps@1
JotunWarriorDualWield | DeepNorth | Humanoid+MonsterAI | MoldArmorGoldChest@0.07, MoldArmorGoldHelmet@0.07, MoldArmorGoldLegs@0.07, MemorialCoal@0.2, TrophyJotunWarrior@0.1, Leatherstraps@1
JotunWitch | DeepNorth | Humanoid+MonsterAI | NornThread@1, TrophyJotunWitch@0.1, BloodGoldKey@0.1, MoldArmorMageChest@0.05, MoldArmorMageHelmet@0.05, MoldArmorMageLegs@0.05
Leech | Undead | Humanoid+MonsterAI | TrophyLeech@0.1, Bloodbag@1
Leech_cave | Undead | Humanoid+MonsterAI | TrophyLeech@0.1, Bloodbag@1
Lox | PlainsMonsters | Humanoid+MonsterAI+Tameable+Procreation | LoxMeat@1, TrophyLox@0.1, LoxPelt@1
Lox_Calf | PlainsMonsters | Humanoid+AnimalAI | LoxMeat@1
Mistile | Dverger | Humanoid+MonsterAI | 
Moose | DeepNorth | Humanoid+MonsterAI+Tameable+Procreation | MooseMeat@1, TrophyMoose@0.1, MooseHide@1, MooseSinew@1
Moose_calf | DeepNorth | Humanoid+AnimalAI | 
Moose_spiritcaller | Players | Humanoid+MonsterAI+Tameable | 
Morgen | Demon | Humanoid+MonsterAI | MorgenSinew@1, MorgenHeart@0.8, TrophyMorgen@0.05
Morgen_NonSleeping | Demon | Humanoid+MonsterAI | MorgenSinew@1, MorgenHeart@0.8, TrophyMorgen@0.05
Neck | ForestMonsters | Humanoid+MonsterAI | NeckTail@0.7, TrophyNeck@0.05
piece_TrainingDummy | TrainingDummy | Humanoid+MonsterAI | 
Player | Players | Humanoid | 
Seal | DeepNorth | AnimalAI | SealHide@1, TrophySeal@0.1, SealBlubber@1
Seal_Pup | DeepNorth | AnimalAI | SealBlubber@0.1
Seeker | MistlandsMonsters | Humanoid+MonsterAI | BugMeat@1, Carapace@1, TrophySeeker@0.05
SeekerBrood | MistlandsMonsters | Humanoid+MonsterAI | RoyalJelly@0.5
SeekerBrute | MistlandsMonsters | Humanoid+MonsterAI | BugMeat@1, Carapace@1, TrophySeekerBrute@0.05, Mandible@1
SeekerQueen | Boss | Humanoid+MonsterAI | TrophySeekerQueen@1, QueenDrop@1
Serpent | SeaMonsters | Humanoid+MonsterAI | TrophySerpent@0.33, SerpentMeat@1, SerpentScale@1
ShadowPerson | DeepNorth | Humanoid+MonsterAI | 
Skeleton | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
Skeleton_aspect | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
Skeleton_DeepNorth | DeepNorth | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1, Ice@1
Skeleton_Friendly | Players | Humanoid+MonsterAI+Tameable | 
Skeleton_Hildir | Undead | Humanoid+MonsterAI | chest_hildir1@1, TrophySkeletonHildir@1
Skeleton_Hildir_nochest | Undead | Humanoid+MonsterAI | TrophySkeletonHildir@1
Skeleton_Meadows | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
Skeleton_Meadows_noarcher | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
Skeleton_Mountains | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
Skeleton_Mountains_noarcher | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
Skeleton_NoArcher | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
Skeleton_Poison | Undead | Humanoid+MonsterAI | TrophySkeletonPoison@0.1, BoneFragments@1
Skeleton_Swamps | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
Skeleton_Swamps_noarcher | Undead | Humanoid+MonsterAI | TrophySkeleton@0.1, BoneFragments@1
staff_greenroots_tentaroot | Players | Humanoid+MonsterAI | 
StoneGolem | ForestMonsters | Humanoid+MonsterAI | TrophySGolem@0.05, Stone@1, Crystal@1
Surtling | Demon | Humanoid+MonsterAI | Coal@1, SurtlingCore@0.5, TrophySurtling@0.05
Tendril | Boss | Humanoid+MonsterAI | 
Tendril_back | Boss | Humanoid+MonsterAI | 
TentaRoot | Boss | Humanoid+MonsterAI | 
TentaRoot_wild | ForestMonsters | Humanoid+MonsterAI | 
TheHive | MistlandsMonsters | Humanoid+MonsterAI | TrophyHatchling@0.1, FreezeGland@1
Tick | MistlandsMonsters | Humanoid+MonsterAI | GiantBloodSack@1, TrophyTick@0.05
TrainingDummy | Undead | Humanoid+MonsterAI | Entrails@0.5, TrophyDraugr@0.1
Troll | ForestMonsters | Humanoid+MonsterAI | Coins@1, TrophyFrostTroll@0.5, TrollHide@1
Troll_sleeping | ForestMonsters | Humanoid+MonsterAI | Coins@1, TrophyFrostTroll@0.5, TrollHide@1
Troll_Summoned | PlayerSpawned | Humanoid+MonsterAI | 
TrollFrost | DeepNorth | Humanoid+MonsterAI | 
Ulv | MountainMonsters | Humanoid+MonsterAI | WolfFang@0.5, TrophyUlv@0.1
Unbjorn | PlainsMonsters | Humanoid+MonsterAI | TrophyBjornUndead@0.1, BjornMeat@1, RottenMeat@0.8, UndeadBjornRibcage@1, BjornHide@1
Volture | Demon | Humanoid+MonsterAI | TrophyVolture@0.1, VoltureMeat@1, Feathers@0.5, VoltureEgg@0.5
Wolf | MountainMonsters | Humanoid+MonsterAI+Tameable+Procreation | TrophyWolf@0.1, WolfMeat@1, WolfPelt@1, WolfFang@0.4
Wolf_cub | MountainMonsters | Humanoid+AnimalAI | 
Wolf_spiritcaller | Players | Humanoid+MonsterAI+Tameable+Procreation | 
Wraith | Undead | Humanoid+MonsterAI | TrophyWraith@0.05, Chain@1
Writhan | Undead | Humanoid+MonsterAI | TrophyWrithan@0.1, WrithanRoots@1
