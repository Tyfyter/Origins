using Humanizer;
using MagicStorage;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Other.Testing;
using Origins.Tiles.Brine;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Shelf_Coral : ModTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override void SetStaticDefaults() {
			if (Main.netMode != NetmodeID.Server) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			AddMapEntry(new Color(40, 165, 240));

			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateHeights = [28];

			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.StyleMultiplier = 3;
			TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<Shelf_Coral_TE>().Generic_HookPostPlaceMyPlayer;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(1, 0);
			TileObjectData.newAlternate.AnchorWall = true;
			TileObjectData.addAlternate(1);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(2, 0);
			TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0);
			TileObjectData.addAlternate(2);

			TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, 1, 0);
			TileObjectData.addTile(Type);
			//soundType = SoundID.Grass;

			OriginsSets.Tiles.MultitileCollisionOffset[Type] = OffsetCollision;
		}
		static void OffsetCollision(Tile tile, ref float y, ref int height) {
			int frameY = tile.TileFrameY / 34;
			switch (frameY) {
				case 5:
				int frameX = tile.TileFrameX / 18;
				int altX = frameX / 3;
				frameX %= 3;
				switch ((altX, frameX)) {
					case (0, 2):
					case (1, 0) or (1, 2):
					case (2, 0):
					height = -1600;
					break;
				}
				break;

				default:
				if (frameY > 5) {
					height = -1600;
				}
				break;
			}
		}
		public override bool CanDrop(int i, int j) => true;

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// When the tile is removed, we need to remove the Tile Entity as well.
			ModContent.GetInstance<Shelf_Coral_TE>().Kill(i, j);
		}
		public override void FloorVisuals(Player player) {
			foreach ((int i, int j) in Collision.GetTilesIn(player.BottomLeft, player.BottomRight + Vector2.UnitY)) {
				if (TileEntity.TryGet(i, j, out Shelf_Coral_TE tileEntity)) tileEntity.isStoodOn = true;
			}
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowTexture = this.GetGlowTexture(drawData.tileCache.TileColor);
			drawData.glowSourceRect = new(drawData.tileFrameX, drawData.tileFrameY, 16, 28);
			drawData.glowColor = GlowColor;
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Shelf_Coral_TE : ModTileEntity {
		private int timer;
		public State CurrentState { get; private set; }
		public bool isStoodOn = false;
		public override void NetSend(BinaryWriter writer) {
			writer.Write((int)CurrentState);
			writer.Write((int)timer);
		}
		public override void NetReceive(BinaryReader reader) {
			CurrentState = (State)reader.ReadInt32();
			timer = reader.ReadInt32();
		}
		public override void Update() {
			State oldState = CurrentState;
			int frameNum = 0;
			switch (CurrentState) {
				case State.Out:
				const int time = 15;
				const int ticks_per_frame = 3;
				if (isStoodOn) {
					if (timer == 0) ;// sound here
					if (++timer >= time) CurrentState = State.GoingIn;
				} else if (timer > 0 && ++timer >= time) {
					timer = 0;
				}
				frameNum = Math.Min(timer / ticks_per_frame, 4);
				break;
				case State.GoingIn:
				if (timer.Warmup(4 * 5)) CurrentState = State.In;
				frameNum = Math.Max(4, 2 + timer / 4);
				break;
				case State.In:
				if (timer.Warmup(60 * 5)) CurrentState = State.ComingOut;
				frameNum = 7;
				break;
				case State.ComingOut:
				if (timer.Warmup(6 * 3)) CurrentState = State.Out;
				else frameNum = 7 - timer / 6;
				break;
			}
			isStoodOn = false;
			frameNum = Math.Min(frameNum, 6);
			frameNum *= 34;
			for (int i = 0; i < 3; i++) {
				Framing.GetTileSafely(Position.X + i, Position.Y).TileFrameY = (short)frameNum;
			}
			if (CurrentState != oldState) {
				timer = 0;
				if (Main.netMode == NetmodeID.Server) {
					// The TileEntitySharing message will trigger NetSend, manually syncing the changed data.
					NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
				}
			}
		}
		public override bool IsTileValidForEntity(int x, int y) {
			Tile tile = Main.tile[x, y];
			return tile.HasTile && tile.TileType == ModContent.TileType<Shelf_Coral>();
		}
		public enum State {
			Out,
			GoingIn,
			In,
			ComingOut
		}
	}
	public class Shelf_Coral_Item : TestingItem {
		public override string Texture => typeof(Shelf_Coral).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Shelf_Coral>());
		}
	}
}
