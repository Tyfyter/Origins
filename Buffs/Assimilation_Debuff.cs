using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Buffs {
    public class Corrupt_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Corrupt Assimilation");
			// Description.SetDefault("You're being assimilated by the Corruption");
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
			player.GetDamage(DamageClass.Generic) -= Math.Min(percent / 2, 0);
			player.GetAttackSpeed(DamageClass.Generic) -= Math.Min(percent / 2, 0);
			player.GetCritChance(DamageClass.Generic) -= Math.Min(percent / 2, 0);
			player.GetKnockback(DamageClass.Generic) -= Math.Min(percent / 2, 0);
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
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Crimson Assimilation");
			// Description.SetDefault("You're being assimilated by the Crimson");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			float percent = originPlayer.CrimsonAssimilation * originPlayer.crimsonAssimilationDebuffMult;
			int buffChosen = Main.rand.Next(0, 2);
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Crimson"
				}, 40, 0);
			}
			if (Main.rand.NextFloat(50, 200) < percent) {
				if (buffChosen == 0) {
					player.AddBuff(BuffID.Confused, Main.rand.Next(24, 69) * (1 + (int)percent));
				} else if (buffChosen == 1) {
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
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Defiled Assimilation");
			// Description.SetDefault("You're being assimilated by the Defiled");
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
			if (percent >= 0.17) {
				player.AddBuff(BuffID.Weak, 10);
            }
			if (percent >= 0.33) {
				player.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), 10);
			}
			if (percent >= 0.67) {
				player.AddBuff(BuffID.BrokenArmor, 10);
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
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Riven Assimilation");
			// Description.SetDefault("You're being assimilated by the Riven");
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
			//OriginPlayer.InflictTorn(player, 60, 1800, 1 - percent); This is the code I tried implementing. Could fix to where the player doesn't just count down their health to zero?
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().RivenAssimilation;

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
}
