using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Food {
	public class Potato : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Potato");
			Tooltip.SetDefault("{$CommonItemTooltip.MinorStats}\nIt's a potato...");
			SacrificeTotal = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ChocolateChipCookie);
			Item.holdStyle = ItemHoldStyleID.HoldUp;
			Item.scale = 0.75f;
			Item.buffType = BuffID.WellFed;
			Item.buffTime = 60 * 60 * 10;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.White;
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
