using System;
using RoR2;
using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace SuckplantMod.SkillStates.Suckplant
{
	// Token: 0x02000C37 RID: 3127
	public class DeathState : GenericCharacterDeath
	{
		// Token: 0x060046A2 RID: 18082 RVA: 0x0011E3D0 File Offset: 0x0011C5D0
		public override void OnEnter()
		{
			base.OnEnter();
			Transform modelTransform = base.GetModelTransform();
			base.PlayAnimation("Death", "Death", "Spawn.playbackRate", SpawnState.duration);
			/*
			if (modelTransform && modelTransform.GetComponent<ChildLocator>() && DeathState.initialEffect)
			{
				EffectManager.SpawnEffect(DeathState.initialEffect, new EffectData
				{
					origin = base.transform.position,
					scale = DeathState.initialEffectScale
				}, false);
			}
			*/
		}

        public override void PlayDeathSound()
        {
        }

        // Token: 0x060046A3 RID: 18083 RVA: 0x0011E494 File Offset: 0x0011C694
        public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge > 0.5f)
			{
				base.DestroyBodyAsapServer();
			}
		}

		// Token: 0x060046A4 RID: 18084 RVA: 0x0011E4B6 File Offset: 0x0011C6B6
		public override void OnExit()
		{
			base.OnExit();
		}

		public static float dropTime = .15f;
		// Token: 0x040040BA RID: 16570
		public static GameObject initialEffect;

		// Token: 0x040040BB RID: 16571
		public static float initialEffectScale;

		// Token: 0x040040BC RID: 16572
		public static float velocityMagnitude;

		// Token: 0x040040BD RID: 16573
		public static float explosionForce;
	}
}
