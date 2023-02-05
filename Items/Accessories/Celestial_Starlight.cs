using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Celestial_Starlight : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestial Starlight");
			Tooltip.SetDefault("Increases pickup range for mana\nChance for mana stars to fall from critical hits");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 3, silver: 30);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.CelestialMagnet);
			recipe.AddIngredient(ModContent.ItemType<Dim_Starlight>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.dimStarlight = true;
			player.manaMagnet = true;

			float light = 0.23f + (originPlayer.dimStarlightCooldown / 1000f);
			Lighting.AddLight(player.Center, 0.3f, 0.3f, 1f);
		}
	}
}
