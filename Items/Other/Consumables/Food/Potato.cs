using Microsoft.Xna.Framework;
using Origins.Items.Weapons;
using Origins.Items.Weapons.Magic;
using Origins.Tiles.Other;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
    public class Potato : ModItem {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(216, 209, 135),
				new Color(209, 188, 92),
				new Color(181, 148, 58)
			];
			ItemID.Sets.IsFood[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ChocolateChipCookie);
			Item.holdStyle = ItemHoldStyleID.HoldUp;
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
