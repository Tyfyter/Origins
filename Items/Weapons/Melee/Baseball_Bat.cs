using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using Origins.Items.Materials;
namespace Origins.Items.Weapons.Melee {
	public class Baseball_Bat : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Sword"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenSword);
			Item.damage = 5;
			Item.knockBack = 8f;
			Item.value = Item.sellPrice(silver: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Wood, 7)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
