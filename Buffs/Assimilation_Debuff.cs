using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Buffs {
	public class Corrupt_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			float percent = originPlayer.CorruptionAssimilation * originPlayer.corruptionAssimilationDebuffMult;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Corruption"
				}, 40, 0);
			}
			player.GetDamage(DamageClass.Generic) -= percent / 2;
			player.GetAttackSpeed(DamageClass.Generic) -= percent / 2;
			player.GetCritChance(DamageClass.Generic) -= percent * 10;
			player.GetKnockback(DamageClass.Generic) -= percent / 2;
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().CorruptionAssimilation;

			string text = $"{percent * 100:#0}%";
			Color color = new Color(new Vector4(Main.buffAlpha[buffIndex]));
			spriteBatch.DrawString(
				FontAssets.ItemStack.Value,
				text,
				drawParams.TextPosition,
				color,
				0f,
				Vector2.Zero,
				0.8f,
				SpriteEffects.None,
			0f);
		}
	}
	public class Crimson_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			float percent = originPlayer.CrimsonAssimilation * originPlayer.crimsonAssimilationDebuffMult;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Crimson"
				}, 40, 0);
			}
			if (Main.rand.NextFloat(0.5f, 2f) < percent) {
				if (Main.rand.NextBool(2)) {
					player.AddBuff(BuffID.Confused, Main.rand.Next(24, 48) * (1 + (int)percent));
				} else {
					player.AddBuff(BuffID.Bleeding, Main.rand.Next(48, 138) * (1 + (int)percent));
				}
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().CrimsonAssimilation;

			string text = $"{percent * 100:#0}%";
			Color color = new Color(new Vector4(Main.buffAlpha[buffIndex]));
			spriteBatch.DrawString(
				FontAssets.ItemStack.Value,
				text,
				drawParams.TextPosition,
				color,
				0f,
				Vector2.Zero,
				0.8f,
				SpriteEffects.None,
			0f);
		}
	}
	public class Defiled_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			float percent = originPlayer.DefiledAssimilation * originPlayer.defiledAssimilationDebuffMult;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Defiled"
				}, 40, 0);
			}
			if (percent >= 0.125 /*&& Main.rand.NextFloat(0, 200) < percent - 0.125*/) {
				player.AddBuff(BuffID.Weak, 300);
            }
			if (percent >= 0.35 /*&& Main.rand.NextFloat(0, 200) < percent - 0.35*/) {
				player.AddBuff(BuffID.BrokenArmor, 180);
			}
			if (percent >= 0.5) {
				player.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), (int)(((percent - 0.5) / (1 - 0.5)) * 14));
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().DefiledAssimilation;

			string text = $"{percent * 100:#0}%";
			Color color = new Color(new Vector4(Main.buffAlpha[buffIndex]));
			spriteBatch.DrawString(
				FontAssets.ItemStack.Value,
				text,
				drawParams.TextPosition,
				color,
				0f,
				Vector2.Zero,
				0.8f,
				SpriteEffects.None,
			0f);
		}
	}
	public class Riven_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			float percent = originPlayer.RivenAssimilation * originPlayer.rivenAssimilationDebuffMult;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Riven"
				}, 40, 0);
			}
			
			OriginPlayer.InflictTorn(player, 60, originPlayer.timeSinceRivenAssimilated < 5 ? 5 : 1000, percent * ServerSideAccessibility.Instance.RivenAsimilationMultiplier, true);
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().RivenAssimilation;

			string text = $"{percent * 100:#0}%";
			Color color = new(new Vector4(Main.buffAlpha[buffIndex]));
			spriteBatch.DrawString(
				FontAssets.ItemStack.Value,
				text,
				drawParams.TextPosition,
				color,
				0f,
				Vector2.Zero,
				0.8f,
				SpriteEffects.None,
			0f);
		}
	}
}
