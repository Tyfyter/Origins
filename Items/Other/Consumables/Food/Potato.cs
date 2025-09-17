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
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.DefaultToFood(16, 16, BuffID.WellFed, 60 * 60 * 10);
			Item.holdStyle = ItemHoldStyleID.HoldUp;
			Item.value = Item.sellPrice(silver: 1);
			Item.ammo = ModContent.ItemType<Potato>();
			Item.notAmmo = true;
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
			Recipe.Create(ModContent.ItemType<Hot_Potato>())
			.AddIngredient(ItemID.HellstoneBar, 14)
			.AddIngredient(this)
			.AddTile(TileID.DemonAltar)
			.Register();

			Recipe.Create(ItemID.PotatoChips)
			.AddIngredient(this)
			.AddTile(TileID.CookingPots)
			.AddCondition(OriginsModIntegrations.NotAprilFools)
			.Register();

			Recipe.Create(ItemID.PotatoChips)
			.AddIngredient(this)
			.AddTile(TileID.DemonAltar)
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();
		}
	}
}
