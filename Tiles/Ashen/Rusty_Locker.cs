using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Items.Weapons.Ammo;
using Origins.Reflection;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Rusty_Locker : ModChest, ICustomSizeContainer {
		public int Width { get; } = 2;
		public int Height { get; } = 3;
		public override void Load() {
			new TileItem(this)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddRecipeGroup(ALRecipeGroups.SilverBars)
				.AddIngredient<Scrap>(10)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			TileID.Sets.BasicChest[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = false;
			AddMapEntry(new(36, 33, 31), CreateMapEntryName(), MapChestName);
			AdjTiles = [TileID.Containers];
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetBookcaseCollision;
			OriginsSets.Tiles.ChestSoundOverride[Type] = (Origins.Sounds.MetalCreakOpen.WithVolume(0.3f), Origins.Sounds.MetalCreakClose.WithVolume(0.3f));
		}
		static void OffsetBookcaseCollision(Tile tile, ref float y, ref int height) {
			if (tile.TileFrameY != 0) height = -1600;
		}
		public override void ModifyTileData() {
			TileObjectData.newTile.SetHeight(3);
			TileObjectData.newTile.Origin = new(0, 2);
		}
		public override bool IsLockedChest(int i, int j) => false;
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			Point key = new(i, j);
			TileObjectData data = TileObjectData.GetTileData(drawData.tileCache);
			TileUtils.GetMultiTileTopLeft(i, j, data, out key.X, out key.Y);
			Dictionary<Point, int> _chestPositions = TileDrawingMethods._chestPositions.GetValue(Main.instance.TilesRenderer);
			if (!_chestPositions.TryGetValue(key, out int value)) {
				value = Chest.FindChest(key.X, key.Y);
				_chestPositions[key] = value;
			}
			if (value != -1) {
				drawData.addFrY = (short)(data.CoordinateFullHeight * (int)(Main.chest[value].frameCounter / 2.5f));
			}
		}
		public override void GetTopLeft(int i, int j, in Tile tile, out int left, out int top) {
			TileObjectData data = TileObjectData.GetTileData(tile);
			TileUtils.GetMultiTileTopLeft(i, j, data, out left, out top);
		}
	}
}
