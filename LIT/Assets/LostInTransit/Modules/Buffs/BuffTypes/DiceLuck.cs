using Moonstorm;
using RoR2;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    [DisabledContent]
    public class DiceLuck : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("DiceLuck");

        //Todo: Have this as a hook on CharacterMaster.OnInventoryChanged (otherwise the "luck" stat gets rewritten every time the inventory changes)
        public class DiceLuckBehavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.DiceLuck;

            private CharacterMaster master;
            public void Start()
            {
                master = body.master;
                if(master)
                {
                    master.luck += Items.BlessedDice.luckAmount;
                    
                }
            }
            public void OnDestroy()
            {
                if(master)
                {
                    master.luck -= Items.BlessedDice.luckAmount;
                }
            }
        }
    }
}