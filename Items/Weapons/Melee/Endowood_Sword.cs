using Origins.Dev;
using Origins.Tiles.Defiled;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Endowood_Sword : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Sword
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ShadewoodSword);
			Item.damage = 11;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(copper: 30);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Endowood_Item>(), 7)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
