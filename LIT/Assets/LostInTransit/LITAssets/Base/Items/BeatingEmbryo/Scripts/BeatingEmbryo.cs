using Mono.Cecil.Cil;
using MonoMod.Cil;
using MSU;
using R2API;
using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using UnityEngine;
using EntityStates.GoldGat;
using UnityEngine.Networking;
using EntityStates.QuestVolatileBattery;
using RoR2.ContentManagement;
using System.Collections;

namespace LostInTransit.Items
{
    /*
     * The following equipments dont get any special methods/hooks because using the equipment memthod twice yeilds the "Improved" result.
     * 
     * FireCommandMissile
     * FireFruit
     * FireSaw
     * FireMolotov
     * 
     * The following equipments dont have any special features ddue to being difficult to implement.
     * AffixBlighted
     * AffixFrenzied
     * AffixVolatile
     * 
     * Other LIT Equipment have their embryo interaction codeed in their equipment class.
     */

    public sealed class BeatingEmbryo : LITItem, IContentPackModifier
    {
        private const string VANILLA_BBOSSHUNTER_CONSUMED_TOKEN = "EQUIPMENT_BOSSHUNTERCONSUMED_CHAT";
        public string[] bossHunterOptions = new string[] { VANILLA_BBOSSHUNTER_CONSUMED_TOKEN, "LIT_EQUIPMENT_BOSSHUNTERCONSUMED_CHAT_1", "LIT_EQUIPMENT_BOSSHUNTERCONSUMED_CHAT_2", "LIT_EQUIPMENT_BOSSHUNTERCONSUMED_CHAT_3", "LIT_EQUIPMENT_BOSSHUNTERCONSUMED_CHAT_4" };

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        private static GameObject _bfg10kController;
        private AssetCollection _assetCollection;
        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += HandleHiddenCritDamage;

            //Lines with //* are methods that are either unfinished, or have ToDo's
            //commmented out hooks are methods that are almost finished, but produce invalid IL
            RoR2Application.onLoad += () =>
            {
                BeatingEmbryoManager.AddEmbryoEffect(RoR2Content.Equipment.BFG, FireBFG);
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
                IL.RoR2.EquipmentSlot.FireGateway += FireGateway;
                IL.RoR2.EquipmentSlot.FireTonic += FireTonic;
                BeatingEmbryoManager.AddEmbryoEffect(RoR2Content.Equipment.Cleanse, FireCleanse);
                IL.RoR2.EquipmentSlot.FireFireBallDash += FireFireBallDash;
                IL.RoR2.EquipmentSlot.FireRecycle += FireRecycle;
                IL.RoR2.EquipmentSlot.FireGainArmor += FireGainArmor;
                IL.RoR2.EquipmentSlot.FireLifeStealOnHit += FireLifeStealOnHit;
                IL.RoR2.EquipmentSlot.FireTeamWarCry += FireTeamWarCry;
                IL.RoR2.EquipmentSlot.FireDeathProjectile += FireDeathProjectile;
                IL.RoR2.EquipmentSlot.FireVendingMachine += FireVendingMachine;
                IL.RoR2.EquipmentSlot.FireBossHunter += FireBossHunter;
                IL.RoR2.EquipmentSlot.FireBossHunterConsumed += FireBossHunterConsumed;
                IL.RoR2.EquipmentSlot.FireGummyClone += FireGummyClone;
                IL.RoR2.Items.MultiShopCardUtils.OnPurchase += MultiShopCardUtils_OnPurchase;
                IL.EntityStates.GoldGat.GoldGatFire.FireBullet += GoldGat;
                IL.EntityStates.QuestVolatileBattery.CountDown.Detonate += QuestVolatileBattery; ;
                BeatingEmbryoManager.AddToBlackList(LITContent.Equipments.AffixVolatile);
                BeatingEmbryoManager.AddToBlackList(LITContent.Equipments.AffixFrenzied);
                BeatingEmbryoManager.AddToBlackList(LITContent.Equipments.AffixBlighted);
                BeatingEmbryoManager.AddToBlackList(LITContent.Equipments.GiganticAmethyst);
                BeatingEmbryoManager.AddToBlackList(LITContent.Equipments.Prescriptions);
                BeatingEmbryoManager.AddToBlackList(LITContent.Equipments.Thqwib);
                BeatingEmbryoManager.AddToBlackList(LITContent.Equipments.UnstableWatch);
            };
        }

