using Origins.Core;
using Origins.World.BiomeData;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.SpecialChest;

namespace Origins.Tiles.Ashen {
	public class Medium_Storage_Container : ModSpecialChest {
		public override ChestData CreateChestData() => new Medium_Storage_Container_Data();
		public override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(this.DropTileItem)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(TileItem.Get<Small_Storage_Container>(), 4)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
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
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = false;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
			OriginsSets.Tiles.ChestSoundOverride[Type] = (Origins.Sounds.MetalBoxOpen, default);
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
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
		public override void MouseOver(int i, int j) => SpecialChest.MouseOver(i, j);
		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			if (player.cursorItemIconText == "") {
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = ItemID.None;
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
