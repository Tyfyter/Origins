using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen;
public class Conveyor_Scooper : ModTile {
	const short dir_size = 2 * 3 * 18;
	public override void Load() {
		new TileItem(this)
		.WithExtraStaticDefaults(item => ItemID.Sets.DisableAutomaticPlaceableDrop[item.type] = true)
		.RegisterItem();
	}
	public override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileLighted[Type] = true;
		TileID.Sets.DrawTileInSolidLayer[Type] = true;
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.newTile.SetHeight(3);
		TileObjectData.newTile.SetOriginBottomCenter();
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(2);

		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.newAlternate.Origin = new(1, 0);
		TileObjectData.newAlternate.AnchorTop = new(AnchorType.SolidBottom, 0, 3);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
		TileObjectData.addAlternate(1);
		TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
		TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
		TileObjectData.newAlternate.Origin = new(1, 0);
		TileObjectData.newAlternate.AnchorTop = new(AnchorType.SolidBottom, 0, 3);
		TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
		TileObjectData.addAlternate(3);
		TileObjectData.addTile(Type);
		AddMapEntry(new Color(194, 69, 12), CreateMapEntryName());
		DustType = Ashen_Biome.DefaultTileDust;
	}
	public override void HitWire(int i, int j) {
		if (!Ashen_Wire_Data.HittingAshenWires) {
			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			for (int y = 0; y < data.Height; y++) {
				for (int x = 0; x < data.Width; x++) {
					Tile tile = Main.tile[left + x, top + y];
					if (tile.TileType != Type) continue;
					tile.TileFrameX += dir_size;
					tile.TileFrameX %= dir_size * 2;
				}
			}
		}
	}
}