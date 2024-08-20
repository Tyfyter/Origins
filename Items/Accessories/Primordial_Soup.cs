using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Primordial_Soup : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Vitality"
        ];
        public override void SetStaticDefaults() {
            ItemID.Sets.ShimmerTransformToItem[ItemID.PanicNecklace] = ModContent.ItemType<Primordial_Soup>();
            ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Primordial_Soup>()] = ItemID.PanicNecklace;
            glowmask = Origins.AddGlowMask(this);
        }
        static short glowmask;
        public override void SetDefaults() {
            Item.DefaultToAccessory(38, 20);
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.PanicNecklace);
            recipe.AddIngredient(ItemID.ThornsPotion);
            recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.DisableDecraft();
			recipe.Register();

            recipe = Recipe.Create(ItemID.BandofStarpower);
            recipe.AddIngredient(ItemID.ManaCrystal);
            recipe.AddIngredient(Type);
            recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.DisableDecraft();
			recipe.Register();
        }
        public override void UpdateAccessory(Player player, bool isHidden) {
            player.GetModPlayer<OriginPlayer>().primordialSoup = true;
        }
	}
}
