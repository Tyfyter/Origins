using Microsoft.Xna.Framework.Graphics;
using Origins.Core.Structures;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Tiles.Dev; 
public class Room_Socket_Marker : ModTile {
	public static bool IsSocketMarker(Tile tile) => tile.HasTile && TileLoader.GetTile(tile.TileType) is Room_Socket_Marker;
	public static bool HasSocket(int i, int j, out Direction direction) {
		if (Room_Socket_TE.TryGet(new(i, j), out direction)) return true;
		Tile tile = Main.tile[i, j];
		if (IsSocketMarker(tile)) {
			direction = (tile.TileFrameX / 18) switch {
				1 => Direction.Right,
				2 => Direction.Left,
				3 => Direction.Down,
				4 => Direction.Up,
				_ => 0
			};
			return true;
		}
		return false;
	}
	public sealed override void SetStaticDefaults() {
		Main.tileFrameImportant[Type] = true;
		TileID.Sets.CanBeSloped[Type] = true;
	}
	public sealed override bool Slope(int i, int j) {
		Tile tile = Main.tile[i, j];
		tile.TileFrameX += 18;
		tile.TileFrameX %= 5 * 18;
		return false;
	}
	public sealed override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
	class Room_Socket_Marker_Item : ModItem, IExpectToBeUnobtainable {
#if !DEBUG
		public override bool IsLoadingEnabled(Mod mod) => !IsDebug || DebugConfig.Instance.ForceEnableDebugItems;
#endif
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
			Item.ResearchUnlockCount = 0;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Room_Socket_Marker>());
			Item.useAnimation = Item.useTime;
			Item.width = 14;
			Item.height = 28;
			Item.consumable = false;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.createTile = -1;
			} else {
				Item.createTile = ModContent.TileType<Room_Socket_Marker>();
			}
			return base.CanUseItem(player);
		}
		public override void HoldStyle(Player player, Rectangle heldItemFrame) {
			Item.createTile = ModContent.TileType<Room_Socket_Marker>();
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2 || Main.tile[Player.tileTargetX, Player.tileTargetY].HasTile) {
				if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileIsType<Room_Socket_Marker>()) return false;
				Room_Socket_TE.Cycle(new(Player.tileTargetX, Player.tileTargetY));
				return true;
			}
			return null;
		}
	}
	class Room_Socket_TE : TESystem<Room_Socket_TE.Data> {
		AutoLoadingTexture tex = "Origins/Tiles/Dev/Room_Socket_Overlay_Marker";
#if DEBUG
		protected override bool IsValidTile(Tile tile) => true;
#else
		protected override bool IsValidTile(Tile tile) => false;
#endif
		public static void Cycle(Point16 position) {
			Room_Socket_TE instance = ModContent.GetInstance<Room_Socket_TE>();
			Dictionary<Point16, Data> tileEntities = instance.tileEntities;
			if (!tileEntities.TryGetValue(position, out Data data)) {
				instance.AddTileEntity(position, new());
				return;
			}
			switch (data.Value) {
				case 0:
				case Direction.Right:
				case Direction.Left:
				case Direction.Up:
				data.Value++;
				tileEntities[position] = data;
				break;
				
				case Direction.Down:
				tileEntities.Remove(position);
				break;
			}
		}
		public static bool TryGet(Point16 position, out Direction direction) {
			if (ModContent.GetInstance<Room_Socket_TE>().tileEntities.TryGetValue(position, out Data data)) {
				direction = data.Value;
				return true;
			}
			direction = default;
			return false;
		}
		public override void PostDrawTiles() {
			Rectangle rect = new(-16, -16, Main.screenWidth + 16, Main.screenHeight + 16);
			Main.spriteBatch.Begin();
			foreach ((Point16 pos, Data data) in tileEntities) {
				Vector2 position = pos.ToWorldCoordinates(0, 0) - Main.screenPosition;
				if (rect.Contains(position)) Main.spriteBatch.Draw(tex, position, new Rectangle(18 * (int)data.Value, 0, 16, 16), Color.White);
			}
			Main.spriteBatch.End();
		}
		public struct Data() : ITileEntityData {
			public Direction Value {
				get;
				set => IsDirty |= field.TrySet(value);
			}
			public bool IsDirty { get; set; } = true;
			readonly void ITileEntityData.NetSend(BinaryWriter writer) => writer.Write((byte)Value);
			static Data ITileEntityData.NetReceive(BinaryReader reader, Data data) => new() { Value = (Direction)reader.ReadByte() };
			readonly void ITileEntityData.SaveTE(TagCompound tag) => tag[nameof(Value)] = (byte)Value;
			static Data ITileEntityData.LoadTE(TagCompound tag) => new() { Value = (Direction)tag.SafeGet<byte>(nameof(Value)) };
			readonly void ITileEntityData.Update(Point16 position) { }
		}
	}
}
