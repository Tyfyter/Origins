using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Dev;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
    public class Soulspore : OriginTile, IDefiledTile {
        public string[] Categories => [
			WikiCategories.Plant
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

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			TileObjectData.newTile.AnchorValidTiles = [
				TileType<Defiled_Grass>(),
				TileType<Defiled_Stone>(),
				TileType<Defiled_Jungle_Grass>()
			];
			TileObjectData.addTile(Type);

			HitSound = SoundID.Grass;
			DustType = DustID.Marble;
			RegisterItemDrop(ItemType<Soulspore_Item>());
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 0) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Main.tile[i, j].TileFrameX = (short)(18 * (i % 3));
			return true;
		}
	}
	public class Soulspore_Item : MaterialItem {
		public override int Value => Item.sellPrice(copper: 10);
		public override bool Hardmode => false;
	}
}
