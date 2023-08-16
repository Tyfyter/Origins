using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories
{
    public class Dim_Starlight : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.BandofStarpower] = ModContent.ItemType<Dim_Starlight>();
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Dim_Starlight>()] = ItemID.BandofStarpower;
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 30);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.BandofStarpower);
            recipe.AddIngredient(ModContent.ItemType<Wilting_Rose_Item>());
            recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.DisableDecraft();
            recipe.Register();

            recipe = Recipe.Create(ItemID.PanicNecklace);
            recipe.AddIngredient(ItemID.SwiftnessPotion);
            recipe.AddIngredient(Type);
            recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.DisableDecraft();
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
