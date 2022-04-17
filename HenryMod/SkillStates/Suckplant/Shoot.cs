using EntityStates;
using RoR2;
using SuckplantMod.Modules.Components;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SuckplantMod.SkillStates
{
    public class Shoot : BaseSkillState
    {
        public static float damageCoefficient = 10f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 1f;
        public static float force = 800f;
        public static float recoil = 10f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = Addressables.LoadAssetAsync<UnityEngine.GameObject>(key: "RoR2/Base/Captain/TracerCaptainShotgun.prefab").WaitForCompletion();
        public Vector3 SoakVFX;
        private Quaternion VFXQuat;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;
        private DamageEaten damageEaten;
        public static Transform SoakHitbox;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = Shoot.baseDuration / this.attackSpeedStat;
            this.fireTime = 1f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.muzzleString = "Muzzle";
            Chat.AddMessage("FiredShotgun!");

            base.PlayAnimation("Shoot", "ShootGun", "ShootGun.playbackRate", 1.8f);
            damageEaten = GetComponent<DamageEaten>();
            SoakHitbox = base.GetModelChildLocator().FindChild("SoakHitbox");
        }


        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                base.characterBody.AddSpreadBloom(1.5f);
                SoakVFX = base.characterBody.corePosition;
                var MuzzleVFX = GameObject.Instantiate(Modules.Assets.MuzzleVFX, new Vector3(SoakHitbox.position.x, SoakHitbox.position.y, SoakHitbox.position.z), new Quaternion(SoakHitbox.rotation.x, SoakHitbox.rotation.y, SoakHitbox.rotation.z, SoakHitbox.rotation.w));
                MuzzleVFX.transform.parent = gameObject.transform;

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    base.AddRecoil(-1f * Shoot.recoil, -2f * Shoot.recoil, -0.5f * Shoot.recoil, 0.5f * Shoot.recoil);

                    new BulletAttack
                    {
                        bulletCount = 5 + damageEaten.DamageSoaked,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = Shoot.damageCoefficient * this.damageStat +damageEaten.DamageSoaked,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        maxDistance = Shoot.range,
                        force = Shoot.force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 10f,
                        maxSpread = 5f,
                        isCrit = base.RollCrit(),
                        owner = base.gameObject,
                        muzzleName = muzzleString,
                        smartCollision = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 2f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        tracerEffectPrefab = Shoot.tracerEffectPrefab,  
                        spreadPitchScale = 0.5f,
                        spreadYawScale = 0.5f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    }.Fire();
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireTime)
            {
                this.Fire();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            damageEaten.DamageSoaked = 0;

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}