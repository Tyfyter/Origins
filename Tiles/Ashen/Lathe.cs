using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Lathe : ModTile {
		public static int ID { get; private set; }
		public override void Load() {
			new TileItem(this)
			.WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.Sawmill);
				item.createTile = Type;
				//item.rare++;
				item.value += Item.buyPrice(gold: 1);
			}).RegisterItem();
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileNoAttach[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.addTile(Type);

			AddMapEntry(FromHexRGB(0x0A3623), this.GetTileItem().DisplayName);
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
			ID = Type;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
	}
}
