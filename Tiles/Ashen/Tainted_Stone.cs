using Origins.Dev;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Tainted_Stone : OriginTile, IAshenTile {
		public override string Texture => typeof(Defiled_Stone).GetDefaultTMLName();
		public string[] Categories => [
            "Stone"
        ];
        public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.Stone[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			/*Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
            Main.tileMerge[Type][TileID.Stone] = true;
            for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
            }*/
			//ItemDrop = ItemType<Defiled_Stone_Item>();
			AddMapEntry(new Color(255, 200, 200));
			//SetModTree(Defiled_Tree.Instance);
			//mergeID = TileID.Stone;
			MinPick = 65;
			MineResist = 2;
			HitSound = Origins.Sounds.DefiledIdle;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid && Main.rand.NextBool(250)) {
				above.SetToType((ushort)TileType<Fungarust>(), Main.tile[i, j].TileColor);
				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
	public class Tainted_Stone_Item : ModItem, ICustomWikiStat {
		public override string Texture => typeof(Defiled_Stone_Item).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.StoneBlock, Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Tainted_Stone>());
		}
		public LocalizedText PageTextMain => WikiPageExporter.GetDefaultMainPageText(this)
			.WithFormatArgs(65,
			Language.GetText("Mods.Origins.Generic.Ashen_Factory"),
			"Stone"
		);
	}
}
