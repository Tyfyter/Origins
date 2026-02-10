using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Consumables;
using Origins.Tiles.Ashen;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.Walls;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles {
	public class OriginsGlobalTile : GlobalTile {
		static Dictionary<int, AutoLoadingAsset<Texture2D>> stalactiteTextures;
		public override void SetStaticDefaults() {
			stalactiteTextures = new() {
				[ModContent.TileType<Brown_Ice>()] = "Origins/Tiles/Ashen/Brown_Icicle",
				[ModContent.TileType<Defiled_Ice>()] = "Origins/Tiles/Defiled/Defiled_Icicle",
				[ModContent.TileType<Primordial_Permafrost>()] = "Origins/Tiles/Riven/Primordial_Permafrost_Icicle"
			};
		}
		public override void Unload() {
			stalactiteTextures = null;
		}
		public override void PlaceInWorld(int i, int j, int type, Item item) {
			if (TileLoader.GetTile(type) is IAshenWireTile ashenWireTile) {
				ashenWireTile.UpdatePowerState(i, j, ashenWireTile.IsPowered(i, j));
			}
			if (TileObjectData.GetTileData(Main.tile[i, j]) is TileObjectData data) {
				TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
				for (int x = -1; x <= data.Width; x++) {
					Tile tile = Main.tile[left + x, top + data.Height];
					if (tile.HasTile && Catwalk.Catwalks[tile.TileType]) WorldGen.TileFrame(left + x, top + data.Height);
				}
			}
		}
		public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged) {
			//if (Main.tile[i, j - 1].TileType == Defiled_Altar.ID && type != Defiled_Altar.ID) return false;
			//if (Main.tile[i, j - 1].TileType == Riven_Altar.ID && type != Riven_Altar.ID) return false;
			return true;
		}
		public override void MouseOver(int i, int j, int type) {
			Point targetPos = new(i, j);
			Tile tile = Framing.GetTileSafely(targetPos);
			if (tile.HasTile && Main.tileContainer[tile.TileType]) {
				if (tile.TileFrameX % 36 != 0) {
					targetPos.X--;
				}
				if (tile.TileFrameY != 0) {
					targetPos.Y--;
				}
				OriginSystem originSystem = ModContent.GetInstance<OriginSystem>();
				if (originSystem.VoidLocks.TryGetValue(targetPos, out Guid owner)) {
					if (owner == Main.LocalPlayer.GetModPlayer<OriginPlayer>().guid) {
						Main.LocalPlayer.cursorItemIconID = ItemID.ShadowKey;
						if (Main.LocalPlayer.tileInteractAttempted) {
							if (Main.LocalPlayer.HasItemInInventoryOrOpenVoidBag(ItemID.ShadowKey)) {
								originSystem.VoidLocks.Remove(targetPos);
							} else {
								Main.LocalPlayer.tileInteractAttempted = false;
							}
						}
					} else {
						Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<Void_Lock>();
						Main.LocalPlayer.tileInteractAttempted = false;
					}
				}
			}
		}
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
			Tile tile = Main.tile[i, j];
			if (tile.WallType == ModContent.WallType<Baryte_Wall>()) {
				tile.LiquidAmount = 255;
				tile.LiquidType = LiquidID.Water;
			}
		}
		public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak) {
			switch (type) {
				case TileID.Plants:
				case TileID.CorruptPlants:
				case TileID.CrimsonPlants:
				case TileID.HallowedPlants:
				ConvertPlantsByAnchor(ref Main.tile[i, j].TileType, Main.tile[i, j + 1].TileType);
				return true;
			}
			if (type == ModContent.TileType<Defiled_Foliage>() || type == ModContent.TileType<Riven_Foliage>() || type == ModContent.TileType<Ashen_Foliage>()) {
				ConvertPlantsByAnchor(ref Main.tile[i, j].TileType, Main.tile[i, j + 1].TileType);
			}
			return true;
		}
		public static void ConvertPlantsByAnchor(ref ushort plant, ushort anchor) {
			switch (anchor) {
				case TileID.Grass:
				plant = TileID.Plants;
				return;
				case TileID.CorruptGrass:
				plant = TileID.CorruptPlants;
				return;
				case TileID.CrimsonGrass:
				plant = TileID.CrimsonPlants;
				return;
				case TileID.HallowedGrass:
				plant = TileID.HallowedPlants;
				return;
			}
			if (anchor == ModContent.TileType<Defiled_Grass>()) plant = (ushort)ModContent.TileType<Defiled_Foliage>();
			else if (anchor == ModContent.TileType<Riven_Grass>()) plant = (ushort)ModContent.TileType<Riven_Foliage>();
			else if (anchor == ModContent.TileType<Ashen_Grass>()) plant = (ushort)ModContent.TileType<Ashen_Foliage>();
		}
		public static bool GetStalactiteTexture(int i, int j, int frameY, out AutoLoadingAsset<Texture2D> texture) {
			int direction = -1;
			if (frameY is 36 or 54 or 90) {
				direction = 1;
			}
			int baseY = j;
			while (Main.tile[i, baseY].TileIsType(TileID.Stalactite)) {
				baseY += direction;
			}
			return stalactiteTextures.TryGetValue(Main.tile[i, baseY].TileType, out texture);
		}
		public override void RandomUpdate(int i, int j, int type) {
			if (i > WorldGen.beachDistance && i < Main.maxTilesX - WorldGen.beachDistance && Ocotillo.data.AnchorValidTiles.Contains(type)) {
				bool hasLimestone = false;
				int limestone = ModContent.TileType<Limestone.Limestone>();
				for (int y = 0; y < 10 && !hasLimestone; y++) {
					if (Framing.GetTileSafely(i, j + y).TileIsType(limestone)) hasLimestone = true;
				}
				if (WorldGen.genRand.NextBool(hasLimestone ? 80 : 200)) {
					ushort ocotillo = (ushort)ModContent.TileType<Ocotillo>();
					if (TileExtenstions.CanActuallyPlace(i, j - 1, ocotillo, 0, 1, out TileObject objectData, onlyCheck: false)) {
						if (TileObject.Place(objectData)) WorldGen.SquareTileFrame(i, j - 1);
					}
				}
			}
		}
		public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (type == TileID.Stalactite) {
				if (GetStalactiteTexture(i, j, drawData.tileFrameY, out AutoLoadingAsset<Texture2D> texture)) {
					if (texture.IsLoaded) {
						drawData.tileFrameY %= 54;
						drawData.drawTexture = texture;
					} else {
						texture.LoadAsset();
					}
				}
			}
		}
		public override void FloorVisuals(int type, Player player) {
		}
		public override void ReplaceTile(int i, int j, int type, int targetType, int targetStyle) {
			if (targetType == ModContent.TileType<Small_Storage_Container>()) {
				int randomStyle = Main.rand.Next(3);
				TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
				TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
				for (int x = 0; x < data.Width; x++) {
					for (int y = 0; y < data.Height; y++) {
						Tile tile = Main.tile[left + x, top + y];
						tile.TileFrameX = (short)((x + randomStyle * 2) * 18);
					}
				}
			}
		}
	}
}
