using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;

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
}
