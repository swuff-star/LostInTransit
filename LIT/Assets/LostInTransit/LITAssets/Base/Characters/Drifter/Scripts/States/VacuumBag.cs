using EntityStates;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

namespace EntityStates.Drifter
{
    public class VacuumBag : BaseSkillState
    {
        public static string muzzle;

        public static float baseMinDuration;
        public static float baseMaxDuration;

        public static float fieldOfView;
        public static float backupDistance;
        public static float maxDistance;
        public static float idealDistanceToPlaceTargets;
        public static float liftVelocity;
        public static float dmgCoefficient;

        public static AnimationCurve shoveSuitabilityCurve;

        public static float baseSuckFrequency;
        private float suckFrequency;

        public static GameObject suckFX;

        private float minDuration;
        private float maxDuration;
        private float suckTimer;

        public override void OnEnter()
        {
            base.OnEnter();
            minDuration = baseMinDuration / attackSpeedStat;
            maxDuration = baseMaxDuration / attackSpeedStat;
            suckFrequency = baseSuckFrequency / attackSpeedStat;
            characterBody.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChild("SuckFX").gameObject.SetActive(true);

            Suck();
            characterBody.SetAimTimer(suckFrequency);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((fixedAge >= minDuration && !inputBank.skill3.down) || (fixedAge >= maxDuration))
                ExitAttack();

            suckTimer += Time.fixedDeltaTime;

            if (suckTimer >= suckFrequency)
            {
                suckTimer = 0;
                Debug.Log("suck timer reset - current time : " + fixedAge);
                Suck();
            }
        }

        public void Suck()
        {
            Debug.Log("sucking");
            Ray aimRay = GetAimRay();
            aimRay.origin -= aimRay.direction * backupDistance;
            if (NetworkServer.active)
            {
                if (suckFX != null)
                {
                    //EffectManager.SimpleMuzzleFlash(suckFX, gameObject, muzzle, true);
                }

                characterBody.SetAimTimer(suckFrequency);

                BullseyeSearch bullseyeSearch = new BullseyeSearch();
                bullseyeSearch.teamMaskFilter = TeamMask.all;
                bullseyeSearch.maxAngleFilter = fieldOfView * 0.5f;
                bullseyeSearch.maxDistanceFilter = maxDistance;
                bullseyeSearch.searchOrigin = aimRay.origin;
                bullseyeSearch.searchDirection = aimRay.direction;
                bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
                bullseyeSearch.filterByLoS = true;
                bullseyeSearch.RefreshCandidates();
                bullseyeSearch.FilterOutGameObject(gameObject);
                IEnumerable<HurtBox> hurtboxEnum = bullseyeSearch.GetResults().Where(new System.Func<HurtBox, bool>(Util.IsValid)).Distinct(default(HurtBox.EntityEqualityComparer));
                TeamIndex team = GetTeam();

                foreach (HurtBox hurtBox in hurtboxEnum)
                {
                    if (FriendlyFireManager.ShouldSplashHitProceed(hurtBox.healthComponent, team))
                    {
                        Vector3 vector = hurtBox.transform.position - aimRay.origin;
                        float magnitude = vector.magnitude;
                        float magnitude2 = new Vector2(vector.x, vector.z).magnitude;
                        Vector3 vector2 = vector / magnitude;
                        float mass = 1f;
                        CharacterBody body = hurtBox.healthComponent.body;
                        if (body.characterMotor)
                        {
                            mass = body.characterMotor.mass;
                        }
                        else if (hurtBox.healthComponent.GetComponent<Rigidbody>())
                        {
                            mass = rigidbody.mass;
                        }
                        float mass2 = shoveSuitabilityCurve.Evaluate(mass);
                        float acceleration = body.acceleration;
                        Vector3 a = vector2;
                        float d = Trajectory.CalculateInitialYSpeedForHeight(Mathf.Abs(idealDistanceToPlaceTargets - magnitude)) * Mathf.Sign(idealDistanceToPlaceTargets - magnitude);
                        a *= d;
                        a.y = liftVelocity;
                        if (body.isFlying)
                            a.y *= -10f;
                        DamageInfo damageInfo = new DamageInfo
                        {
                            attacker = gameObject,
                            damage = damageStat * dmgCoefficient,
                            position = hurtBox.transform.position,
                            procCoefficient = 0,
                        };
                        hurtBox.healthComponent.TakeDamageForce(a * (mass * mass2), true, true);
                        hurtBox.healthComponent.TakeDamage(new DamageInfo
                        {
                            attacker = gameObject,
                            damage = damageStat * dmgCoefficient,
                            position = hurtBox.transform.position,
                            procCoefficient = 0
                        });
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, hurtBox.healthComponent.gameObject);
                    }
                }
            }
            /*if (isAuthority && characterBody && characterBody.characterMotor)
            {
                float height = characterBody.characterMotor.isGrounded ? groundKnockbackDistance : airKnockbackDistance;
                float num3 = characterBody.characterMotor ? characterBody.characterMotor.mass : 1f;
                float acceleration2 = characterBody.acceleration;
                float num4 = Trajectory.CalculateInitialYSpeedForHeight(height, -acceleration2);
                characterBody.characterMotor.ApplyForce(-num4 * num3 * aimRay.direction, false, false);
            }*/
        }

        public void ExitAttack()
        {
            characterBody.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChild("SuckFX").gameObject.SetActive(false);
            Suffocate nextState = new Suffocate();
            outer.SetNextState(nextState);
        }
    }
}
