using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Hallowed_Cleaver : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Gun
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
			Recipe.Create(Type)
			.AddIngredient(ItemID.HallowedBar, 13)
            .AddTile(TileID.MythrilAnvil)
            .Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-18, -2);
	}
}
