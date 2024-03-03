using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Tiles.Endowood {
	public class Endowood_Door : DoorBase {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(6)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
	}
	public class Endowood_Grandfather_Clock : ClockBase {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Glass, 6)
				.AddIngredient<Endowood_Item>(10)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
	}
	public class Endowood_Chair : ChairBase {
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
			};
		}
	}
	public class Endowood_Toilet : ChairBase {
		public override int BaseTileID => TileID.Toilets;
		public override Color MapColor => new(44, 39, 58);
		public override void OnLoad() {
			item.OnAddRecipes += (item) => {
				Recipe.Create(item.type)
				.AddIngredient<Endowood_Item>(6)
				.AddTile(TileID.Sawmill)
				.Register();
			};
		}
		public override void HitWire(int i, int j) {
			ToiletHitWire(i, j);
		}
	}
	public class Endowood_Chest : ModChest {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), MapChestName);
			AdjTiles = new int[] { TileID.Containers };
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
	}
	public class Endowood_Chest_Item : ModItem {
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Endowood_Chest>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Endowood_Item>(8)
			.AddRecipeGroup(RecipeGroupID.IronBar, 2)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