        private void HandleHiddenCritDamage(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.GetBuffCount(LITContent.Buffs.bdHiddenCritDamage) > 0)
            {
                args.critDamageMultAdd += 1;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "BeatingEmbryo" - Items
             * BuffDef - "bdHiddenCritDamage" - Items
             * GameObject - "BFG10KBodyAttachment" - Items
             */

            var request = LITAssets.LoadAssetAsync<AssetCollection>("acBeatingEmbryo", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;

            _itemDef = _assetCollection.FindAsset<ItemDef>("BeatingEmbryo");
            _bfg10kController = _assetCollection.FindAsset<GameObject>("BFG10KBodyAttachment");
        }

        #region Upgraded Effects
        //Fires a blackhole on the oposite direction
        //Todo, maybe instead of two blackholes it fires a special blackhole that inflicts a debuff?
        private void FireBlackHole(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<ProjectileManager>(nameof(ProjectileManager.FireProjectile)));

            if (!flag)
            {
                LogEmbryoHookFailed("Fire secondary black hole.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, 0);
            cursor.EmitDelegate<Action<EquipmentSlot, Ray>>((slot, ray) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    ProjectileManager.instance.FireProjectile(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/GravSphere"), slot.transform.position, Util.QuaternionSafeLookRotation(-ray.direction), slot.gameObject, 0f, 0f, crit: false);
                }
            });
            //since we managed to ILHook succesfully, we'll just add it to the blacklist to ensure the method doesnt get callede twice.
            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Blackhole);

        }

        //This is absolutely unecesary but its fucking epic. BFG10K death laser.
        //Note: might not be networked, idk
        private bool FireBFG(EquipmentSlot self)
        {
            ProcTrackerBeatingEmbryoBehaviour tracker = null;
            if (self.TryGetComponent(out tracker))
            {
                if (tracker.BFG10kAttachment)
                {
                    return false;
                }
                tracker.SpawnBFG10KAttachmentAndFire();
                return true;
            }
            return self.FireBfg();
        }

        //Doubles lifetime
        private void FireDroneBackup(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(25f),
                x => x.MatchStloc(1));

            if (!flag)
            {
                LogEmbryoHookFailed("Increase DroneBackup lifetime.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, 1);
            cursor.EmitDelegate<Func<EquipmentSlot, float, float>>((slot, lifetime) =>
            {
                return BeatingEmbryoManager.BeatingEmbryoProcs(slot) ? lifetime * 2 : lifetime;
            });
            cursor.Emit(OpCodes.Stloc_1);

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.DroneBackup);
        }

