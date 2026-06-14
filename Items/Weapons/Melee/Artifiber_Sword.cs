using Origins.Dev;
using Origins.Tiles.Ashen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Artifiber_Sword : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Sword
        ];
		public override void SetStaticDefaults() => Origins.AddGlowMask(this);
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodSword);
			Item.damage = 11;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(copper: 30);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Artifiber_Item>(), 7)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
