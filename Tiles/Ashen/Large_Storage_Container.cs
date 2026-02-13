using Origins.Core;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.SpecialChest;

namespace Origins.Tiles.Ashen {
	public class Large_Storage_Container : ModSpecialChest {
		public override ChestData CreateChestData() => new Large_Storage_Container_Data();
		public override void Load() {
			new TileItem(this)
			.WithExtraStaticDefaults(this.DropTileItem)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddRecipeGroup(RecipeGroupID.IronBar, 25)
				.AddIngredient<Scrap>(80)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
		}
		public override void ModifyTileData() {
			TileObjectData.newTile.Origin = new Point16(4, 4);
			TileObjectData.newTile.Width = 9;
			TileObjectData.newTile.SetHeight(5);
			TileObjectData.newTile.RandomStyleRange = 2;
			for (int i = TileObjectData.newTile.RandomStyleRange; i > 0; i--) {
				AddMapEntry(new(36, 33, 31), CreateMapEntryName(), MapChestName);
			}
			AdjTiles = [TileID.Containers];
			DustType = Ashen_Biome.DefaultTileDust;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = false;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
			OriginsSets.Tiles.ChestSoundOverride[Type] = (Origins.Sounds.MetalCreakOpen.WithVolume(0.3f), Origins.Sounds.MetalCreakClose.WithVolume(0.3f));
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			if (tile.TileFrameY != 0) height = -1600;
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
		public record class Large_Storage_Container_Data() : Storage_Container_Data() {
			public override int Capacity => 450;
			public override int Width => 9;
			public override int Height => 5;
			protected internal override bool IsValidSpot(Point position) => Main.tile[position].TileIsType(ModContent.TileType<Large_Storage_Container>());
		}
	}
}
