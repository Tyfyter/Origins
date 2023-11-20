using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Keepsake_Remains : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"Torn",
			"TornSource"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 28);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().symbioteSkull = true;
			player.GetArmorPenetration(DamageClass.Generic) += 5;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SharkToothNecklace);
			recipe.AddIngredient(ModContent.ItemType<Symbiote_Skull>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
