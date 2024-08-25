using Origins.Dev;
using Origins.Items.Other.Consumables.Food;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Potato_Mine : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "OtherExplosive",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LandMine);
			Item.damage = 85;
			Item.createTile = ModContent.TileType<Potato_Mine_Tile>();
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.White;
			Item.ammo = ModContent.ItemType<Potato>();
            Item.ArmorPenetration += 6;
        }
		public override void HoldItem(Player player) {
			Item.shoot = ProjectileID.None;
		}
		public override void UpdateInventory(Player player) {
			Item.shoot = Potato_Mine_P.ID; // has to be done here somewhere like this because it blocks placing the tile if it's not 0 when the player uses the item
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type, 150);
			recipe.AddIngredient(ItemID.ExplosivePowder, 15);
			recipe.AddIngredient(ModContent.ItemType<Potato>());
			recipe.Register();
		}
	}
}
