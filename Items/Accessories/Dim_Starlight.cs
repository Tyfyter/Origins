using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories
{
    public class Dim_Starlight : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Dim Starlight");
			// Tooltip.SetDefault("Chance for mana stars to fall from critical hits");
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			ItemID.Sets.ShimmerTransformToItem[ItemID.BandofStarpower] = ModContent.ItemType<Dim_Starlight>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Dim_Starlight>()] = ItemID.BandofStarpower;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.BandofStarpower);
            recipe.AddIngredient(ModContent.ItemType<Wilting_Rose_Item>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();

            recipe = Recipe.Create(ItemID.PanicNecklace);
            recipe.AddIngredient(ItemID.SwiftnessPotion);
            recipe.AddIngredient(Type);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
        public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.dimStarlight = true;
			float light = 0.2f + (originPlayer.dimStarlightCooldown / 1000f);
			Lighting.AddLight(player.Center, light, light, light);
		}
	}
}
