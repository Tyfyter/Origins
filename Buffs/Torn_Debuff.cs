using Origins.NPCs;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.UI.Chat;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Localization;

namespace Origins.Buffs {
	public class Torn_Debuff : ModBuff {
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Torn");
			// Description.SetDefault("Your max life has been reduced!");
			Main.debuff[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<OriginPlayer>().tornDebuff = true;
			if (player.buffTime[buffIndex] <= 1) {
				player.buffTime[buffIndex] = 10;
				player.buffType[buffIndex] = Torn_Decay_Debuff.ID;
			}
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().tornDebuff = true;
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			string text = $"{Main.LocalPlayer.GetModPlayer<OriginPlayer>().tornCurrentSeverity * 100:#0}%";
			var font = FontAssets.CombatText[0].Value;
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				font,
				text,
				drawParams.TextPosition + new Vector2(0, Main.buffNoTimeDisplay[Type] ? 0 : 16),
				new Color(50, 180, 230),
				0,
				font.MeasureString(text) * new Vector2(0f, 0f),
				new Vector2(0.55f)
			);
		}
	}
	public class Torn_Decay_Debuff : Torn_Debuff {
		public static new int ID { get; private set; } = -1;
		public override string Texture => "Origins/Buffs/Torn_Debuff";
		public override LocalizedText DisplayName => Language.GetOrRegister($"{LocalizationCategory}.Torn_Debuff.DisplayName");
		public override LocalizedText Description => Language.GetOrRegister($"{LocalizationCategory}.Torn_Debuff.Description");
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.GetModPlayer<OriginPlayer>().tornCurrentSeverity <= 0) {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
