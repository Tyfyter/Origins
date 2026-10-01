using Origins.Core;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Door_Stopper : OriginTile, IAshenTile, IAshenWireTile {
		public virtual Color MapColor => FromHexRGB(0x5e3f2c);
		public override void Load() => new TileItem(this).WithExtraStaticDefaults(this.DropTileItem).RegisterItem();
		public override void SetStaticDefaults() {
			BlockTileInteractions.TilesBlockInteraction[Type] = true;
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.GeneralPlacementTiles[Type] = false;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
			OriginsSets.Tiles.LavaAboveEmitsSmoke[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.SetOriginBottomCenter();
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(1);
			this.SetAnimationHeight();
			TileObjectData.addTile(Type);
			AddMapEntry(FromHexRGB(0x5e3f2c), this.GetTileItem().DisplayName);

			MinPick = 210;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void HitWire(int i, int j) {
			if (Ashen_Wire_Data.HittingAshenWires) UpdatePowerState(i, j, IsPowered(i, j));
		}
		public bool IsPowered(int i, int j) => Main.tile[i, j].Get<Ashen_Wire_Data>().AnyPower;
		public void UpdatePowerState(int i, int j, bool powered) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY.TrySet(powered.Mul<short>(18))) {
				int sub = (tile.TileFrameX / 18) % 2;
				Main.tile[i + (sub == 0).ToDirectionInt(), j].TileFrameY = tile.TileFrameY;
				NetMessage.SendTileSquare(-1, i - sub, j, 2, 1);
			}
		}
	}
}
