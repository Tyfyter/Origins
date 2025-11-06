using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Scrap_Railing : Platform_Tile {
		public override void OnLoad() {
			Item.OnAddRecipes += (item) => {
				Recipe.Create(item.type, 2)
				.AddIngredient(ModContent.ItemType<Scrap>(), 1)
				.Register();
				Recipe.Create(ModContent.ItemType<Scrap>())
				.AddIngredient(item.type, 2)
				.Register();
			};
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			TileID.Sets.Platforms[Type] = false;
			TileID.Sets.CanPlaceNextToNonSolidTile[Type] = true;
			TileID.Sets.HasSlopeFrames[Type] = true;
			Main.tileSolidTop[Type] = false;
			Main.tileSolid[Type] = false;
			DustType = DustID.Lihzahrd;
			RegisterItemDrop(Item.Type);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
			switch (tileFrameX / 18) {
				case 8:
				case 10:
				offsetY = 8;
				break;
			}
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j];
			Tile below = Framing.GetTileSafely(i, j + 1);
			Tile left = Framing.GetTileSafely(i - 1, j);
			Tile right = Framing.GetTileSafely(i + 1, j);
			tile.TileFrameY = 2 * 18;
			switch ((left.TileIsType(Type) && Framing.GetTileSafely(i - 1, j + 2).Slope == SlopeType.SlopeDownRight, right.TileIsType(Type) && Framing.GetTileSafely(i + 1, j + 2).Slope == SlopeType.SlopeDownLeft)) {
				case (true, true):
				tile.TileFrameX = 18 * 3;
				return false;
				case (true, false):
				tile.TileFrameX = 18 * 5;
				return false;
				case (false, true):
				tile.TileFrameX = 18 * 6;
				return false;
			}
			bool polish = false;
			if (below.HasTile) {
				if (below.TileType == Type) {
					tile.TileFrameX = 0;
					polish = true;
				} else if (Main.tileSolid[below.TileType]) {
					tile.TileFrameX = 0;
					tile.TileFrameY = 3 * 18;
					j -= 1;
					left = Framing.GetTileSafely(i - 1, j);
					right = Framing.GetTileSafely(i + 1, j);
					polish = true;
					if (!Framing.GetTileSafely(i, j).TileIsType(Type)) {
						tile.TileFrameY = 0;
						return false;
					}
				}
			}
			below = Framing.GetTileSafely(i, j + 2);
			if (below.HasTile && Main.tileSolid[below.TileType] && below.TopSlope) {
				int xOffset = 0;
				switch (below.Slope) {
					case SlopeType.SlopeDownLeft:
					tile.TileFrameX = 18 * 10;
					xOffset = 1;
					break;
					case SlopeType.SlopeDownRight:
					tile.TileFrameX = 18 * 8;
					xOffset = -1;
					break;
				}
				if (tile.TileFrameY != 18 * 3) {
					if (Framing.GetTileSafely(i, j + 1).TileIsType(Type)) {
						if (Framing.GetTileSafely(i + xOffset, j).TileIsType(Type)) {
							tile.TileFrameY = 18 * 2;
						} else {
							tile.TileFrameY = 18;
						}
					} else {
						tile.TileFrameY = 0;
					}
				}
				return false;
			}
			if (polish) {
				switch ((left.TileIsType(Type), right.TileIsType(Type))) {
					case (true, true):
					tile.TileFrameX = 0;
					break;
					case (true, false):
					tile.TileFrameX = 18;
					break;
					case (false, true):
					tile.TileFrameX = 18 * 2;
					break;
					case (false, false):
					tile.TileFrameX = 18 * 4;
					break;
				}
			} else {
				switch ((left.TileIsType(Type), right.TileIsType(Type))) {
					case (true, true):
					tile.TileFrameX = 18 * 3;
					break;
					case (true, false):
					tile.TileFrameX = 18 * 5;
					break;
					case (false, true):
					tile.TileFrameX = 18 * 6;
					break;
					case (false, false):
					tile.TileFrameX = 0;
					tile.TileFrameY = 3 * 18;
					break;
				}
			}
			if (tile.TileFrameY == 2 * 18) {
				switch (tile.TileFrameX / 18) {
					case 0:
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
					case 6: {
						if (Framing.GetTileSafely(i, j - 1).TileIsType(Type)) {
							tile.TileFrameY -= 18;
						}
						break;
					}
				}
			}
			return false;
		}
	}
}
