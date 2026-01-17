using Origins.Core;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.SpecialChest;
using static Origins.Core.SpecialChest.ChestData;

namespace Origins.Tiles.Ashen {
	public class Medium_Storage_Container : ModTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this));
		}
		public override void SetStaticDefaults() {
			Main.tileSpelunker[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 12000;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileOreFinderPriority[Type] = 500;
			TileID.Sets.HasOutlines[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(1, 3);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.SetHeight(4);
			TileObjectData.newTile.AnchorInvalidTiles = [127];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.addTile(Type);
			_ = DefaultContainerName(0, 0);
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = false;
			for (int i = TileObjectData.GetTileData(Type, 0).RandomStyleRange; i > 0; i--) {
				AddMapEntry(new(36, 33, 31), CreateMapEntryName(), MapChestName);
			}
			AdjTiles = [TileID.Containers];
			DustType = Ashen_Biome.DefaultTileDust;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
			OriginsSets.Tiles.ChestSoundOverride[Type] = (Origins.Sounds.MetalBoxOpen, default);
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			switch ((tile.TileFrameX / 18) / 4) {
				case 0:
				case 1:
				if ((tile.TileFrameY / 18) % 2 != 0) height = -1600;
				else if (tile.TileFrameY != 0) y -= 4;
				break;

				case 2:
				if (tile.TileFrameY != 0) height = -1600;
				break;
			}
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void PlaceInWorld(int i, int j, Item item) {
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out i, out j);
			new Set_Special_Chest_Action(new(i, j), new Medium_Storage_Container_Data()).Perform();
		}
		public override bool RightClick(int i, int j) {
			SpecialChest.OpenChest(i, j);
			return true;
		}
		public record class Medium_Storage_Container_Data() : Storage_Container_Data() {
			public override int Capacity => 160;
			public override int Width => 4;
			public override int Height => 4;
			protected override bool IsValidSpot(Point position) => Main.tile[position].TileIsType(ModContent.TileType<Medium_Storage_Container>());
		}
	}
}
