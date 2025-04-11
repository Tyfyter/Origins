using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Limestone {
	public class Limestone_Stalactite : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new(203, 194, 149));

			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.CoordinateHeights = new int[3] {
				16,
				16,
				16
			};

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.Origin = new();
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 1;
			TileObjectData.addTile(Type);

			DustType = DustID.Sand;
		}
	}
	public class Limestone_Stalagmite : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new(203, 194, 149));

			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.CoordinateHeights = new int[3] {
				16,
				16,
				16
			};

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Direction = TileObjectDirection.None;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 1;
			TileObjectData.addTile(Type);

			DustType = DustID.Sand;
		}
	}
	public class Limestone_Stalactite_Fake : Limestone_Stalactite {
		public override string Texture => base.Texture[..^"_Fake".Length];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			FlexibleTileWand.RubblePlacementMedium.SetupRubblemakerClone<Limestone_Item>(this, 0, 1, 2);
		}
	}
	public class Limestone_Stalagmite_Fake : Limestone_Stalagmite {
		public override string Texture => base.Texture[..^"_Fake".Length];
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			FlexibleTileWand.RubblePlacementMedium.SetupRubblemakerClone<Limestone_Item>(this, 0, 1, 2);
		}
	}
}
