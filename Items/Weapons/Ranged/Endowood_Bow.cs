using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Tiles.Defiled;
namespace Origins.Items.Weapons.Ranged {
    public class Endowood_Bow : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Bow
        ];
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
			.AddIngredient(ModContent.ItemType<Endowood_Item>(), 10)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
