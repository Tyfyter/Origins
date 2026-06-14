using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Tiles.Ashen;

namespace Origins.Items.Weapons.Ranged {
    public class Artifiber_Bow : ModItem {
		public override void SetStaticDefaults() => Origins.AddGlowMask(this);
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodBow);
			Item.damage = 8;
			Item.width = 24;
			Item.height = 56;
			Item.value = Item.sellPrice(copper: 20);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-3f, 0);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Artifiber_Item>(), 10)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
