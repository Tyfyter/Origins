using Origins.Items.Materials;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
	public class Omnidirectional_Claymore : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "OtherExplosive",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Omnidirectional_Claymore_Tile>());
			Item.damage = 165;
			Item.value = Item.sellPrice(silver: 1, copper: 75);
			Item.rare = ItemRarityID.Pink;
			Item.noMelee = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 250)
			.AddIngredient(ItemID.ExplosivePowder, 7)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 13)
			.AddIngredient(ModContent.ItemType<Rotor>(), 25)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
}
