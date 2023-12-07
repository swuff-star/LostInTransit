using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm;
using R2API;
using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using UnityEngine;
using EntityStates.GoldGat;

namespace LostInTransit.Items
{
    /*TODO:
     * > FireBallDash (Volcanic Egg) and Gateway (Eccentric Vase) are on blacklist, find unique upgrades (On blacklist because they use vehicles.)
     * > BFG (Preon) just lowers how fast the bfg projectile appears, must figure out how to fire a XIConstruct death laser.}
     * > Primordial cube may do nothing if doubled, need more testing
     * > Executive card may do nothing if doubled, needs more testing.
     * > Recycler may do nothing if doubled, needs more testing.
     * > Gnarled Woodsprite needs testing.
     * > Radar Scanner still useless
     * > Remote Caffeinator seems to spawn 2 vending machines, only one seems interactable? needs more testing
     * > Make the upgraded royal capacitor spawn a AOE effect alongside the lightning strike
     * > The Crowdfunder does nothing, needs more testing
     * > Add more funny messages to the used trophy hunter's tricorn
     */

    /*
     * The following equipments dont get any special methods/hooks because using the equipment memthod twice yeilds the "Improved" result.
     * 
     * FireCommandMissile
     * FireFruit
     * FireSaw
     * FireMolotov
     * 
     * The following equipments dont have any special features ddue to being difficult to implement.
     * FireFireballDash
     */

    public class BeatingEmbryo : ItemBase
    {
        public override ItemDef ItemDef => LITAssets.LoadAsset<ItemDef>("BeatingEmbryo", LITBundle.Items);

        public string[] bossHunterOptions = new string[] { "EQUIPMENT_BOSSHUNTERCONSUMED_CHAT", "LIT_EQUIPMENT_BOSSHUNTERCONSUMED_CHAT" };

        public override void Initialize()
        {
            //Crit On use special buff
            HG.ArrayUtils.ArrayAppend(ref LITContent.Instance.SerializableContentPack.buffDefs, LITAssets.LoadAsset<BuffDef>("bdHiddenCritDamage", LITBundle.Items));
            RecalculateStatsAPI.GetStatCoefficients += (body, args) =>
            {
                if (body.GetBuffCount(LITContent.Buffs.bdHiddenCritDamage) > 0)
                {
                    args.critDamageMultAdd += 1;
                }
            };

            //Lines with //* are methods that are either unfinished, or have ToDo's
            //commmented out hooks are methods that are almost finished, but produce invalid IL
            RoR2Application.onLoad += () =>
            {
                On.RoR2.EquipmentSlot.FireBfg += FireBFG; //*
                IL.RoR2.EquipmentSlot.FireBlackhole += FireBlackHole; //*
                IL.RoR2.EquipmentSlot.FireDroneBackup += FireDroneBackup;
                IL.RoR2.EquipmentSlot.FireMeteor += FireMeteor;
                IL.RoR2.EquipmentSlot.FireCritOnUse += FireCritOnUse;
                On.RoR2.EquipmentSlot.FireJetpack += FireJetpack;
                IL.RoR2.EquipmentSlot.FireLightning += FireLightning;
                IL.RoR2.EquipmentSlot.FirePassiveHealing += FirePassiveHealing;
                IL.RoR2.EquipmentSlot.FireBurnNearby += FireBurnNearby;
                IL.RoR2.EquipmentSlot.FireScanner += FireScanner;
                IL.RoR2.EquipmentSlot.FireCrippleWard += FireCrippleWard;
                //IL.RoR2.EquipmentSlot.FireGateway += FireGateway;
                IL.RoR2.EquipmentSlot.FireTonic += FireTonic; //*
                BeatingEmbryoManager.AddEmbryoEffect(RoR2Content.Equipment.Cleanse, FireCleanse);
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.FireBallDash);
                //IL.RoR2.EquipmentSlot.FireRecycle += FireRecycle;
                IL.RoR2.EquipmentSlot.FireGainArmor += FireGainArmor;
                IL.RoR2.EquipmentSlot.FireLifeStealOnHit += FireLifeStealOnHit;
                IL.RoR2.EquipmentSlot.FireTeamWarCry += FireTeamWarCry;
                IL.RoR2.EquipmentSlot.FireDeathProjectile += FireDeathProjectile;
                IL.RoR2.EquipmentSlot.FireVendingMachine += FireVendingMachine;
                IL.RoR2.EquipmentSlot.FireBossHunter += FireBossHunter;
                IL.RoR2.EquipmentSlot.FireBossHunterConsumed += FireBossHunterConsumed;
                IL.RoR2.EquipmentSlot.FireGummyClone += FireGummyClone;
                IL.RoR2.Items.MultiShopCardUtils.OnPurchase += MultiShopCardUtils_OnPurchase;
                //IL.EntityStates.GoldGat.GoldGatFire.FireBullet += GoldGat;
            };
        }

