# Lost In Transit
Lost in Transit is a mod for Risk of Rain 2 that adapts content from Risk of Rain 1, intending to balance and redesign varying features in ways that preserve that best original identites while also respecting the balance and gameplay changes made in Risk of Rain 2.

In its current state, the mod is intended to be an adaptation over a pure port - some features may have changed function or identities for varying reasons. This is done to help players less familiar with Risk of Rain 1's content ease into it and keep things simple. 

If you have any issues with the mod, any feedback you'd like to give, any ideas for new content, or would like to follow progress, you can either join the Lost in Transit Discord Server (https://discord.gg/jzxXsQEZ5y) or you can contact me on Discord (swuff★#2224) - shoot me a DM or ping me in a mutual server. The mod is in active development, so expect new features soon. And as a personal note: I'm still fairly new to modding, so expect issues. 

## Items / Equipment

| Icon | Item | Description | Rarity |
|:-|-|------|-|
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconRootNewer.png) | **Bitter Root** | Increase health regeneration by 3 hp/s for 3s (+3s per stack) after killing an enemy. | Common |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconSavingsNew.png) | **Life Savings** | Gain 1 (+1 per stack) Chest's worth of gold at the beginning of each stage. | Common |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/meatnugger.png) | **Meat Nugget** | 8% chance on hitting an enemy to spawn a nugget that increases health regeneration by +1.6 hp/s for 2 seconds (+2 seconds per stack). | Common |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/mysteriousvial.png) | **Mysterious Vial** | Increase health regeneration by +0.8 hp/s (+0.8 hp/s per stack). | Common |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconCatNew.png) | **Beckoning Cat** | Elite monsters have a 4.5% (+1.5% per stack) chance to drop items on death. | Uncommon |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconPickupGoldGun.png) | **Golden Gun** | Deal extra damage based on held gold, up to +40% damage (+20% per stack) at 300 gold (+150 per stack, scaling with time). | Uncommon |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconguardiansheart%201.png) | **Guardian's Heart** | Gain 60 shield. All shields become gated, blocking any damage exceeding your current amount of shield and giving 40 armor for 3 (+50% per stack) seconds when broken. | Uncommon |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconShacklesNew.png) | **Prison Shackles** | Slow monsters for -30% attack speed for 2 (+2 per stack) seconds. | Uncommon |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconArmor.png) | **Repulsion Armor** | Getting hit 6 times activates the repulsion armor, giving 500 armor and reflecting projectiles for 3 (+1.5 per stack) seconds.
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texLopperNew.png) | **Ol' Lopper** | Deal 2% (+2% per stack) more damage for every 1% health missing from enemies, up to 50% (+50% per stack) damage. | Rare |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/Mitosis.png) | **Rapid Mitosis** | Decrease equipment cooldown by 35% (+35% per stack). While you have at least 1 equipment charge, health regeneration is increased by 3.8 hp/s (+1.9 hp/s per stack). | Rare |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texThallium.png) | **Thallium** | 10% chance to inflict thallium poisoning for 500% (+250% per stack) of enemy's base damage and slow by 75%. | Rare |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconAmethyst.png) | **Gigantic Amethyst** | Reset skill cooldowns on use. | Equipment |
|![](https://raw.githubusercontent.com/swuff-star/LostInTransit/master/LIT/Assets/Icons/texIconThqwib.png) | **Thqwib** | Release a bloom of 30 thqwibs, detonating on impact for 200% damage. | Equipment |

## Credits
* Code - Gaforb, Nebby, swuff★
* Art/Modelling/Animation - bruh, GEMO, LucidInceptor, Nebby, SOM
* Sound - UnknownGlaze, neik, SOM
* Writing - BlimBlam, Lyrical Endymion, QandQuestion, swuff★, T_Dimensional
* Additional support/special thanks - Draymarc, KevinFromHPCustomerService, KomradeSpectre, Moffein, rob, Shared, TheTimesweeper, xpcybic, /vm/

## Changelog

### 0.4.0
* General
	* Updated to Survivors of the Void & MSU 1.0.0
* Items
	* Added Bitter Root (Re-implementation of old Bison Steak)
	* Tweaked Life Savings (Keep 5% of gold -> Gain 1 chest in gold at beginning of each stage)
	* Added Meat Nugget
	* Added Guardian's Heart
	* Disabled Smart Shopper (Reworking)
	* Added Repulsion Armor
	* Added Ol' Lopper
	* Added Rapid Mitosis
* Equipment
	* Added Thqwib
* Elites
	* Disabled all Elites (Reworking)
* Known Bugs
	* No sounds!

### 0.3.9

* Updated to use MSU 0.6.0
* Re-Enabled Bitter Root
* Thallium no longer causes on kill effects to not trigger
* Fixed an issue where Leeching elites may rarely set the regen boost strength to negative values.

### 0.3.8
* Re-did IDRS for Volatile, Leeching and Frenzied Elites
* Added support for the following mod's idrs:
  * Starstorm2's Nemmando, Executioner & Security Drone
  * Enforcer's Nemesis Enforcer
  * Chef
  * Sniper
  * Miner
  * HAN-D
  * Archaic Wisps
  * Clayman
  * Ancient Wisp
* Changed the Volatile, Leeching and Frenzied Ramps to be more distinct from each other.

### 0.3.7

* Items
	* Removed Blessed Dice for the time being, as theyre unfinished.
* Elites:
	* Blighted: Fixed a bug where the game would crash upon entering to the bazaar.

### 0.3.6
* Items
	* Prison Shackles: Reduced attack speed reduction (60% -> 30%)
* Elites
	* Leeching: Increased time between healing bursts from 10 to 15 seconds
	* Leeching: Now have a visible window before healing, accompanied by an AoE radius
	* Leeching: Damaging a Leeching elite during the aforementioned window now reduces total healing done, up to 50%
	* Leeching: Reduced particle count on VFX
	* Volatile: Added cooldown between bombs appearing
	* Volatile: Maximum bomb count halved
	* Volatile: Bomb damage halved
	* Blighted: Fixed bug where spawn rate didn't reset in-between runs
	* Blighted: Replaced configurable blacklist with a configurable weight system
	* Blighted: Teleporter bosses can now be Blighted, for double the cost (also configurable)
	* Blighted: Removed SFX from smoke visual
	* Blighted: Lowered CDR (50% -> 25%)
	* Blighted: Slightly lowered damage boost (-25%), but raised attack speed (+110%)
	* Blighted: Smoke effect no longer spawns when the aspect is held by a player

### 0.3.5
* Elites
	* Blighted: Added config to allow bosses to spawn as Blighted enemies
	* Blighted: Fixed invisibility

### 0.3.4
*Developer Note: PLEASE wipe your config.*
* Items
	* Beckoning Cat: Added On-Proc sound effect
	* Bitter Root: Temporariliy disabled. Go join Mortar Tube!
	* Gigantic Amethyst: Added On-Use sound effect
	* Golden Gun: Hopefully tackled some issues with how the number of buffs was calculated
	* Telescopic Sight: Added On-Proc sound effect
	* Thallium: Added On-Proc sound effect
* Equipments
	* Gigantic Amethyst: Added on use sound effect
* Elites
	* Blighted: Lowered extra damage (300% -> 200%)
	* Blighted: Changed how visibility / invisiblity is handled to make them visible for longer periods of time
	* Blighted: Should be less common overall.
	* Leeching: Added a sound effect to their AoE heal
	* Frenzied: Added sound effect when Dash is ready
	* Volatile: Added missing IDRS
* BugFixes:
	* Volatile elites should no longer cause a myriad of issues, included but not limited to:
		* Volatile Bombs exploding constantly rarely. [link](https://github.com/swuff-star/LostInTransit/issues/5)
		* Glacial Elites's freezing explosion constantly triggering. [link](https://github.com/swuff-star/LostInTransit/issues/4)
		* Volatile Elites should no longer cause infinite explosions when interacting with Enforcer's shield. [link](https://github.com/swuff-star/LostInTransit/issues/8)
		* Causing the camera to get stuck while having aetherium installed.
	* Disabling blighted elites should no longer cause extreme stat scaling bugs. [link](https://github.com/swuff-star/LostInTransit/issues/2)
	* Prison Shackles should no longer trigger on attacks with proc coefficient of 0. [link](https://github.com/swuff-star/LostInTransit/issues/7)

	
(Old changelogs can be found [Here](https://github.com/swuff-star/LostInTransit/blob/master/OldChangelogs.md))
