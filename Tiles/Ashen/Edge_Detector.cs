using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Items.Tools.Wiring.Ashen_Wire_Data;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Edge_Detector : ModTile, IAshenWireTile {
		public sealed override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(this.DropTileItem)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Lens)
				.AddIngredient(ItemID.Wire, 8)
				.AddIngredient<Scrap>(12)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
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
			AddMapEntry(FromHexRGB(0xe47430), this.GetTileItem().DisplayName);

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void HitWire(int i, int j) {
			if (HittingAshenWires) UpdatePowerState(i, j, IsPowered(i, j));
		}
		public bool IsPowered(int i, int j) => Main.tile[i, j].Get<Ashen_Wire_Data>().AnyPower;
		public void UpdatePowerState(int i, int j, bool powered) {
			if (Main.tile[i, j].TileFrameX.TrySet(powered.Mul<short>(18))) {
				using HittingWiresOverride _ = new(false);
				OriginSystem.QueueTripWire(i, j);
				NetMessage.SendData(MessageID.TileSquare, Main.myPlayer, -1, null, i, j, 1, 1);
			}
		}
	}
}
