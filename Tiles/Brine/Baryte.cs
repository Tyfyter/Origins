using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	[LegacyName("Sulphur_Stone", "Dolomite")]
	public class Baryte : OriginTile {
        public string[] Categories => [
            "Stone"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMerge[Type][TileID.Mud] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			AddMapEntry(new Color(18, 73, 56));
			//mergeID = TileID.Mud;
			MinPick = 195;
			HitSound = SoundID.Dig;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
		public override void RandomUpdate(int i, int j) {
			if (!Framing.GetTileSafely(i, j + 1).HasTile) {
				if (TileObject.CanPlace(i, j + 1, WorldGen.genRand.NextBool(3) ? TileType<Brineglow>() : TileType<Underwater_Vine>(), 0, 0, out TileObject objectData, false, checkStay: true)) {
					objectData.style = 0;
					objectData.alternate = 0;
					objectData.random = 0;
					TileObject.Place(objectData);
				}
			}
		}
	}
	[LegacyName("Sulphur_Stone_Item", "Dolomite_Item")]
	public class Baryte_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Baryte>());
		}
	}
}
