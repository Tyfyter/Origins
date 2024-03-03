using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using System.Collections.Generic;
using Terraria;
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
}
