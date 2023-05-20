using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Buffs {
    public class Corrupt_Assimilation_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Corrupt Assimilation");
			Description.SetDefault("You're being assimilated by the Corruption");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().CorruptionAssimilation;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Corruption"
				}, 40, 0);
			}
			// custom effects per percent in else if chain
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().CorruptionAssimilation;

			string text = $"{percent:P0}";
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
			DisplayName.SetDefault("Crimson Assimilation");
			Description.SetDefault("You're being assimilated by the Crimson");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().CrimsonAssimilation;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Crimson"
				}, 40, 0);
			}
			// custom effects per percent in else if chain
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().CrimsonAssimilation;

			string text = $"{percent:P0}";
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
			DisplayName.SetDefault("Defiled Assimilation");
			Description.SetDefault("You're being assimilated by the Defiled");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().DefiledAssimilation;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Defiled"
				}, 40, 0);
			}
			// custom effects per percent in else if chain
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().DefiledAssimilation;

			string text = $"{percent:P0}";
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
			DisplayName.SetDefault("Riven Assimilation");
			Description.SetDefault("You're being assimilated by the Riven");
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().RivenAssimilation;
			if (percent >= OriginPlayer.assimilation_max) {
				player.KillMe(new KeyedPlayerDeathReason() {
					Key = "Mods.Origins.DeathMessage.Assimilation.Riven"
				}, 40, 0);
			}
			// custom effects per percent in else if chain
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			float percent = Main.LocalPlayer.GetModPlayer<OriginPlayer>().RivenAssimilation;

			string text = $"{percent:P0}";
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
