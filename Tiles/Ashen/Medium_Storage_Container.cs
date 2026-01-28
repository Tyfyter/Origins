using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.SpecialChest;

namespace Origins.Tiles.Ashen {
	public class Medium_Storage_Container : ModSpecialChest {
		public override ChestData CreateChestData() => new Medium_Storage_Container_Data();
		public override void Load() {
			Mod.AddContent(new TileItem(this));
		}
		public override void ModifyTileData() {
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.SetHeight(4);
			TileObjectData.newTile.RandomStyleRange = 3;
			for (int i = TileObjectData.newTile.RandomStyleRange; i > 0; i--) {
				AddMapEntry(new(36, 33, 31), CreateMapEntryName(), MapChestName);
			}
			AdjTiles = [TileID.Containers];
			DustType = Ashen_Biome.DefaultTileDust;
			//OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
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
		public record class Medium_Storage_Container_Data() : Storage_Container_Data() {
			public override int Capacity => 160;
			public override int Width => 4;
			public override int Height => 4;
			protected internal override bool IsValidSpot(Point position) => Main.tile[position].TileIsType(ModContent.TileType<Medium_Storage_Container>());
		}
	}
}
