using Humanizer;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Transistor : OriginTile, IAshenTile {
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			AddMapEntry(FromHexRGB(0x7a391a));

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			switch ((tile.TileFrameY / 18) % 3) {
				case 0: {
					if (tile.TileFrameY.TrySet((short)(tile.Get<Ashen_Wire_Data>().AnyPower ? 18 * 3 : 0))) {
						UpdateTransistor(new(i, j));
					}
					break;
				}
				case 1: {
					tile.TileFrameY = (short)((tile.TileFrameY + 3 * 18) % (6 * 18));
					UpdateTransistor(new Point(i, j) - GetDirection(tile));
					break;
				}
			}
		}
		public static void UpdateTransistor(Point pos) {
			Tile input = Main.tile[pos];
			bool inputPower = input.Get<Ashen_Wire_Data>().AnyPower;
			input.TileFrameY = (short)((input.TileFrameY + 3 * 18) % (3 * 18) + inputPower.ToInt() * 3 * 18);
			Point dir = GetDirection(input);
			Tile @switch = Main.tile[pos += dir];
			Tile output = Main.tile[pos += dir];
			bool power = @switch.TileFrameY >= 3 * 18 && inputPower;
			Ashen_Wire_Data.SetTilePowered(pos.X, pos.Y, power);
			output.TileFrameY = (short)((output.TileFrameY + 3 * 18) % (3 * 18) + power.ToInt() * 3 * 18);
		}
		public static Point GetDirection(Tile tile) {
			switch (tile.TileFrameX / 18) {
				case 0:
				return new(0, 1);
				case 1:
				return new(0, -1);
				case 2:
				return new(-1, 0);
				case 3:
				return new(1, 0);

				default:
				throw new ArgumentException($"Invalid frame {tile.TileFrameX}");
			}
		}
		public bool TryPlace(int i, int j) {
			return TryGetPlacement(new(i, j), out IEnumerable<(Point pos, Point frame)> placement) && TryPlace(placement);
		}
		public bool TryPlace(Point position, Point direction) => TryPlace(GetPlacement(position, direction));
		public bool TryPlace(IEnumerable<(Point pos, Point frame)> placement) {
			foreach ((Point pos, _) in placement) {
				Tile tile = Framing.GetTileSafely(pos);
				if (tile.HasTile && !(Main.tileCut[tile.TileType] || TileID.Sets.BreakableWhenPlacing[tile.TileType])) {
					return false;
				}
			}
			foreach ((Point pos, Point frame) in placement) {
				Tile tile = Framing.GetTileSafely(pos);
				if (tile.HasTile) WorldGen.KillTile(pos.X, pos.Y);
				tile.HasTile = true;
				tile.TileType = Type;
				tile.TileFrameX = (short)frame.X;
				tile.TileFrameY = (short)frame.Y;
				NetMessage.SendTileSquare(-1, pos.X, pos.Y, 1);
			}
			return true;
		}
		public static bool TryGetPlacement(Point pos, out IEnumerable<(Point pos, Point frame)> placement) {
			Vector2 diff = Main.MouseWorld - pos.ToWorldCoordinates();
			if (diff == default || diff.HasNaNs()) {
				placement = default;
				return false;
			}
			Point direction;
			if (Math.Abs(diff.X) >= Math.Abs(diff.Y)) {
				direction = new(Math.Sign(diff.X), 0);
			} else {
				direction = new(0, Math.Sign(diff.Y));
			}
			placement = GetPlacement(pos, direction);
			return true;
		}
		public static IEnumerable<(Point pos, Point frame)> GetPlacement(Point pos, Point direction) {
			if (Math.Abs(direction.X) + Math.Abs(direction.Y) != 1) throw new ArgumentException($"Invalid direction {direction}, direction must have a magnitude of 1", nameof(direction));
			Point frame = new();
			switch (direction) {
				case (0, 1):
				frame.X = 0 * 18;
				break;
				case (0, -1):
				frame.X = 1 * 18;
				break;
				case (-1, 0):
				frame.X = 2 * 18;
				break;
				case (1, 0):
				frame.X = 3 * 18;
				break;

				default:
				throw new ArgumentException($"Invalid direction {direction}, direction must have a magnitude of 1", nameof(direction));
			}
			for (int i = 0; i < 3; i++) {
				yield return (pos, frame);
				pos += direction;
				frame.Y += 18;
			}
		}
	}
	public class Transistor_Item : ModItem, ISpecialTilePreviewItem {
		public override string Texture => typeof(Transistor).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Transistor>());
			Item.createTile = -1;
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI != Main.myPlayer || !player.ItemAnimationJustStarted) return false;
			return GetInstance<Transistor>().TryPlace(Player.tileTargetX, Player.tileTargetY);
		}
		public void DrawPreview() {
			if (!Transistor.TryGetPlacement(new(Player.tileTargetX, Player.tileTargetY), out IEnumerable<(Point pos, Point frame)> placement)) return;
			Color color = Color.White;
			foreach ((Point pos, _) in placement) {
				Tile tile = Framing.GetTileSafely(pos);
				if (tile.HasTile && !(Main.tileCut[tile.TileType] || TileID.Sets.BreakableWhenPlacing[tile.TileType])) {
					color = Color.Red * 0.7f;
					break;
				}
			}
			color *= 0.5f;
			Texture2D texture = TextureAssets.Tile[TileType<Transistor>()].Value;
			foreach ((Point pos, Point frame) in placement) {
				Main.spriteBatch.Draw(
					texture,
					pos.ToWorldCoordinates(0, 0) - Main.screenPosition,
					new Rectangle(frame.X, frame.Y, 16, 16),
					color
				);
			}
		}
	}
}
