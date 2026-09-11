using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen;
public class Conveyor_Scooper : ModTile {
	public override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(this.DropTileItem)
		.RegisterItem();
	}
	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		TileID.Sets.DrawTileInSolidLayer[Type] = true;
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.SetHeight(3);
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.newAlternate.AnchorTop = new(AnchorType.SolidBottom, 0, 3);
		TileObjectData.addAlternate(1);
		TileObjectData.addTile(Type);
		AddMapEntry(new Color(194, 69, 12), CreateMapEntryName());
		DustType = Ashen_Biome.DefaultTileDust;
	}
	static int GetFacingDirection(int i, int j) {
		Tile tile = Main.tile[i, j];
		int style = TileObjectData.GetTileStyle(tile);
		int dir = 0;
		i -= (tile.TileFrameX / 18) % 3;
		j -= tile.TileFrameY / 18;
		if (style != 0) {
			j -= 1;
		} else {
			j += 3;
		}
		for (int x = 0; x < 3; x++) {
			if (!WorldGen.InWorld(i + x, j)) continue;
			tile = Main.tile[i + x, j];
			if (!tile.HasTile) continue;
			dir += TileID.Sets.ConveyorDirection[tile.TileType];
		}
		return dir * (style == 0).ToDirectionInt();
	}
	bool isDrawingFlipped;
	public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
		isDrawingFlipped = GetFacingDirection(i, j) < 0;
		if (isDrawingFlipped) spriteEffects ^= SpriteEffects.FlipHorizontally;
	}
	public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
		if (isDrawingFlipped) tileFrameX += (short)(36 - (tileFrameX % (3 * 18)) * 2);
	}
}