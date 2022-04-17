using System;
using RoR2;
using EntityStates;

namespace SuckplantMod.SkillStates.Suckplant
{
	// Token: 0x02000C38 RID: 3128
	public class SpawnState : GenericCharacterSpawnState
	{
		// Token: 0x060046A6 RID: 18086 RVA: 0x0011E4BE File Offset: 0x0011C6BE
		public override void OnEnter()
		{
			base.OnEnter();
			base.PlayAnimation("Body", "Spawn", "Spawn.playbackRate", SpawnState.duration);
			Util.PlaySound(EntityStates.ClayBruiserMonster.SpawnState.spawnSoundString, base.gameObject);
		}

		// Token: 0x060046A7 RID: 18087 RVA: 0x0011E4F1 File Offset: 0x0011C6F1
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= SpawnState.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
			}
		}

		// Token: 0x060046A8 RID: 18088 RVA: 0x000E45B9 File Offset: 0x000E27B9
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}

		// Token: 0x040040BE RID: 16574
		public static float duration = 3f;

		// Token: 0x040040BF RID: 16575
		public static string spawnSoundString;
	}
}
