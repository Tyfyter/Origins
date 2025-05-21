using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Origins.TileExtenstions;
using Origins.Journal;

namespace Origins.Tiles.Brine {
	[LegacyName("Sulphur_Stone", "Dolomite")]
	public class Baryte : OriginTile {
        public string[] Categories => [
            "Stone"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
			Origins.TileBlocksMinecartTracks[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.GeneralPlacementTiles[Type] = false;
			AddMapEntry(new Color(18, 73, 56));
			MinPick = 195;
			HitSound = SoundID.Tink;
			DustType = DustID.GreenMoss;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			DoFraming(i, j, resetFrame, map: [(Type, 1), (TileID.Mud, 2)], ExtraTileBlending);
			return false;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
		public override void RandomUpdate(int i, int j) {
			if (!Framing.GetTileSafely(i, j + 1).HasTile && Framing.GetTileSafely(i, j + 1).LiquidAmount >= 255) {
				if (TileObject.CanPlace(i, j + 1, WorldGen.genRand.NextBool(3) ? TileType<Brineglow>() : TileType<Underwater_Vine>(), 0, 0, out TileObject objectData, false, checkStay: true)) {
					objectData.style = 0;
					objectData.alternate = 0;
					objectData.random = 0;
					TileObject.Place(objectData);
				}
			}
			Brine_Leaf_Clover_Tile.TryGrowOnTile(i, j);
		}
	}
	[LegacyName("Sulphur_Stone_Item", "Dolomite_Item")]
	public class Baryte_Item : ModItem, IJournalEntrySource {
		public string EntryName => "Origins/" + typeof(Baryte_Entry).Name;
		public class Baryte_Entry : JournalEntry {
			public override string TextKey => "Baryte";
			public override JournalSortIndex SortIndex => new("Brine_Pool_And_Lost_Diver", 3);
		}
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Baryte>());
		}
	}
}
