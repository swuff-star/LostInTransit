using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using R2API;
using RoR2.Items;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


namespace LostInTransit
{
    //Should probably move all of these to their respective item classes.
    public class LITHooks : MonoBehaviour
    {
        public static void Init()
        {
            On.RoR2.Inventory.CalculateEquipmentCooldownScale += (orig, self) =>
            {
                float num = orig.Invoke(self);
                num *= (1 - MSUtil.InverseHyperbolicScaling(Items.RapidMitosis.mitosisEquipCD, Items.RapidMitosis.mitosisEquipCD, 0.7f, self.GetItemCount(LITContent.Items.RapidMitosis)));
                return num;
            };

            On.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved += ProcMitosis;
            CharacterBody.onBodyStartGlobal += MarkHitList;
        }

        private static void ProcMitosis(On.RoR2.EquipmentSlot.orig_RpcOnClientEquipmentActivationRecieved orig, EquipmentSlot self)
        {
            orig(self);

            if (self.hasAuthority && self.inventory)
            {
                int mitosisCount = self.inventory.GetItemCount(LITContent.Items.RapidMitosis);
                if (mitosisCount > 0)
                {
                    self.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdMitosisBuff.buffIndex, Items.RapidMitosis.mitosisDur);
                }    
            }
        }

        private static void MarkHitList(CharacterBody body)
        {
            List<CharacterMaster> CharMasters(bool playersOnly = false)
            {
                return CharacterMaster.readOnlyInstancesList.Where(x => x.hasBody && x.GetBody().healthComponent.alive && (x.GetBody().teamComponent.teamIndex != body.teamComponent.teamIndex)).ToList();
            }

            int hitListCount = 0;

            foreach (CharacterMaster chrm in CharMasters())
            {
                hitListCount += chrm?.inventory?.GetItemCount(LITContent.Items.HitList) ?? 0;
            }

            //Debug.Log("Hit List Count: " + hitListCount);
            //Debug.Log("Team: " + body.teamComponent.teamIndex);

            if (hitListCount == 0) return;

            //Debug.Log("Rolling a... " + (Items.HitList.markChance + ((Items.HitList.markChance / 2) * (hitListCount - 1))) + " ... % chance");

            if (Util.CheckRoll(Items.HitList.markChance + ((Items.HitList.markChance / 2) * (hitListCount - 1)), body.master))
            {
                //Debug.Log("Marking a " + body.baseNameToken + "!");
                body.SetBuffCount(LITContent.Buffs.bdHitListMarked.buffIndex, 1);
            }
            else
            {
                //Debug.Log("Mark failed!");
            }    
        }
    }
}
