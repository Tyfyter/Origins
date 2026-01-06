using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
    public class Acetabularia : OriginTile, IRivenTile {
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
			AddMapEntry(new Color(0, 175, 200), name);

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			TileObjectData.newTile.AnchorValidTiles = [
				TileType<Spug_Flesh>(),
				TileType<Riven_Grass>(),
				TileType<Riven_Jungle_Grass>(),
			];
			TileObjectData.addTile(Type);

			HitSound = SoundID.Grass;
			DustType = DustID.Marble;
			RegisterItemDrop(ItemType<Acetabularia_Item>());
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
	public class Acetabularia_Item : MaterialItem {
		public override int Value => Item.sellPrice(copper: 10);
		public override bool Hardmode => false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			OriginsSets.Items.EvilMaterialAchievement[Type] = true;
		}
	}
}
