using ModLiquidLib.ModLoader;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Liquid;
using Terraria.ID;

namespace Origins.Liquids; 
public class OriginsGlobalLiquid : GlobalLiquid {
	public override bool EmitEffects(int i, int j, int type, LiquidRenderer.LiquidCache liquidCache) {
		if (type == LiquidID.Lava && !BlocksLavaSmoke(Main.tile[i, j - 1]) && Main.rand.NextBool(100)) {
			int y = j + 1;
			Tile tile = Main.tile[i, y];
			while (!tile.HasTile && tile.LiquidAmount > 0) tile = Main.tile[i, ++y];
			if (tile.HasTile && OriginsSets.Tiles.LavaAboveEmitsSmoke[tile.TileType]) {
				Vector2 pos = new(i * 16, j * 16 + 16 - tile.LiquidAmount / 16f);
				Gore.NewGoreDirect(
					new EntitySource_TileUpdate(i, j),
					pos,
					default,
					GoreID.ChimneySmoke1 + Main.rand.Next(3)
				);
			}
		}
		return true;
	}
	static bool BlocksLavaSmoke(Tile tile) => (tile.HasTile && Main.tileSolid[tile.TileType]) || tile.LiquidAmount > 0;
}
