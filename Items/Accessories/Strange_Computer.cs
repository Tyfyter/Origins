using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Strange_Computer : ModItem {
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory(28, 20);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			//player.GetModPlayer<OriginPlayer>().strangeComputer = true; blue laser
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Glass, 5);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 10);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddIngredient(ModContent.ItemType<Rotor>(), 6);
			recipe.AddTile(TileID.MythrilAnvil); //fabricator
			recipe.Register();
		}
	}
}
