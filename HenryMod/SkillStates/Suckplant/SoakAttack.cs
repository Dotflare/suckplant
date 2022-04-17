using SuckplantMod.SkillStates.BaseStates;
using System;
using System.Collections.Generic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using System.Collections;
using SuckplantMod.Modules.Components;
using UnityEngine.AddressableAssets;

namespace SuckplantMod.SkillStates
{
    public class SoakAttack : BaseTimedSkillState


    {
        public float SoakRadius = 20f;
        public RoR2.CharacterBody body;
        List<ProjectileController> bullets = new List<ProjectileController>();
        public Transform TargetTransform;
        public static float SkillBaseDuration = 12f;
        public static float SkillStartTime = 0.1f;
        public static float SkillEndTime = 0.9f;
        private DamageEaten damageEaten;
        public float EatDistance;
        public static Transform SoakHitbox;




        public override void OnEnter()
        {
            base.OnEnter();
            damageEaten = GetComponent<DamageEaten>();
            InitDurationValues(SkillBaseDuration, SkillStartTime, SkillEndTime);
            SoakHitbox = base.GetModelChildLocator().FindChild("SoakHitbox");
            base.PlayAnimation("SoakAttack", "Start");
        }

        protected override void OnCastEnter()
        {
            Chat.AddMessage("StartSucking");
            var SuckVFX = GameObject.Instantiate(Modules.Assets.IsSuckingVFX, new Vector3(SoakHitbox.position.x, SoakHitbox.position.y, SoakHitbox.position.z), Quaternion.identity);
            SuckVFX.transform.parent = gameObject.transform;

        }

        protected override void OnCastFixedUpdate()
        {
            this.TargetTransform = base.characterBody.transform;
            this.SoakProjectiles();


            //Chat.AddMessage("IsSucking");
        }

        protected override void OnCastExit()
        {
            base.OnExit();
            //Chat.AddMessage("StoppedSucking");
            base.PlayAnimation("SoakAttack", "End");
        }

        private void SoakProjectiles()
        {
            
            //Debug.LogWarning(damageEaten != null);


            new RoR2.SphereSearch
            {
                radius = SoakRadius,
                mask = RoR2.LayerIndex.projectile.mask,
                origin = this.TargetTransform ? this.TargetTransform.position : body.corePosition
            }.RefreshCandidates().FilterCandidatesByProjectileControllers().GetProjectileControllers(bullets);
            if (bullets.Count > 0)
                foreach (ProjectileController controller in bullets)
                {
                    if (controller)
                    {
                        var controllerOwner = controller.owner;
                        if (controllerOwner)
                        {
                            var ownerBody = controllerOwner.GetComponent<RoR2.CharacterBody>();
                            if (ownerBody)
                            {
                                var projectileDamage = controller.gameObject.GetComponent<ProjectileDamage>();
                                if (projectileDamage)
                                { 
                                    projectileDamage = null;
                                    ProjectileTargetComponent targetComponent = controller.GetComponent<ProjectileTargetComponent>();
                                    ProjectileSteerTowardTarget steerComponent = controller.GetComponent<ProjectileSteerTowardTarget>();
                                    ProjectileSimple dirComponent = controller.GetComponent<ProjectileSimple>();

                                    if (steerComponent == null)
                                    {
                                        targetComponent = controller.gameObject.AddComponent<ProjectileTargetComponent>();
                                        steerComponent = controller.gameObject.AddComponent<ProjectileSteerTowardTarget>();
                                        dirComponent = controller.gameObject.AddComponent<ProjectileSimple>();
                                        //Chat.AddMessage("Added Component");
                                    }

                                    steerComponent.rotationSpeed = 360;

                                    targetComponent.target = base.GetModelChildLocator().FindChild("SoakHitbox");

                                    dirComponent.updateAfterFiring = true;

                                    dirComponent.desiredForwardSpeed = 60f;

                                    projectileDamage = null;


                                    EatDistance = Vector3.Distance(controller.transform.position, transform.position);

                                    if (EatDistance <= 9f)
                                    {

                                        damageEaten.DamageSoaked = +1;
                                        EntityState.Destroy(controller.gameObject);
                                        //Chat.AddMessage("Deleted Projectile!");
                                    }


                                }
                            }
                        }
                    }

                }


        }
    }
}
