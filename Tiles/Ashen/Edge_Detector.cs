using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Edge_Detector : ModTile, IAshenWireTile {
		public sealed override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(this.DropTileItem)
			.RegisterItem();
		}
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = false;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.DrawTileInSolidLayer[Type] = true;
			TileID.Sets.CanPlaceNextToNonSolidTile[Type] = true;
			AddMapEntry(FromHexRGB(0xe47430), CreateMapEntryName());

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void HitWire(int i, int j) {
			if (Ashen_Wire_Data.HittingAshenWires) UpdatePowerState(i, j, IsPowered(i, j));
		}
		public bool IsPowered(int i, int j) => Main.tile[i, j].Get<Ashen_Wire_Data>().AnyPower;
		public void UpdatePowerState(int i, int j, bool powered) {
			if (Main.tile[i, j].TileFrameX.TrySet(powered.Mul<short>(18))) {
				Wiring.TripWire(i, j, 1, 1);
				NetMessage.SendData(MessageID.TileSquare, Main.myPlayer, -1, null, i, j, 1, 1);
			}
		}
	}
}
