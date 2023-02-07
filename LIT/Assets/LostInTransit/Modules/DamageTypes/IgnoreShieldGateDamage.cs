using Moonstorm;
using static R2API.DamageAPI;

namespace LostInTransit.DamageTypes
{
    public sealed class IgnoreShieldGateDamage : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get => ignoreShieldGateDamage; protected set => ignoreShieldGateDamage = value; }

        public static ModdedDamageType ignoreShieldGateDamage;

        public override void Delegates()
        {

        }
    }
}
