using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Coologne : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Coologne");
            Tooltip.SetDefault("Increases life regeneration at low health\n	Puts a shell around the owner when below 50% life that reduces damage by 25%");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 21;
            Item.height = 20;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.sellPrice(gold: 10);
        }
        public override void UpdateAccessory(Player player, bool hideVisual) {
            if (player.statLife <= player.statLifeMax2 * 0.5f) player.AddBuff(BuffID.IceBarrier, 5);
            Mysterious_Spray.EquippedEffect(player);
			if (!hideVisual) {
                Mysterious_Spray.VanityEffect(player);
            }
        }
		public override void UpdateVanity(Player player) {
            Mysterious_Spray.VanityEffect(player);
        }
		public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FrozenTurtleShell);
            recipe.AddIngredient(ModContent.ItemType<Mysterious_Spray>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
		}
	}
}
