using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
    public class Wrycoral : OriginTile {
        public string[] Categories => [
            "Plant"
        ];
        public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			Main.tileNoFail[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(128, 128, 128), name);

			HitSound = SoundID.Grass;
			DustType = DustID.WhiteTorch;

			RegisterItemDrop(ItemType<Wrycoral_Item>());
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileType = TileID.Cobweb;
			WorldGen.TileFrame(i, j, resetFrame, noBreak);
			tile.TileType = Type;
			return false;
		}
	}
    public class Wrycoral_Item : ModItem {
        public override void SetStaticDefaults() {
            Item.ResearchUnlockCount = 25;
        }
        public override void SetDefaults() {
			Item.maxStack = Item.CommonMaxStack;
			Item.width = 12;
			Item.height = 14;
			Item.value = Item.sellPrice(copper: 20);
		}
    }
}