        #region Upgraded Effects

        //Fires a blackhole on the opoosite direction
        //Todo, maybe instead of two blackholes it fires a special blackhole that inflicts a debuff?
        private void FireBlackHole(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<ProjectileManager>(nameof(ProjectileManager.FireProjectile)));

            if (flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, 0);
                cursor.EmitDelegate<Action<EquipmentSlot, Ray>>((slot, ray) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GravSphere"), slot.transform.position, Util.QuaternionSafeLookRotation(-ray.direction), slot.gameObject, 0f, 0f, crit: false);
                    }
                });
                //since we managed to ILHook succesfully, we'll just add it to the blacklist to ensure the method doesnt get callede twice.
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Blackhole);
            }
        }

        //Todo: Xi Construct Mega Laser
        private bool FireBFG(On.RoR2.EquipmentSlot.orig_FireBfg orig, EquipmentSlot self)
        {
            var val = orig(self);
            if (BeatingEmbryoManager.Procs(self))
            {
                self.bfgChargeTimer /= 2;
                self.subcooldownTimer /= 2;
            }
            return val;
        }

        //Doubles lifetime
        private void FireDroneBackup(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(25f),
                x => x.MatchStloc(1));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, 1);
                cursor.EmitDelegate<Func<EquipmentSlot, float, float>>((slot, lifetime) =>
                {
                    return BeatingEmbryoManager.Procs(slot) ? lifetime * 2 : lifetime;
                });
                cursor.Emit(OpCodes.Stloc_1);
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.DroneBackup);
            }
        }

        //Increased meteors and blast radius
        private void FireMeteor(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate)),
                x => x.MatchCallOrCallvirt(typeof(GameObject), nameof(GameObject.GetComponent)));

            if(flag)
            {
                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<MeteorStormController,EquipmentSlot>>((controller, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        controller.blastRadius *= 2;
                        controller.waveCount = Mathf.RoundToInt(controller.waveCount * 1.5f);
                        controller.waveMinInterval /= 2;
                        controller.waveMaxInterval /= 1.5f;
                    }
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Meteor);
            }
        }

        //Adds a hidden buff which doubles crit damage
        private void FireCritOnUse(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EquipmentSlot>("get_" + nameof(EquipmentSlot.characterBody)),
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.FullCrit)),
                x => x.MatchLdcR4(8f));
            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((fullCritDuration, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        slot.characterBody.AddTimedBuff(LITContent.Buffs.bdHiddenCritDamage, fullCritDuration);
                    }
                    return fullCritDuration;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Meteor);
            }
        }

        //reduced boost cooldown, increased speed multiplier... might not be networked?
        private bool FireJetpack(On.RoR2.EquipmentSlot.orig_FireJetpack orig, EquipmentSlot self)
        {
            var result = orig(self);
            if(BeatingEmbryoManager.Procs(self))
            {
                JetpackController controller = JetpackController.FindJetpackController(self.gameObject);
                controller.boostCooldown /= 2;
                controller.boostSpeedMultiplier *= 2;
            }
            return result;
        }

        //Double Damage
        private void FireLightning(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<CharacterBody>("get_" + nameof(CharacterBody.damage)),
                x => x.MatchLdcR4(30),
                x => x.MatchMul());

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((damageMultiplier, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return damageMultiplier * 2f;
                    }
                    return damageMultiplier;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Lightning);
            }
        }

        //double initial healing fraction
        private void FirePassiveHealing(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchPop(),
                x => x.MatchBr(out _),
                x => x.MatchLdcR4(0.1f));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((fraction, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return fraction * 2;
                    }
                    return fraction;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.PassiveHealing);
            }
        }

        //Doubles the duration. FYI: the method "AddHelfireDuration" doesnt add, it overrides. thanks Hotpoo games.
        private void FireBurnNearby(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(12));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((duration, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return duration * 2;
                    }
                    return duration;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.BurnNearby);
            }
        }

        //Doubles the radius and duration of the reveal, might not be networked?
        private void FireScanner(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(Quaternion), "get_" + nameof(Quaternion.identity)),
                x => x.MatchCallOrCallvirt(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate)));

            if(flag)
            {
                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<GameObject, EquipmentSlot>>((scannerObj, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        var scanner = scannerObj.GetComponent<RoR2.ChestRevealer>();
                        scanner.revealDuration *= 2;
                        scanner.radius *= 2;
                    }
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Scanner);
            }
        }

        //Doubles radius and buff duration, might not be networked?
        private void FireCrippleWard(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate)));

            if(flag)
            {
                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<GameObject, EquipmentSlot>>((ward, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        var buffWard = ward.GetComponent<BuffWard>();
                        buffWard.radius *= 2;
                        buffWard.buffDuration *= 2;
                    }
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.CrippleWard);
            }
        }

        //Doubles distance and lifetime of the gateway, currently doesnt work.
        private void FireGateway(ILContext il)
        {
            /*var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(x => x.MatchLdcR4(1000f));
            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((maxDistance, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return maxDistance * 2;
                    }
                    return maxDistance;
                });
            }

            flag = cursor.TryGotoNext(MoveType.After, x => x.MatchDup(),
                x => x.MatchLdcR4(30f));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((lifetime, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return lifetime * 2;
                    }
                    return lifetime;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Gateway);
            }*/
        }


        //Doubled tonic buff. Todo: increased affliction chance by 10%.
        private void FireTonic(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld(typeof(EquipmentSlot), nameof(EquipmentSlot.tonicBuffDuration)));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((duration, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return duration * 2;
                    }
                    return duration;
                });
            }
        }

        //Idea from Gaforb
        //Blast attack for pushing enemmies back.
        private bool FireCleanse(EquipmentSlot slot)
        {
            BlastAttack attack = new BlastAttack
            {
                attacker = slot.characterBody.gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseForce = 10000,
                bonusForce = Vector3.up,
                canRejectForce = false,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Stun1s,
                falloffModel = BlastAttack.FalloffModel.None,
                inflictor = slot.gameObject,
                losType = BlastAttack.LoSType.None,
                position = slot.characterBody.footPosition,
                procCoefficient = 0,
                radius = slot.characterBody.bestFitRadius,
                baseDamage = slot.characterBody.damage,
                teamIndex = slot.characterBody.teamComponent.teamIndex
            };
            attack.Fire();
            return slot.FireCleanse();
        }

        //Duplicate the original pickup. said duplicate cant be recycled.
        //Doesnt work, idk why.
        private void FireRecycle(ILContext il)
        {
            /*var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdfld<GenericPickupController>(nameof(GenericPickupController.pickupIndex)));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldloc_0);
                cursor.EmitDelegate<Action<GenericPickupController, EquipmentSlot>>((pickupController, slot) =>
                {
                    if(BeatingEmbryoManager.Procs(slot))
                    {
                        var duplicate = GameObject.Instantiate(pickupController.gameObject);
                        duplicate.GetComponent<GenericPickupController>().NetworkRecycled = true;
                        NetworkServer.Spawn(duplicate);
                    }
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Recycle);
            }*/
        }

        //Doubled buff duration
        private void FireGainArmor(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(5f));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((duration, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return duration * 2;
                    }
                    return duration;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.GainArmor);
            }
        }

        //Doubled buff duration
        private void FireLifeStealOnHit(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(8f));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((duration, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return duration * 2;
                    }
                    return duration;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.LifestealOnHit);
            }
        }

        //Doubled buff duration
        private void FireTeamWarCry(ILContext il)
        {
            var cursor = new ILCursor(il);
            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.TeamWarCry)),
                x => x.MatchLdcR4(7));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((selfDuration, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return selfDuration * 2;
                    }
                    return selfDuration;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.TeamWarCry);
            }

            flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdlen(),
                x => x.MatchCallOrCallvirt<Component>(nameof(Component.GetComponent)),
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.TeamWarCry)),
                x => x.MatchLdcR4(7f));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((otherDuration, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return otherDuration * 2;
                    }
                    return otherDuration;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.TeamWarCry);
            }
        }

        //Doubled damage //Doesnt seem to work?
        private void FireDeathProjectile(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdloca(5),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EquipmentSlot>("get_" + nameof(EquipmentSlot.characterBody)),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_" + nameof(CharacterBody.damage)));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((damageStat, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return damageStat * 2;
                    }
                    return damageStat;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.DeathProjectile);
            }
        }

        //Not much we can do, so we're duplicating the damage.
        private void FireVendingMachine(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EquipmentSlot>("get_" + nameof(EquipmentSlot.characterBody)),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_" + nameof(CharacterBody.damage)));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((damageStat, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        return damageStat * 2;
                    }
                    return damageStat;
                });
                BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.VendingMachine);
            }
        }

        //Doubled drop
        private void FireBossHunter(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(x => x.MatchCallOrCallvirt<PickupDropletController>(nameof(PickupDropletController.CreatePickupDroplet)));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.Emit(OpCodes.Ldloc_2);
                cursor.Emit(OpCodes.Ldloc_3);
                cursor.EmitDelegate<Action<EquipmentSlot, DeathRewards, Vector3, Vector3>>((slot, rewards, vector, normalized) =>
                {
                    if(BeatingEmbryoManager.Procs(slot))
                    {
                        PickupDropletController.CreatePickupDroplet(rewards.bossDropTable.GenerateDrop(slot.rng), vector, normalized);
                    }
                });
                BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.BossHunter);
            }
        }

        //Extra random messages
        //Doesnt return properly, sometimes nothing happens
        private void FireBossHunterConsumed(ILContext il)
        {
            var cursor = new ILCursor(il);

            ILLabel label = null;
            cursor.GotoNext(x => x.MatchBrfalse(out label));

            var flag = cursor.TryGotoNext(x => x.MatchNewobj<Chat.BodyChatMessage>());

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<EquipmentSlot, bool>>(slot =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        Chat.SendBroadcastChat(new Chat.BodyChatMessage
                        {
                            bodyObject = slot.gameObject,
                            token = bossHunterOptions[slot.rng.RangeInt(0, bossHunterOptions.Length)]
                        });
                        slot.subcooldownTimer = 1;
                        return true;
                    }
                    return false;
                });
                cursor.Emit(OpCodes.Brfalse, label);
                BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.BossHunterConsumed);
            }
        }

        //Two gummies
        private void FireGummyClone(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCall<ProjectileManager>("get_" + nameof(ProjectileManager.instance)),
                x => x.MatchLdloc(4));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<FireProjectileInfo, EquipmentSlot, FireProjectileInfo>>((info, slot) =>
                {
                    if (BeatingEmbryoManager.Procs(slot))
                    {
                        ProjectileManager.instance.FireProjectile(info);
                    }
                    return info;
                });
                BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.GummyClone);
            }
        }

        //Increased gold gain
        private void MultiShopCardUtils_OnPurchase(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdloc(4),
                x => x.MatchLdloc(1),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_" + nameof(CharacterBody.mainHurtBox)),
                x => x.MatchStfld<Orb>(nameof(Orb.target)),
                x => x.MatchLdloc(4),
                x => x.MatchLdcR4(0.1f));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, CostTypeDef.PayCostContext, float>>((paybackFraction, context) =>
                {
                    if (context.activatorBody.TryGetComponent<ProcTrackerBeatingEmbryoBehaviour>(out var tracker))
                    {
                        return tracker.Procs() ? paybackFraction * 2 : paybackFraction;
                    }
                    return paybackFraction;
                });
                BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.MultiShopCard);
            }
        }

        //Increased damage and reduced cost
        private void GoldGat(ILContext il)
        {
            /*var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchStloc(2));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc_2);
                cursor.EmitDelegate<Func<GoldGatFire, int, int>>((state, goldCost) =>
                {
                    ProcTrackerBeatingEmbryoBehaviour tracker = null;
                    if (state.body?.TryGetComponent(out tracker) ?? false)
                    {
                        return tracker.Procs() ? Mathf.Max(1, Mathf.CeilToInt(goldCost / 2)) : goldCost;
                    };
                    return goldCost;
                });
                cursor.Emit(OpCodes.Stloc_2);
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.GoldGat);
            }

            flag = cursor.TryGotoNext(MoveType.After, p => p.MatchDup(),
                p => p.MatchLdarg(0),
                p => p.MatchLdfld<BaseGoldGatState>(nameof(BaseGoldGatState.body)),
                p => p.MatchCallOrCallvirt<CharacterBody>("get_" + nameof(CharacterBody.damage)));

            if(flag)
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<GoldGatFire, float, float>>((state, damage) =>
                {
                    ProcTrackerBeatingEmbryoBehaviour tracker = null;
                    if (state.body?.TryGetComponent(out tracker) ?? false)
                    {
                        return tracker.Procs() ? damage * 2 : damage;
                    }
                    return damage;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.GoldGat);
            }*/
        }

        #endregion

        public class ProcTrackerBeatingEmbryoBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => LITContent.Items.BeatingEmbryo;
            public bool HasProccedThisFrame { get; internal set; }
            private void FixedUpdate()
            {
                if (HasProccedThisFrame)
                    HasProccedThisFrame = false;
            }

            public bool Procs()
            {
                if (HasProccedThisFrame)
                {
#if DEBUG
                    LITLog.Info("Embryo already procced for " + body);
#endif
                    return true;
                }

                if(Util.CheckRoll(MSUtil.InverseHyperbolicScaling(20, 20, 100, stack), body.master))
                {
#if DEBUG
                    LITLog.Info("Embryo Proc for " + body);
#endif
                    HasProccedThisFrame = true;
                    return true;
                }
                HasProccedThisFrame = false;
                return false;
            }


            private static int GetCount(EquipmentSlot slot)
            {
                var inventory = slot.inventory;
                if (!inventory)
                    return 0;
                return inventory.GetItemCount(LITContent.Items.BeatingEmbryo);
            }
        }
    }

    public static class BeatingEmbryoManager
    {
        private static readonly HashSet<EquipmentDef> _blacklist = new HashSet<EquipmentDef>();
        private static readonly Dictionary<EquipmentDef, Func<EquipmentSlot, bool>> _equipToFunction = new Dictionary<EquipmentDef, Func<EquipmentSlot, bool>>();

        public static Func<EquipmentSlot, bool> GetFunc(EquipmentDef def)
        {
            if(_equipToFunction.TryGetValue(def, out var func))
            {
                return func;
            }
            return null;
        }

        public static void AddToBlackList(EquipmentDef equipmentDef)
        {
            _blacklist.Add(equipmentDef);
        }

        public static void AddEmbryoEffect(EquipmentDef equipmentDef, Func<EquipmentSlot, bool> equipmentEffect)
        {
            _equipToFunction.Add(equipmentDef, equipmentEffect);
        }

        [SystemInitializer(typeof(EquipmentCatalog), typeof(ItemCatalog))]
        private static void Init()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformUpgradedAction;
        }

        private static bool PerformUpgradedAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            //No embryo? dont do anything
            if (!self.TryGetComponent<BeatingEmbryo.ProcTrackerBeatingEmbryoBehaviour>(out var tracker))
                return orig(self, equipmentDef);

            //Is in the blacklist? dont do anything funky. Either equip cant support double trigger, or there's an IL/ON hook that checks for proc.
            if (_blacklist.Contains(equipmentDef))
                return orig(self, equipmentDef);

            //If def not in blacklist, check if we can call function override or call twice.
            if (!tracker.Procs())
                return orig(self, equipmentDef);

            //If the index has a funky function, use that instead of orig(self);
            if(_equipToFunction.TryGetValue(equipmentDef, out var function))
            {
                return function(self);
            }

            //If the index does not have a funky function, use this for loop for triggering the equipment twice.
            bool result = false;
            for(int i = 0; i < 2; i++)
            {
                result = orig(self, equipmentDef);
            }
            return result;
        }

        public static bool Procs(EquipmentSlot slot)
        {
            if (!slot.TryGetComponent<BeatingEmbryo.ProcTrackerBeatingEmbryoBehaviour>(out var tracker))
            {
                return false;
            }

            return tracker.Procs();
        }
    }
}