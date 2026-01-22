using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Tools.Wiring;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Ashen {
	public class Transistor : OriginTile, IAshenTile {
		public virtual Color MapColor => FromHexRGB(0x7a391a);
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Ashen_Foliage>(), 0, 6));
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			TileID.Sets.DrawTileInSolidLayer[Type] = true;
			AddMapEntry(MapColor, CreateMapEntryName());

			MinPick = 65;
			MineResist = 2;
			HitSound = SoundID.Tink;
			DustType = Ashen_Biome.DefaultTileDust;
			RegisterItemDrop(ItemType<Transistor_Item>());
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Main.tile[i, j];
			Point pos = new(i, j);
			Point direction = GetDirection(tile);
			bool IsInvalidConnecton(int offset) {
				Point position = pos;
				position.X += direction.X * offset;
				position.Y += direction.Y * offset;
				Tile other = Framing.GetTileSafely(position);
				if (!other.HasTile) return true;
				if (other.TileType != Type) return true;
				if (other.TileFrameX != tile.TileFrameX) return true;
				if ((other.TileFrameY % (3 * 18)) != (tile.TileFrameY % (3 * 18) + 18 * offset)) return true;
				return false;
			}
			switch ((tile.TileFrameY / 18) % 3) {
				case 0: {
					if (IsInvalidConnecton(1) || IsInvalidConnecton(2)) {
						WorldGen.KillTile(pos.X, pos.Y, noItem: true);
					}
					return false;
				}
				case 1: {
					if (IsInvalidConnecton(-1) || IsInvalidConnecton(1)) {
						WorldGen.KillTile(pos.X, pos.Y, noItem: true);
					}
					return false;
				}
				case 2: {
					if (IsInvalidConnecton(-2) || IsInvalidConnecton(-1)) {
						WorldGen.KillTile(pos.X, pos.Y, noItem: true);
					}
					return false;
				}
			}
			return base.TileFrame(i, j, ref resetFrame, ref noBreak);
		}
		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			switch ((tile.TileFrameY / 18) % 3) {
				case 0: {
					if (tile.TileFrameY.TrySet((short)(tile.Get<Ashen_Wire_Data>().AnyPower ? 18 * 3 : 0))) {
						Wiring.SkipWire(i, j);
						UpdateTransistor(new(i, j));
					}
					break;
				}
				case 1: {
					tile.TileFrameY = (short)((tile.TileFrameY + 3 * 18) % (6 * 18));
					Wiring.SkipWire(i, j);
					UpdateTransistor(new Point(i, j) - GetDirection(tile));
					break;
				}
			}
		}
		public virtual void UpdateTransistor(Point pos) {
			Tile input = Main.tile[pos];
			bool inputPower = input.Get<Ashen_Wire_Data>().AnyPower;
			SetTilePowerFrame(pos.X, pos.Y, inputPower);

			Point dir = GetDirection(input);
			pos += dir;
			Tile @switch = Main.tile[pos];
			pos += dir;
			Tile output = Main.tile[pos];

			bool power = @switch.TileFrameY >= 3 * 18 && inputPower;
			Ashen_Wire_Data.SetTilePowered(pos.X, pos.Y, power);
			SetTilePowerFrame(pos.X, pos.Y, power);
		}
		static void SetTilePowerFrame(int i, int j, bool on) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY.TrySet((short)((tile.TileFrameY + 3 * 18) % (3 * 18) + on.ToInt() * 3 * 18)))
				NetSyncTile(i, j);
		}
		static void NetSyncTile(int i, int j) {
			NetMessage.SendData(MessageID.TileSquare, Main.myPlayer, -1, null, i, j, 1, 1);
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
			placement = placement.ToArray();
			if (placement.Count() != 3) throw new ArgumentException($"Invalid placement [{string.Join(", ", placement)}], must have 3 positions");
			foreach ((Point pos, _) in placement) {
				Tile tile = Framing.GetTileSafely(pos);
				if (tile.HasTile && !(Main.tileCut[tile.TileType] || TileID.Sets.BreakableWhenPlacing[tile.TileType])) {
					return false;
				}
			}
			Point? first = null;
			foreach ((Point pos, Point frame) in placement) {
				Tile tile = Framing.GetTileSafely(pos);
				if (tile.HasTile) WorldGen.KillTile(pos.X, pos.Y);
				tile.HasTile = true;
				tile.TileType = Type;
				tile.TileFrameX = (short)frame.X;
				tile.TileFrameY = (short)frame.Y;
				NetSyncTile(pos.X, pos.Y);
				first ??= pos;
			}
			UpdateTransistor(first.Value);
			return true;
		}
		public static bool TryGetPlacement(Point pos, out IEnumerable<(Point pos, Point frame)> placement) {
			Vector2 diff = Main.MouseWorld - pos.ToWorldCoordinates();
			if (diff == default || diff.HasNaNs()) diff = -Vector2.UnitY;
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
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Transistor>());
			Item.createTile = -1;
			Item.mech = true;
		}
		public override void AddRecipes() {
			CreateRecipe(2)
			.AddIngredient(ItemID.Wire, 4)
			.AddIngredient(ItemID.CopperBar)
			.AddIngredient<Silicon_Bar>()
			.AddTile(ModContent.TileType<Metal_Presser>())
			.Register();
			CreateRecipe(2)
			.AddIngredient(ItemID.Wire, 4)
			.AddIngredient(ItemID.TinBar)
			.AddIngredient<Silicon_Bar>()
			.AddTile(ModContent.TileType<Metal_Presser>())
			.Register();
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI != Main.myPlayer || !player.ItemAnimationJustStarted || TileLoader.GetTile(Item.createTile) is not Transistor transistor) return false;
			return transistor.TryPlace(Player.tileTargetX, Player.tileTargetY);
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
			Texture2D texture = TextureAssets.Tile[Item.createTile].Value;
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