        //Increased meteors and blast radius
        private void FireMeteor(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate)),
                x => x.MatchCallOrCallvirt(typeof(GameObject), nameof(GameObject.GetComponent)));

            if (!flag)
            {
                LogEmbryoHookFailed("Increase meteor radius, count, and frequency.");
                return;
            }

            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<MeteorStormController, EquipmentSlot>>((controller, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    controller.blastRadius *= 2;
                    controller.waveCount = Mathf.RoundToInt(controller.waveCount * 1.5f);
                    controller.waveMinInterval /= 2;
                    controller.waveMaxInterval /= 1.5f;
                }
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Meteor);
        }

        //Adds a hidden buff which doubles crit damage
        private void FireCritOnUse(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EquipmentSlot>("get_" + nameof(EquipmentSlot.characterBody)),
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.FullCrit)),
                x => x.MatchLdcR4(8f));

            if (!flag)
            {
                LogEmbryoHookFailed("CritOnUse 100% Crit Damage increase.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((fullCritDuration, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    slot.characterBody.AddTimedBuff(LITContent.Buffs.bdHiddenCritDamage, fullCritDuration);
                }
                return fullCritDuration;
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Meteor);
        }

        //reduced boost cooldown, increased speed multiplier... might not be networked?
        private bool FireJetpack(On.RoR2.EquipmentSlot.orig_FireJetpack orig, EquipmentSlot self)
        {
            var result = orig(self);
            if (BeatingEmbryoManager.BeatingEmbryoProcs(self))
            {
                JetpackController controller = JetpackController.FindJetpackController(self.gameObject);
                controller.boostCooldown /= 1.5f;
                controller.boostSpeedMultiplier *= 1.5f;
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

            if (!flag)
            {
                LogEmbryoHookFailed("Lightning damage increase.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((damageMultiplier, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    return damageMultiplier * 2f;
                }
                return damageMultiplier;
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Lightning);
        }

        //double initial healing fraction
        private void FirePassiveHealing(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchPop(),
                x => x.MatchBr(out _),
                x => x.MatchLdcR4(0.1f));

            if (!flag)
            {
                LogEmbryoHookFailed("Double PassiveHealing initial fraction.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((fraction, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    return fraction * 2;
                }
                return fraction;
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.PassiveHealing);

        }

        //Doubles the duration. FYI: the method "AddHelfireDuration" doesnt add, it overrides. thanks Hotpoo games.
        private void FireBurnNearby(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(12));

            if (!flag)
            {
                LogEmbryoHookFailed("Double BurnNearby duration.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((duration, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    return duration * 2;
                }
                return duration;
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.BurnNearby);
        }

        //Doubles the radius and duration of the reveal, might not be networked?
        private void FireScanner(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(Quaternion), "get_" + nameof(Quaternion.identity)),
                x => x.MatchCallOrCallvirt(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate)));

            if (!flag)
            {
                LogEmbryoHookFailed("Double scanner's radius and reveal duration.");
                return;
            }

            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<GameObject, EquipmentSlot>>((scannerObj, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    var scanner = scannerObj.GetComponent<RoR2.ChestRevealer>();
                    scanner.revealDuration *= 2;
                    scanner.radius *= 2;
                }
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Scanner);
        }

        //Doubles radius and buff duration, might not be networked?
        private void FireCrippleWard(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate)));

            if (!flag)
            {
                LogEmbryoHookFailed("Double CrippleWard's radius and buff duration.");
                return;
            }

            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<GameObject, EquipmentSlot>>((ward, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    var buffWard = ward.GetComponent<BuffWard>();
                    buffWard.radius *= 2;
                    buffWard.buffDuration *= 2;
                }
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.CrippleWard);
        }

        //Doubles distance and lifetime of the gateway
        private void FireGateway(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(1000f));
            if (!flag)
            {
                LogEmbryoHookFailed("Double Gateway max distance.", "Distance will not be changed.");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((maxDistance, slot) =>
                {
                    if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                    {
                        return maxDistance * 2;
                    }
                    return maxDistance;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Gateway);
            }


            bool flag2 = flag;
            flag = cursor.TryGotoNext(MoveType.After, x => x.MatchDup(),
                x => x.MatchLdcR4(30f));

            if (!flag)
            {
                LogEmbryoHookFailed("Double Gateway Lifetime.", "Lifetime will not be changed");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((lifetime, slot) =>
                {
                    if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                    {
                        return lifetime * 2;
                    }
                    return lifetime;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Gateway);
            }

            if (!flag && !flag2)
            {
                LITLog.Fatal("ILHook for Gateway failed, equipment will activate twice.");
            }
        }

        //Doubled tonic buff. Doubled affliction chance
        private void FireTonic(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld(typeof(EquipmentSlot), nameof(EquipmentSlot.tonicBuffDuration)));

            if (!flag)
            {
                LogEmbryoHookFailed("Double tonic buff duration", "Duration will not be changed");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((duration, slot) =>
                {
                    if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                    {
                        return duration * 2;
                    }
                    return duration;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Tonic);
            }

            bool flag2 = flag;
            flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(80f));

            if (!flag)
            {
                LogEmbryoHookFailed("Divide chance for no affliction", "Affliction chance will not be changed");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((chanceForNoAffliction, slot) =>
                {
                    if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                    {
                        return chanceForNoAffliction / 2;
                    }
                    return chanceForNoAffliction;
                });
                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Tonic);
            }

            if (!flag && !flag2)
            {
                LITLog.Fatal("ILHook for Tonic failed, equipment will activate twice.");
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

        //Increased target speeed, increased damage coefficient.
        private void FireFireBallDash(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(x => x.MatchStloc(1));

            if (!flag)
            {
                LogEmbryoHookFailed("Increase FireBallDash speed and damage coefficient by 50%");
                return;
            }

            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<GameObject, EquipmentSlot>>((vehicleObject, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot) && vehicleObject.TryGetComponent<FireballVehicle>(out var fireballVehicle))
                {
                    fireballVehicle.targetSpeed *= 1.5f;
                    fireballVehicle.overlapDamageCoefficient *= 1.5f;
                    fireballVehicle.blastDamageCoefficient *= 1.5f;
                }
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.FireBallDash);
        }

        //Duplicate the original pickup. said duplicate cant be recycled.
        private void FireRecycle(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdfld<EquipmentSlot.UserTargetInfo>(nameof(EquipmentSlot.UserTargetInfo.pickupController)),
                x => x.MatchStloc(0));

            if (!flag)
            {
                LogEmbryoHookFailed("Create Recycler Duplicate Pickup");
                return;
            }

            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<GenericPickupController, EquipmentSlot>>((pickupController, slot) =>
            {
                if (pickupController && !pickupController.Recycled && BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    GenericPickupController.CreatePickupInfo pickupInfo = new GenericPickupController.CreatePickupInfo
                    {
                        pickupIndex = pickupController.NetworkpickupIndex,
                        position = pickupController.transform.position + Vector3.up * 2,
                        rotation = pickupController.transform.rotation
                    };
                    GenericPickupController duplicate = GenericPickupController.CreatePickup(pickupInfo);
                    duplicate.NetworkRecycled = true;
                }
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.Recycle);
        }

        //Doubled buff duration
        private void FireGainArmor(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(5f));

            if (!flag)
            {
                LogEmbryoHookFailed("Double GainArmor Duration");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((duration, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    return duration * 2;
                }
                return duration;
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.GainArmor);
        }

        //Doubled buff duration
        private void FireLifeStealOnHit(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdcR4(8f));

            if (flag)
            {
                LogEmbryoHookFailed("Double LifeSteal Duration");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((duration, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    return duration * 2;
                }
                return duration;
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.LifestealOnHit);
        }

        //Doubled buff duration
        private void FireTeamWarCry(ILContext il)
        {
            var cursor = new ILCursor(il);
            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.TeamWarCry)),
                x => x.MatchLdcR4(7));

            if (!flag)
            {
                LogEmbryoHookFailed("Double TeamWarCry's caster duration");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((selfDuration, slot) =>
                {
                    if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                    {
                        return selfDuration * 2;
                    }
                    return selfDuration;
                });

                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.TeamWarCry);
            }

            bool flag2 = flag;
            flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdlen(),
                x => x.MatchCallOrCallvirt<Component>(nameof(Component.GetComponent)),
                x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.TeamWarCry)),
                x => x.MatchLdcR4(7f));

            if (!flag)
            {
                LogEmbryoHookFailed("Double TeamWarCry team duration");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((otherDuration, slot) =>
                {
                    if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                    {
                        return otherDuration * 2;
                    }
                    return otherDuration;
                });

                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.TeamWarCry);
            }

            if (!flag && !flag2)
            {
                LITLog.Fatal("ILHook for TeamWarCry failed, equipment will activate twice.");
            }
        }

        //Doubled damage //Doesnt seem to work?
        private void FireDeathProjectile(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<ProjectileManager>("get_" + (nameof(ProjectileManager.instance))),
                x => x.MatchLdloc(4));

            if (!flag)
            {
                LogEmbryoHookFailed("Fire second DeathProjectile.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<FireProjectileInfo, EquipmentSlot, FireProjectileInfo>>((projectileInfo, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    ProjectileManager.instance.FireProjectile(projectileInfo);
                    return projectileInfo;
                }
                return projectileInfo;
            });

            BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.DeathProjectile);
        }

        //Not much we can do, so we're duplicating the damage.
        private void FireVendingMachine(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<EquipmentSlot>("get_" + nameof(EquipmentSlot.characterBody)),
                x => x.MatchCallOrCallvirt<CharacterBody>("get_" + nameof(CharacterBody.damage)));

            if (flag)
            {
                LogEmbryoHookFailed("Double VendingMachine Damage");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, EquipmentSlot, float>>((damageStat, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    return damageStat * 2;
                }
                return damageStat;
            });

            BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.VendingMachine);
        }

        //Doubled drop
        private void FireBossHunter(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(x => x.MatchCallOrCallvirt<PickupDropletController>(nameof(PickupDropletController.CreatePickupDroplet)));

            if (flag)
            {
                LogEmbryoHookFailed("Drop secondary BossHunter pickup.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.Emit(OpCodes.Ldloc_2);
            cursor.Emit(OpCodes.Ldloc_3);
            cursor.EmitDelegate<Action<EquipmentSlot, DeathRewards, Vector3, Vector3>>((slot, rewards, vector, normalized) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    PickupDropletController.CreatePickupDroplet(rewards.bossDropTable.GenerateDrop(slot.rng), vector, normalized);
                }
            });

            BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.BossHunter);
        }

        //Extra random messages
        //Doesnt return properly, sometimes nothing happens
        private void FireBossHunterConsumed(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool flag = cursor.TryGotoNext(MoveType.After, x => x.MatchLdstr(VANILLA_BBOSSHUNTER_CONSUMED_TOKEN));

            if (!flag)
            {
                LogEmbryoHookFailed("BossHunterConsumed additional messages.");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<string, EquipmentSlot, string>>((originalToken, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    return slot.rng.NextElementUniform(bossHunterOptions);
                }
                return originalToken;
            });

            BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.BossHunterConsumed);
        }

        //Two gummies
        private void FireGummyClone(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCall<ProjectileManager>("get_" + nameof(ProjectileManager.instance)),
                x => x.MatchLdloc(4));

            if (!flag)
            {
                LogEmbryoHookFailed("Fire secondary GummyClone");
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<FireProjectileInfo, EquipmentSlot, FireProjectileInfo>>((info, slot) =>
            {
                if (BeatingEmbryoManager.BeatingEmbryoProcs(slot))
                {
                    ProjectileManager.instance.FireProjectile(info);
                }
                return info;
            });

            BeatingEmbryoManager.AddToBlackList(DLC1Content.Equipment.GummyClone);
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

            if (!flag)
            {
                LogEmbryoHookFailed("Increase MultiShopCard cashback");
                return;
            }

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

        //Increased damage and reduced cost
        private void GoldGat(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchStloc(2));

            if (!flag)
            {
                LogEmbryoHookFailed("Reduce GoldGat gold cost", "Gold cost will not be modified.");
            }
            else
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

            bool flag2 = flag;
            flag = cursor.TryGotoNext(MoveType.After, p => p.MatchDup(),
                p => p.MatchLdarg(0),
                p => p.MatchLdfld<BaseGoldGatState>(nameof(BaseGoldGatState.body)),
                p => p.MatchCallOrCallvirt<CharacterBody>("get_" + nameof(CharacterBody.damage)));

            if (!flag)
            {
                LogEmbryoHookFailed("Increase GoldGat damage", "Damage will not be modified");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, GoldGatFire, float>>((damage, state) =>
                {
                    ProcTrackerBeatingEmbryoBehaviour tracker = null;
                    if (state.body?.TryGetComponent(out tracker) ?? false)
                    {
                        return tracker.Procs() ? damage * 2 : damage;
                    }
                    return damage;
                });

                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.GoldGat);
            }

            if (!flag && !flag2)
            {
                LITLog.Fatal("ILHook for GoldGat failed, equipment will activate twice.");
            }
        }

        //Increased damage coefficient and radius.
        private void QuestVolatileBattery(ILContext il)
        {
            var cursor = new ILCursor(il);

            var flag = cursor.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<HealthComponent>("get_" + nameof(HealthComponent.fullCombinedHealth)),
                x => x.MatchLdcR4(3));

            if (!flag)
            {
                LogEmbryoHookFailed("Increase QuestVolatileBattery Damage", "Damage will not be modified");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, CountDown, float>>((damage, state) =>
                {
                    ProcTrackerBeatingEmbryoBehaviour tracker = null;
                    if (state?.networkedBodyAttachment.attachedBody.TryGetComponent(out tracker) ?? false)
                    {
                        return tracker.Procs() ? damage * 10f : damage;
                    }
                    return damage;
                });

                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.QuestVolatileBattery);
            }

            bool flag2 = flag;
            flag = cursor.TryGotoNext(MoveType.After, x => x.MatchDup(),
                x => x.MatchLdsfld<CountDown>(nameof(CountDown.explosionRadius)));

            if (!flag)
            {
                LogEmbryoHookFailed("Increse QuestVolatileBbattery Radius", "Radius will not be modified");
            }
            else
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, CountDown, float>>((radius, state) =>
                {
                    ProcTrackerBeatingEmbryoBehaviour tracker = null;
                    if (state?.networkedBodyAttachment.attachedBody.TryGetComponent(out tracker) ?? false)
                    {
                        return tracker.Procs() ? radius * 5f : radius;
                    }
                    return radius;
                });

                BeatingEmbryoManager.AddToBlackList(RoR2Content.Equipment.QuestVolatileBattery);
            }

            if (!flag && !flag2)
            {
                LITLog.Fatal("ILHook for QuestVolatileBbattery failed. No changes where made.");
            }
        }
        #endregion


        private void LogEmbryoHookFailed(string message, string postMessage = null)
        {
            string activateTwice = "Equipment will activate twice instead.";
            LITLog.Fatal($"Failed to implement Embryo ILHook!: {message}. {postMessage ?? activateTwice}");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class ProcTrackerBeatingEmbryoBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            public static ItemDef GetItemDef() => LITContent.Items.BeatingEmbryo;
            public bool? HasProccedThisFrame { get; internal set; }
            public NetworkedBodyAttachment BFG10kAttachment { get; private set; }

            public void SpawnBFG10KAttachmentAndFire()
            {
                BFG10kAttachment = Instantiate(_bfg10kController).GetComponent<NetworkedBodyAttachment>();
                BFG10kAttachment.AttachToGameObjectAndSpawn(body.gameObject);
                BFG10kAttachment.GetComponent<EntityStateMachine>().SetNextState(new EntityStates.BFG.ChargeBFG10KLaser());
            }
            private void FixedUpdate()
            {
                if (HasProccedThisFrame.HasValue)
                    HasProccedThisFrame = null;
            }

            public bool Procs()
            {
                if (HasProccedThisFrame.HasValue)
                {
                    return HasProccedThisFrame.Value;
                }

                if (Util.CheckRoll(MSUtil.InverseHyperbolicScaling(20, 20, 100, stack), body.master))
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
        }
    }

    /// <summary>
    /// A static class for managing the doubled effect of BeatingEmbryo
    /// </summary>
    public static class BeatingEmbryoManager
    {
        public const string EMBRYO_EFFECT_DESC = "LIT_EMBRYO_EFFECT_DESC";
        public const string EMBRYO_TOKEN_SUFFIX = "_EMBRYO";
        private static readonly HashSet<EquipmentDef> _blacklist = new HashSet<EquipmentDef>();
        private static readonly Dictionary<EquipmentDef, Func<EquipmentSlot, bool>> _equipToFunction = new Dictionary<EquipmentDef, Func<EquipmentSlot, bool>>();
        private static ItemDef embryoDef;

        /// <summary>
        /// returns the "Improved" version of an equipment's activation using an On-Hook.
        /// </summary>
        /// <param name="def">The def to use</param>
        /// <returns>The improved equipment action</returns>
        public static Func<EquipmentSlot, bool> GetFunc(EquipmentDef def)
        {
            if (_equipToFunction.TryGetValue(def, out var func))
            {
                return func;
            }
            return null;
        }

        /// <summary>
        /// Makes this equipment unable to be procced automatically by the manager's initial EquipmentSlot.PerformEquipmentAction hook
        /// <br>This should be called if the equipment youre implementing already handles the beating embryo interaction,  or if you desire for beating embryo to ignore this equipment</br>
        /// </summary>
        /// <param name="equipmentDef">The equipment to add to the blacklist</param>
        public static void AddToBlackList(EquipmentDef equipmentDef)
        {
            if (equipmentDef)
                _blacklist.Add(equipmentDef);
        }

        /// <summary>
        /// Adds a new Embryo effect in the form of an override to the existing equipment's Activation effect
        /// </summary>
        /// <param name="equipmentDef">The equipmentDef</param>
        /// <param name="equipmentEffect">The replacement Activation effect for this equipment</param>
        public static void AddEmbryoEffect(EquipmentDef equipmentDef, Func<EquipmentSlot, bool> equipmentEffect)
        {
            _equipToFunction.Add(equipmentDef, equipmentEffect);
        }

        [SystemInitializer(typeof(EquipmentCatalog), typeof(ItemCatalog))]
        private static void Init()
        {
            embryoDef = LITAssets.LoadAsset<ItemDef>("BeatingEmbryo", LITBundle.Items);
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformUpgradedAction;
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
        }

        //Returns an embryo desc
        private static string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string TOKEN)
        {
            var val = orig(self, TOKEN);
            if (embryoDef.itemIndex != ItemIndex.None)
            {
                var constructed = $"{TOKEN}{EMBRYO_TOKEN_SUFFIX}";
                if (self.stringsByToken.ContainsKey(constructed))
                {
                    return Language.GetStringFormatted(EMBRYO_EFFECT_DESC, val, Language.GetString(constructed));
                }
            }
            return val;
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
            if (_equipToFunction.TryGetValue(equipmentDef, out var function))
            {
                return function(self);
            }

            //If the index does not have a funky function, use this for loop for triggering the equipment twice.
            bool result = false;
            for (int i = 0; i < 2; i++)
            {
                result = orig(self, equipmentDef);
            }
            return result;
        }

        /// <summary>
        /// Checks if BeatingEmbryo has procced this frame.
        /// </summary>
        public static bool BeatingEmbryoProcs(this EquipmentSlot slot)
        {
            if (!slot.TryGetComponent<BeatingEmbryo.ProcTrackerBeatingEmbryoBehaviour>(out var tracker))
            {
                return false;
            }

            return tracker.Procs();
        }
    }
   
}