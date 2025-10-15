using Origins.Dev;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Marrowick_Sword : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Sword
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodSword);
			Item.damage = 12;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(copper: 30);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Marrowick_Item>(), 7)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
