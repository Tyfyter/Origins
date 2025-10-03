using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Fungarust : OriginTile, IAshenTile {
		private const int FrameWidth = 18; // A constant for readability and to kick out those magic numbers
		public string[] Categories => [
			"Plant"
		];
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(128, 128, 128), name);

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			TileObjectData.newTile.AnchorValidTiles = [
				TileType<Ashen_Grass>(),
				TileType<Ashen_Jungle_Grass>(),
				TileType<Tainted_Stone>()
			];
			TileObjectData.newTile.AnchorAlternateTiles = [
				TileID.ClayPot,
				TileID.PlanterBox
			];
			TileObjectData.addTile(Type);

			HitSound = SoundID.Grass;
			DustType = DustID.Ash;
			RegisterItemDrop(ItemType<Fungarust_Item>());
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
	public class Fungarust_Item : MaterialItem {
		public override int Value => Item.sellPrice(copper: 10);
		public override bool Hardmode => false;
	}
}
