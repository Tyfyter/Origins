using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Neck)]
	public class Volatile_Charm : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat",
			"Misc",
			"Explosive"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(14, 28);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveSelfDamage -= 0.15f;
			originPlayer.explosiveBlastRadius += 0.4f;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Bomb_Charm>());
			recipe.AddIngredient(ModContent.ItemType<Nitro_Crate>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
