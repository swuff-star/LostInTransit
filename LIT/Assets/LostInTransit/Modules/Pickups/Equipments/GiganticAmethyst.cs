﻿using LostInTransit.Items;
using Moonstorm;
using RoR2;

namespace LostInTransit.Equipments
{
    public class GiganticAmethyst : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("GiganticAmethyst", LITBundle.Equips);

        public override bool FireAction(EquipmentSlot slot)
        {
            var sloc = slot.characterBody?.skillLocator;
            if ((bool)!sloc)
            {
                return false;
            }
            if(BeatingEmbryoManager.Procs(slot))
            {
                sloc.ApplyAmmoPack();
            }
            sloc.ApplyAmmoPack();
            Util.PlaySound("AmethystProc", slot.characterBody.gameObject); //Why the fuck is this called "AmethystProc", theres nothing to proc, wtf swuff
            return true;                                                        //you're proccing the amethyst what would you have called it?
        }                                                                       //"amethystfire"? you aren't FIRING anything.
    }                                                                           //"amethystuse" is dull.
                                                                                //n- "AmethystActivation", duh.
}                                                                               //long
