﻿using Origins.Dev;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Ambition_Buff : ModBuff {
		public static int ID { get; private set; }
		public string[] Categories => [
			WikiCategories.GenericBoostBuff
		];
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetDamage(DamageClass.Generic) += 0.025f;
			player.GetCritChance(DamageClass.Generic) += 0.025f;
			player.GetAttackSpeed(DamageClass.Generic) += 0.025f;
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.statSharePercent += 0.125f;
			originPlayer.meleeScaleMultiplier += 0.1f;
			originPlayer.projectileSpeedBoost += 0.1f;
		}
	}
}
