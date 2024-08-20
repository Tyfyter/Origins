using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Hallowed_Cleaver : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Gun"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Gatligator);
			Item.damage = 39;
			Item.useAnimation = Item.useTime = 10;
			Item.shootSpeed *= 2;
			Item.width = 92;
			Item.height = 28;
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.Pink;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.HallowedBar, 13);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-18, -2);
	}
}
