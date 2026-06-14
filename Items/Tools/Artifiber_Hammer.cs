using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Artifiber_Hammer : ModItem {
		public override void SetStaticDefaults() => Origins.AddGlowMask(this);
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodHammer);
			Item.damage = 8;
			Item.DamageType = DamageClass.Melee;
			Item.hammer = 40;
			Item.value = Item.sellPrice(copper: 10);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Artifiber_Item>(), 8)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
