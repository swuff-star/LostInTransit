using MSU;
using RoR2;
using R2API;


namespace LostInTransit.Buffs
{
     public class ToxinCooldown : BuffBase
     {
         public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdToxinCooldown", LITBundle.Items);
     }
}