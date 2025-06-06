using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs.Food {
	public class Nerve_Flan_Food_Buff : ModBuff {
		public override void SetStaticDefaults() {
			BuffID.Sets.IsWellFed[Type] = true;
			BuffID.Sets.IsFedState[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.wellFed = true;
			player.statDefense += 6;
			player.GetCritChance(DamageClass.Generic) += 6;
			player.GetDamage(DamageClass.Generic) += 0.15f;
			player.GetAttackSpeed(DamageClass.Generic) += 0.05f;
			player.GetAttackSpeed(DamageClass.Melee) += 0.1f;
			player.GetKnockback(DamageClass.Summon) += 1.5f;
			player.moveSpeed += 0.5f;
			player.pickSpeed -= 0.2f;
			const int rasterization = 2;
			ref int rasterizedTime = ref player.OriginPlayer().rasterizedTime;
			if (rasterizedTime < rasterization) rasterizedTime = rasterization;
		}
	}
}
