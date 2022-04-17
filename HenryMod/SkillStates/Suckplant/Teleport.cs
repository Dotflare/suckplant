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
using RoR2.Navigation;
using System.Linq;

namespace SuckplantMod.SkillStates
{
	// Token: 0x02000303 RID: 771
	public class Teleport : BaseSkillState
	{

		private Transform modelTransform;
		public bool disappearWhileBlinking;
		public GameObject blinkPrefab;
		public GameObject blinkDestinationPrefab;
		public Material destealthMaterial;
		private Vector3 blinkDestination = Vector3.zero;
		private Vector3 blinkStart = Vector3.zero;
		public float duration = 2f;
		public float TeleportTime = 2f;
		public float exitDuration = 0.1f;
		public float destinationAlertDuration;
		public float blinkDistance = 90f;
		public string beginSoundString;
		public string endSoundString;
		public float blastAttackRadius;
		public float blastAttackDamageCoefficient;
		public float blastAttackForce;
		public float blastAttackProcCoefficient;
		private Animator animator;
		private CharacterModel characterModel;
		private HurtBoxGroup hurtboxGroup;
		private ChildLocator childLocator;
		private GameObject blinkDestinationInstance;
		private bool isExiting;
		private bool hasBlinked;

		public override void OnEnter()
		{
			base.OnEnter();
			Chat.AddMessage("teleporting..");
			Util.PlaySound(this.beginSoundString, base.gameObject);
			this.modelTransform = base.GetModelTransform();
			base.PlayAnimation("Teleport", "TeleportStart");
			if (this.modelTransform)
			{
				this.animator = this.modelTransform.GetComponent<Animator>();
				this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
				this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
				this.childLocator = this.modelTransform.GetComponent<ChildLocator>();
			}
			if (this.disappearWhileBlinking)
			{
				if (this.characterModel)
				{
					this.characterModel.invisibilityCount++;
				}
				if (this.hurtboxGroup)
				{
					HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
					int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
					hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
				}
				if (this.childLocator)
				{
					this.childLocator.FindChild("DustCenter").gameObject.SetActive(false);
				}
			}
			if (base.characterMotor)
			{
				base.characterMotor.enabled = false;
			}
			base.gameObject.layer = LayerIndex.fakeActor.intVal;
			base.characterMotor.Motor.RebuildCollidableLayers();
			this.CalculateBlinkDestination();
			//this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		}
		private void CalculateBlinkDestination()
		{
			Vector3 vector = Vector3.zero;
			Ray aimRay = base.GetAimRay();
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.searchOrigin = aimRay.origin;
			bullseyeSearch.searchDirection = aimRay.direction;
			bullseyeSearch.maxDistanceFilter = this.blinkDistance;
			bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
			bullseyeSearch.filterByLoS = false;
			bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(base.gameObject));
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
			bullseyeSearch.RefreshCandidates();
			HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
			if (hurtBox)
			{
				vector = hurtBox.transform.position - base.transform.position;
			}
			this.blinkDestination = base.transform.position;
			this.blinkStart = base.transform.position;
			NodeGraph groundNodes = SceneInfo.instance.groundNodes;
			NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(base.transform.position + vector, base.characterBody.hullClassification, float.PositiveInfinity);
			groundNodes.GetNodePosition(nodeIndex, out this.blinkDestination);
			this.blinkDestination += base.transform.position - base.characterBody.footPosition;
			base.characterDirection.forward = vector;
		}


		private void CreateBlinkEffect(Vector3 origin)
		{
			if (this.blinkPrefab)
			{
				EffectData effectData = new EffectData();
				effectData.rotation = Util.QuaternionSafeLookRotation(this.blinkDestination - this.blinkStart);
				effectData.origin = origin;
				EffectManager.SpawnEffect(this.blinkPrefab, effectData, false);
			}
		}

		private void SetPosition(Vector3 newPosition)
		{

			base.PlayAnimation("Teleport", "TeleportEnd");
			if (base.characterMotor)
			{
				base.characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration)
				{ if (base.characterMotor)
					{
						base.characterMotor.velocity = Vector3.zero;
					}
					if (!this.hasBlinked)
					{
						this.SetPosition(this.blinkDestination);
					}
					if (base.fixedAge >= this.duration - this.destinationAlertDuration && !this.hasBlinked)
					{
						this.hasBlinked = true;
						if (this.blinkDestinationPrefab)
						{
							this.blinkDestinationInstance = UnityEngine.Object.Instantiate<GameObject>(this.blinkDestinationPrefab, this.blinkDestination, Quaternion.identity);
							this.blinkDestinationInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = this.destinationAlertDuration;
						}
						this.SetPosition(this.blinkDestination);
					}

					if (base.fixedAge >= this.duration + this.exitDuration && base.isAuthority)
					{
						this.outer.SetNextStateToMain();
					}
				}
		}

		public override void OnExit()
		{
			base.OnExit();
		}
	}
}
