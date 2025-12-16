using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.Tiles.Dev;
using PegasusLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Core.Structures {
	public class PaintTile : SerializableTileDescriptor {
		readonly TileDescriptor[] descriptors = new TileDescriptor[PaintID.IlluminantPaint];
		public override void Load() {
			AddSerializer(tile => tile.TileColor == PaintID.None ? null : $"PaintTile({PaintID.Search.GetName(tile.TileColor)})");
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			if (!parameters[0].EndsWith("Paint")) parameters[0] += "Paint";
			byte paintType = (byte)PaintID.Search.GetId(parameters[0]);
			return descriptors[paintType] ??= new((_, _, i, j) => {
				Tile tile = Main.tile[i, j];
				tile.TileColor = paintType;
			}, Parts: [originalText]);
		}
	}
	public class PlaceWire : SerializableTileDescriptor {
		public override void Load() {
			AddSerializer(tile => {
				StringBuilder builder = new();
				foreach (WireMode mode in ModContent.GetContent<WireMode>()) {
					(int i, int j) = tile.GetTilePosition();
					if (mode.GetWire(i, j)) {
						if (builder.Length > 0) builder.Append(',');
						builder.Append(mode.Mod is Origins ? mode.Name : mode.FullName);
					}
				}
				if (builder.Length <= 0) return null;
				return $"PlaceWire({builder})";
			});
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			WireMode[] wireModes = parameters.Select(n => ModContent.TryFind(n, out WireMode wireMode) ? wireMode : ModContent.Find<WireMode>("Origins/" + n)).ToArray();
			return new((_, _, i, j) => {
				for (int k = 0; k < wireModes.Length; k++) {
					wireModes[k].SetWire(i, j, true);
				}
			}, Parts: [originalText]);
		}
		public override IEnumerable<(string name, Color color)> GetDisplayLayers(string[] parameters) {
			foreach (WireMode mode in ModContent.GetContent<WireMode>()) {
				if (parameters.Contains(mode.Name) || parameters.Contains(mode.FullName)) {
					yield return (mode.FullName, mode.MiniWireMenuColor);
				}
			}
		}
		public override void Draw(SpriteBatch spriteBatch, Rectangle destination, Color color, bool[,] map, int x, int y, string name) {
			if (ModContent.Find<WireMode>(name["PlaceWire_".Length..]).IsExtra) {
				spriteBatch.Draw(
					TextureAssets.WireUi[11].Value,
					destination,
					color
				);
			} else {
				Rectangle wireFrame = new(0, 0, 16, 16);
				if (map.GetIfInRange(x, y - 1)) wireFrame.X += 18;
				if (map.GetIfInRange(x + 1, y)) wireFrame.X += 36;
				if (map.GetIfInRange(x, y + 1)) wireFrame.X += 72;
				if (map.GetIfInRange(x - 1, y)) wireFrame.X += 144;
				spriteBatch.Draw(
					TextureAssets.Projectile[ProjectileID.WireKite].Value,
					destination,
					wireFrame,
					color
				);
			}
		}
	}
	public class ConditionalTile : PlaceTile {
		public override void Load() { }
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			Accumulator<HashSet<char>, bool> condition = null;
			bool isInverted = false;
			for (int i = 0; i < parameters[0].Length; i++) {
				char socket = parameters[0][i];
				switch (socket) {
					case '!':
					isInverted ^= true;
					break;
					default:
					condition += (HashSet<char> connectedSockets, ref bool canGenerate) => {
						if (!canGenerate) return;
						canGenerate = connectedSockets.Contains(socket) == isInverted;
					};
					break;
				}
			}
			CachedTileType type = new(parameters[1]);
			return new((_, connectedSockets, i, j) => {
				if (condition.Accumulate(connectedSockets, true)) {
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = type;
				}
			}, Parts: [originalText]);
		}
	}
	public class PlaceTile : SerializableTileDescriptor {
		readonly ConcurrentDictionary<string, TileDescriptor> descriptors = [];
		protected static Type GetDescriptorType(Tile tile) {
			if (Main.tileContainer[tile.TileType]) return typeof(PlaceChest);
			if (TileObjectData.GetTileData(tile) is not null) return typeof(PlaceSpecialTile);
			if (Main.tileFrameImportant[tile.TileType]) return typeof(PlaceFramedTile);
			return typeof(PlaceTile);
		}
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile && GetDescriptorType(tile) == typeof(PlaceTile)) {
					if (Main.tileFrameImportant[tile.TileType]) return null;
					if (TileObjectData.GetTileData(tile) is not null) return null;
					return $"PlaceTile({TileID.Search.GetName(tile.TileType)})";
				}
				return null;
			});
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			return descriptors.GetOrAdd(parameters[0], parameter => {
				CachedTileType type = new(parameter);
				return new((_, _, i, j) => {
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = type;
				}, Parts: [originalText]);
			});
		}
		public class CachedTileType(string name) {
			readonly string name = name;
			ushort? id;
			public ushort Value => id ??= (ushort)TileID.Search.GetId(name);
			public static implicit operator ushort(CachedTileType type) => type.Value;
		}
		public override IEnumerable<(string name, Color color)> GetDisplayLayers(string[] parameters) {
			yield return (parameters[0].ToString(), Color.White);
		}
		public override void Draw(SpriteBatch spriteBatch, Rectangle destination, Color color, bool[,] map, int x, int y, string name) {
			if (!int.TryParse(name["PlaceTile_".Length..], out int tileID)) tileID = new CachedTileType(name["PlaceTile_".Length..]);
			spriteBatch.Draw(
				TextureAssets.Tile.GetIfInRange(tileID, TextureAssets.MagicPixel).Value,
				destination,
				new(9 * 18, 3 * 18, 16, 16),
				color
			);
		}
	}
	public class PlaceFramedTile : PlaceTile {
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile && GetDescriptorType(tile) == typeof(PlaceFramedTile)) {
					if (tile.TileType == ModContent.TileType<Room_Socket_Marker>()) return "Empty";
					if (TileObjectData.GetTileData(tile) is not null) return null;
					return $"PlaceFramedTile({TileID.Search.GetName(tile.TileType)},{tile.TileFrameX},{tile.TileFrameY})";
				}
				return null;
			});
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			CachedTileType type = new(parameters[0]);
			short frameX = short.Parse(parameters[1]);
			short frameY = short.Parse(parameters[2]);
			return new((_, _, i, j) => {
				Tile tile = Main.tile[i, j];
				tile.HasTile = true;
				tile.TileType = type;
				tile.TileFrameX = frameX;
				tile.TileFrameY = frameY;
			}, Parts: [originalText]);
		}
		public override IEnumerable<(string name, Color color)> GetDisplayLayers(string[] parameters) {
			yield return ($"{parameters[0]};{parameters[1]};{parameters[2]}", Color.White);
		}
		public override void Draw(SpriteBatch spriteBatch, Rectangle destination, Color color, bool[,] map, int x, int y, string name) {
			string[] parts = name["PlaceFramedTile_".Length..].Split(';');
			if (!int.TryParse(parts[0], out int tileID)) tileID = new CachedTileType(parts[0]);
			if (!int.TryParse(parts[1], out int frameX)) return;
			if (!int.TryParse(parts[2], out int frameY)) return;
			spriteBatch.Draw(
				TextureAssets.Tile.GetIfInRange(tileID, TextureAssets.MagicPixel).Value,
				destination,
				new(frameX, frameY, 16, 16),
				color
			);
		}
	}
	public class PlaceSpecialTile : PlaceTile {
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile && GetDescriptorType(tile) == typeof(PlaceSpecialTile)) {
					TileObjectData data = TileObjectData.GetTileData(tile);
					(int i, int j) = tile.GetTilePosition();
					TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
					if (i != left + data.Origin.X || j != top + data.Origin.Y) return "Empty";
					int style = 0;
					int alternate = 0;
					TileObjectData.GetTileInfo(tile, ref style, ref alternate);
					string direction = "";
					switch (data.Direction) {
						case TileObjectDirection.PlaceLeft:
						direction = ",-1";
						break;
						case TileObjectDirection.PlaceRight:
						direction = ",1";
						break;
					}
					string styleText = (style == 0 && string.IsNullOrEmpty(direction)) ? "" : $",{style}";
					return $"{Name}({TileID.Search.GetName(tile.TileType)}{styleText}{direction})";
				}
				return null;
			});
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			CachedTileType type = new(parameters[0]);
			int style = parameters.Length > 1 ? int.Parse(parameters[1]) : 0;
			int dir = 0;
			if (parameters.Length > 2) _ = int.TryParse(parameters[2], out dir);
			return new((_, _, i, j) => PlaceTile(i, j, type, style, dir, parameters), Parts: [originalText]);
		}
		public virtual void PlaceTile(int i, int j, CachedTileType type, int style, int dir, string[] parameters) {
			TileObject.CanPlace(i, j, type, style, dir, out TileObject objectData);
			TileObject.Place(objectData);
			TileObjectData.CallPostPlacementPlayerHook(i, j, type, style, dir, objectData.alternate, objectData);
			TileLoader.GetTile(type)?.PlaceInWorld(i, j, null);

			TileObjectData data = TileObjectData.GetTileData(Main.tile[i, j]);
			TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
			Point pos = default;
			for (int y = 0; y < data.Height; y++) {
				pos.Y = top + y;
				for (int x = 0; x < data.Width; x++) {
					pos.X = left + x;
					Structure.ignoreEmpty.Add(pos);
				}
			}
		}
		public override IEnumerable<(string name, Color color)> GetDisplayLayers(string[] parameters) {
			yield return ($"{parameters[0]};{parameters.GetIfInRange(1)}", Color.White);
		}
		public override void Draw(SpriteBatch spriteBatch, Rectangle destination, Color color, bool[,] map, int x, int y, string name) {
			string[] parts = name["PlaceSpecialTile_".Length..].Split(';');
			if (!int.TryParse(parts[0], out int tileID)) tileID = new CachedTileType(parts[0]);
			_ = int.TryParse(parts[1], out int style);
			Texture2D texture = TextureAssets.Tile.GetIfInRange(tileID, TextureAssets.MagicPixel).Value;
			TileObjectData data = TileObjectData.GetTileData(tileID, style);
			x = destination.X - data.Origin.X * 16;
			y = destination.Y - data.Origin.Y * 16;
			if (tileID == TileID.ClosedDoor) y -= 16;
			for (int j = 0; j < data.Height; j++) {
				destination.Y = y + j * 16;
				for (int i = 0; i < data.Width; i++) {
					destination.X = x + i * 16;
					spriteBatch.Draw(
						texture,
						destination,
						new(i * 18, j * 18, 16, 16),
						color
					);
				}
			}
		}
	}
	public class PlaceChest : PlaceSpecialTile {
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile && GetDescriptorType(tile) == typeof(PlaceChest)) {
					TileObjectData data = TileObjectData.GetTileData(tile);
					(int i, int j) = tile.GetTilePosition();
					TileUtils.GetMultiTileTopLeft(i, j, data, out int left, out int top);
					if (i != left + data.Origin.X || j != top + data.Origin.Y) return "Empty";
					int style = 0;
					int alternate = 0;
					TileObjectData.GetTileInfo(tile, ref style, ref alternate);
					string lootPool = "";
					if (Main.chest.GetIfInRange(Chest.FindChest(left, top)) is Chest { name: string name } && !string.IsNullOrWhiteSpace(name)) lootPool = "," + name;
					return $"{Name}({TileID.Search.GetName(tile.TileType)},{style}{lootPool})";
				}
				return null;
			});
		}
		public override void PlaceTile(int i, int j, CachedTileType type, int style, int dir, string[] parameters) {
			base.PlaceTile(i, j, type, style, dir, parameters);
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
			if (parameters.Length <= 2) return;
			if (Main.chest.GetIfInRange(Chest.FindChest(left, top)) is not Chest chest || !ModContent.TryFind(parameters[2], out LootPool pool)) return;
			using Origins.ItemDropHandler _ = new((info, item, stack, _) => {
				for (int i = 0; i < chest.item.Length; i++) {
					switch (chest.item[i]?.IsAir) {
						case false:
						continue;
						case null:
						chest.item[i] = new();
						break;
					}
					chest.item[i].SetDefaults(item);
					chest.item[i].stack = stack;
					chest.item[i].Prefix(-1);
					break;
				}
			});
			pool.Resolve(new DropAttemptInfo() {
				player = new() { luck = (float)Main.starGameMath() - 1 },
				rng = WorldGen.genRand,
				IsExpertMode = Main.expertMode,
				IsMasterMode = Main.masterMode
			});
		}
	}
	public class PlaceWall : SerializableTileDescriptor {
		readonly ConcurrentDictionary<string, TileDescriptor> descriptors = [];
		public override void Load() {
			AddSerializer(tile => tile.WallType == WallID.None ? null : $"PlaceWall({WallID.Search.GetName(tile.WallType)})");
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			return descriptors.GetOrAdd(parameters[0], parameter => {
				CachedWallType type = new(parameter);
				return new((_, _, i, j) => {
					Tile tile = Main.tile[i, j];
					tile.WallType = type;
				}, Parts: [originalText]);
			});
		}
		public class CachedWallType(string name) {
			readonly string name = name;
			ushort? id;
			public ushort Value => id ??= (ushort)WallID.Search.GetId(name);
			public static implicit operator ushort(CachedWallType type) => type.Value;
		}
	}
	public class PlaceLiquid : SerializableTileDescriptor {
		readonly TileDescriptor[] fullDescriptors = new TileDescriptor[LiquidID.Count];
		public override void Load() {
			AddSerializer(tile => tile.LiquidAmount switch {
				0 => null,
				255 => $"PlaceLiquid({LiquidID.Search.GetName(tile.LiquidType)})",
				_ => $"PlaceLiquid({LiquidID.Search.GetName(tile.LiquidType)},{tile.LiquidAmount})"
			});
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			byte liquidType = (byte)LiquidID.Search.GetId(parameters[0]);
			byte liquidAmount = 255;
			if (parameters.Length > 1) liquidAmount = byte.Parse(parameters[1]);
			return Create(liquidType, liquidAmount, originalText);
		}
		TileDescriptor Create(byte liquidType, byte liquidAmount, string originalText) {
			TileDescriptor descriptor = fullDescriptors[liquidType];
			if (liquidAmount != 255 || descriptor is null) {
				descriptor = new((_, _, i, j) => {
					Tile tile = Main.tile[i, j];
					tile.LiquidType = liquidType;
					tile.LiquidAmount = liquidAmount;
				}, Parts: [originalText]);
				if (liquidAmount == 255) fullDescriptors[liquidType] = descriptor;
			}
			return descriptor;
		}
	}
	public class SlopeTile : SerializableTileDescriptor {
		readonly TileDescriptor[] descriptors = new TileDescriptor[(int)BlockType.SlopeUpRight + 1];
		public override void Load() {
			AddSerializer(tile => {
				if (tile.HasTile && tile.BlockType != BlockType.Solid) {
					return $"SlopeTile({tile.BlockType})";
				}
				return null;
			});
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) {
			BlockType blockType = Enum.Parse<BlockType>(parameters[0]);
			return descriptors[(int)blockType] ??= new((_, _, i, j) => {
				Tile tile = Main.tile[i, j];
				tile.BlockType = blockType;
			}, Parts: [originalText]);
		}
	}
	public class Void : SerializableTileDescriptor {
		readonly TileDescriptor descriptor = new(null, null, Ignore: true);
		protected override TileDescriptor Create(string[] parameters, string originalText) => descriptor;
	}
	public class Empty : SerializableTileDescriptor {
		readonly TileDescriptor descriptor = new((_, _, i, j) => {
			if (Structure.ignoreEmpty.Contains(new(i, j))) return;
			Tile tile = Main.tile[i, j];
			tile.HasTile = false;
		}, Parts: ["Empty"]);
		public override void Load() {
			AddSerializer(tile => tile.HasTile ? null : "Empty");
		}
		protected override TileDescriptor Create(string[] parameters, string originalText) => descriptor;
	}
}
