using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
namespace Origins.Tiles.Defiled {
	public class Defiled_Stone : OriginTile, IDefiledTile {
        public string[] Categories => [
            "Stone"
        ];
        public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Defiled_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Defiled_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.Stone[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
            Main.tileMerge[Type][TileID.Stone] = true;
            for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
            }*/
			//ItemDrop = ItemType<Defiled_Stone_Item>();
			AddMapEntry(new Color(200, 200, 200));
			//SetModTree(Defiled_Tree.Instance);
			mergeID = TileID.Stone;
			MinPick = 65;
			MineResist = 2;
			AddDefiledTile();
			HitSound = Origins.Sounds.DefiledIdle;
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
	}
	public class Defiled_Stone_Item : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Defiled_Stone>());
		}
		public LocalizedText PageTextMain => WikiPageExporter.GetDefaultMainPageText(this)
			.WithFormatArgs(65,
			Language.GetText("Mods.Origins.Generic.Defiled_Wastelands"),
			"Stone"
		);
	}
}
