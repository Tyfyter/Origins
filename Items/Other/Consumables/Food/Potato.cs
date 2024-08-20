using Origins.Items.Weapons;
using Origins.Items.Weapons.Magic;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Potato : ModItem {
        public string[] Categories => [
            "Food"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ChocolateChipCookie);
			Item.holdStyle = ItemHoldStyleID.HoldUp;
			Item.scale = 0.75f;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 10;
			Item.value = Item.sellPrice(silver: 1);
			Item.ammo = ModContent.ItemType<Potato>();
			Item.shoot = ModContent.ProjectileType<Potato_P>();
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.createTile = ModContent.TileType<Potato_Tile>();
				Item.buffType = 0;
			} else {
				Item.createTile = -1;
				Item.buffType = BuffID.WellFed;
			}
			return true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ModContent.ItemType<Hot_Potato>());
			recipe.AddIngredient(ItemID.HellstoneBar, 14);
			recipe.AddIngredient(this);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
}
