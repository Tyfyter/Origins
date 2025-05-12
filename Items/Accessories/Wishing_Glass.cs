using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Wishing_Glass : ModItem, ICustomWikiStat {
		public override string Texture => "Terraria/Images/Item_" + ItemID.WineGlass;
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
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().WishingGlass = true;
		}
	}
	public class Wishing_Glass_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Item_" + ItemID.WineGlass;
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
