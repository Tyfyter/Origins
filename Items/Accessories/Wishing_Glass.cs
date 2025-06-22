using Origins.Dev;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Wishing_Glass : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"GenericBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.master = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().WishingGlass = true;
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) {
			player.OriginPlayer().wishingGlassVisible = true;
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) => player.OriginPlayer().wishingGlassDye = dye;
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.SubstituteKeybind(Keybindings.WishingGlass);
		}
	}
	public class Wishing_Glass_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Wishing_Glass_Buff";
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (originPlayer.WishingGlass) {
				player.GetAttackSpeed(DamageClass.Generic) *= 2;
				originPlayer.wishingGlassActive = true;
				originPlayer.wishingGlassCooldown = 10 * 60;
			} else {
				player.buffType[buffIndex] = BuffID.Cursed;
				player.buffTime[buffIndex] += 5 * 60;
				originPlayer.wishingGlassCooldown = player.buffTime[buffIndex] + 10 * 60;
			}
		}
	}
}
