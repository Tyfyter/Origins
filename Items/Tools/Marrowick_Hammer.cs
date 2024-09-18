using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
    public class Marrowick_Hammer : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodHammer);
			Item.damage = 7;
			Item.DamageType = DamageClass.Melee;
			Item.hammer = 45;
			Item.value = Item.sellPrice(copper: 10);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Marrowick_Item>(), 8)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
