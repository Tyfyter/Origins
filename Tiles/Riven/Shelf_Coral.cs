using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Other.Testing;
using Origins.World;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Shelf_Coral : ModTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
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
					return;
				}
				break;

				default:
				if (frameY > 5) return;
				break;
			}
			color.DoFancyGlow(new Vector3(0.394f, 0.879f, 0.912f) * GlowValue, tile.TileColor);
		}
		public override void Load() {
			this.SetupGlowKeys();
			On_Projectile.AI_007_GrapplingHooks_CanTileBeLatchedOnTo += On_Projectile_AI_007_GrapplingHooks_CanTileBeLatchedOnTo;
		}

		bool On_Projectile_AI_007_GrapplingHooks_CanTileBeLatchedOnTo(On_Projectile.orig_AI_007_GrapplingHooks_CanTileBeLatchedOnTo orig, Projectile self, int x, int y) {
			if (orig(self, x, y)) {
				Tile tile = Main.tile[x, y];
				if (tile.TileType != Type) return true;
				return tile.TileFrameY / 34 <= 5;
			}
			return false;
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
			TileID.Sets.FramesOnKillWall[Type] = true;
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

			DustType = Riven_Hive.DefaultTileDust;

			Origins.PotType.Add(Type, ((ushort)ModContent.TileType<Riven_Pot>(), 0, 0));
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
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Tile tile = Framing.GetTileSafely(i, j);
			int frameX = tile.TileFrameX / 18;
			int altX = frameX / 3;
			if (altX != (frameX % 3)) return true;
			static bool IsValidAnchor(int i, int j, bool right) {
				Tile tile = Framing.GetTileSafely(i, j);
				if (!tile.HasFullSolidTile()) return false;
				if (tile.BlockType == BlockType.Solid) return true;
				return right ? tile.LeftSlope : tile.RightSlope;
			}
			bool shouldBreak = false;
			switch (altX) {
				case 0:
				shouldBreak = !IsValidAnchor(i - 1, j, true);
				break;
				case 2:
				shouldBreak = !IsValidAnchor(i + 1, j, false);
				break;
			}
			if (shouldBreak) {
				tile.HasTile = false;
				WorldGen.SquareTileFrame(i, j);
				return false;
			}
			return base.TileFrame(i, j, ref resetFrame, ref noBreak);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// When the tile is removed, we need to remove the Tile Entity as well.
			ModContent.GetInstance<Shelf_Coral_TE>().Kill(i, j);
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowTexture = this.GetGlowTexture(drawData.tileCache.TileColor);
			drawData.glowSourceRect = new(drawData.tileFrameX, drawData.tileFrameY, 16, 28);
			drawData.glowColor = GlowColor;
		}
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		static Rectangle playerHitbox = new(0, 0, 20, 40);
		static Rectangle fallPastHitbox = new(0, 0, 20, 40 * 2);
		static Rectangle fallThroughHitbox = new(0, 0, 20, 40 * 2);
		public delegate bool GenerationCheck(int i, int j, bool checkCramped);
		public (string name, TilePatternMatcher pattern, GenerationCheck extraChecks, double weight)[] patterns = [..SetupPatterns()];
		static IEnumerable<(string name, TilePatternMatcher pattern, GenerationCheck extraChecks, double weight)> SetupPatterns() {
			Dictionary<char, Predicate<Tile>> coralPredicates = new() {
				['X'] = OriginExtensions.HasFullSolidTile,
				['_'] = tile => !tile.HasTile,
				['L'] = tile => OriginExtensions.HasFullSolidTile(tile) && (tile.BlockType == BlockType.Solid || tile.RightSlope),
				['R'] = tile => OriginExtensions.HasFullSolidTile(tile) && (tile.BlockType == BlockType.Solid || tile.LeftSlope),
				['|'] = tile => !tile.TileIsType(ModContent.TileType<Shelf_Coral>()) && (tile.LiquidAmount < 64 || tile.LiquidType != LiquidID.Water),
				['A'] = tile => tile.LiquidAmount >= 128 && tile.LiquidType == LiquidID.Water,
			};
			yield return ("Centered", new(
				"""
				 ||| 
				 ||| 
				 ||| 
				X_O_X
				 ___ 
				""",
				coralPredicates
			), (i, j, checkCramped) => {
				playerHitbox.Location = new(i * 16 + 8 - 10, j * 16 - 40);
				return !playerHitbox.OverlapsAnyTiles();
			}, 1);

			yield return ("Left (Soup)", new(
				"""
				 |||
				 |||
				 |||
				RO__
				R___
				 AAA
				""",
				coralPredicates
			), null, 1);
			yield return ("Right (Soup)", new(
				"""
				||| 
				||| 
				||| 
				__OL
				___L
				AAA 
				""",
				coralPredicates
			), null, 1);

			yield return ("Left", new(
				"""
				 |||
				 |||
				 |||
				RO__
				R___
				""",
				coralPredicates
			), (i, j, checkCramped) => {
				playerHitbox.Location = new((i + 1) * 16 + 8 - 10, j * 16 - 40);
				if (playerHitbox.OverlapsAnyTiles()) return false;
				if (!checkCramped) return true;
				fallPastHitbox.Location = new((i + 3) * 16, j * 16 + 8 - 40);
				fallThroughHitbox.Location = new((i + 1) * 16, j * 16 + 8 - 40);
				return fallPastHitbox.OverlapsAnyTiles() && !fallThroughHitbox.OverlapsAnyTiles();
			}, 1);
			yield return ("Right", new(
				"""
				||| 
				||| 
				||| 
				__OL
				___L
				""",
				coralPredicates
			), (i, j, checkCramped) => {
				playerHitbox.Location = new((i - 1) * 16 + 8 - 10, j * 16 - 40);
				if (playerHitbox.OverlapsAnyTiles()) return false;
				if (!checkCramped) return true;
				fallPastHitbox.Location = new((i - 3) * 16 - 16, j * 16 + 8 - 40);
				fallThroughHitbox.Location = new((i - 2) * 16, j * 16 + 8 - 40);
				return fallPastHitbox.OverlapsAnyTiles() && !fallThroughHitbox.OverlapsAnyTiles();
			}, 1);

			yield return ("Open", new(
				"""
				 ||| 
				 ||| 
				 ||| 
				__O__
				 ___ 
				""",
				coralPredicates
			), (i, j, checkCramped) => {
				playerHitbox.Location = new(i * 16 + 8 - 10, j * 16 - 40);
				return !playerHitbox.OverlapsAnyTiles();
			}, 0.03);
		}
		public bool CanGenerate(int i, int j, out double weight, bool checkCramped = true) {
			playerHitbox.Location = new(i * 16 - 10, j * 16 - 40);
			weight = 0;
			if (playerHitbox.OverlapsAnyTiles()) return false;
			Point coralPos = new(i, j);
			for (int k = 0; k < patterns.Length; k++) {
				if (patterns[k].pattern.Matches(coralPos) && (patterns[k].extraChecks?.Invoke(i, j, checkCramped) ?? true)) weight += patterns[k].weight;
			}
			return weight > 0;
		}
	}
	public class Shelf_Coral_TE : ModTileEntity {
		private int timer;
		State _currentState;
		public State CurrentState {
			get => _currentState;
			set {
				if (NetmodeActive.MultiplayerClient) return;
				if (_currentState.TrySet(value)) {
					timer = 0;
					if (Main.netMode == NetmodeID.Server) {
						// The TileEntitySharing message will trigger NetSend, manually syncing the changed data.
						NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
					}
				}
			}
		}
		public bool isStoodOn = false;
		public override void NetSend(BinaryWriter writer) {
			writer.Write((int)CurrentState);
			writer.Write((int)timer);
			writer.Write(isStoodOn);
		}
		public override void NetReceive(BinaryReader reader) {
			_currentState = (State)reader.ReadInt32();
			timer = reader.ReadInt32();
			isStoodOn = reader.ReadBoolean();
		}
		bool wasLocalPlayerStanding = false;
		void ProcessStanding() {
			isStoodOn = false;
			Rectangle coralTop = new(Position.X * 16 - 1, Position.Y * 16, 3 * 16 + 2, 1);
			bool shouldForceSync = NetmodeActive.MultiplayerClient && (wasLocalPlayerStanding != (Main.LocalPlayer.velocity.Y == 0));
			wasLocalPlayerStanding = Main.LocalPlayer.velocity.Y == 0;
			foreach (Player player in Main.ActivePlayers) {
				if (player.velocity.Y != 0) continue;
				Vector2 bottomLeft = player.BottomLeft;
				float distToCoral = float.Ceiling(bottomLeft.Y / 16) * 16 - bottomLeft.Y;
				if (distToCoral > 4) continue;
				Vector2 offset = new(0, distToCoral);
				Rectangle playerBottom = OriginExtensions.BoxOf(bottomLeft + offset, player.BottomRight + offset + Vector2.UnitY);
				isStoodOn = playerBottom.Intersects(coralTop);
				if (isStoodOn) {
					if (player.whoAmI == Main.myPlayer && shouldForceSync) {
						NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
					}
					return;
				}
			}
		}
		public override void Update() {
			State oldState = CurrentState;
			int frameNum = 0;
			bool wasStoodOn = isStoodOn;
			ProcessStanding();
			if (isStoodOn != wasStoodOn && !NetmodeActive.MultiplayerClient) {
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
			}
			switch (CurrentState) {
				case State.Out:
				const int time = 15;
				const int ticks_per_frame = 3;
				if (isStoodOn) {
					if (timer == 0) {
						SoundEngine.PlaySound(SoundID.NPCDeath3.WithPitch(0.5f).WithVolume(0.4f), Position.ToWorldCoordinates());
					}
					if (++timer >= time) {
						bool anythingBlocking = false;
						for (int i = 0; i < 3 && !anythingBlocking; i++) {
							Point pos = new(Position.X + i, Position.Y - 1);
							Tile tile = Framing.GetTileSafely(pos);
							anythingBlocking = tile.HasTile && TileID.Sets.PreventsTileRemovalIfOnTopOfIt[tile.TileType];
						}
						if (!anythingBlocking) CurrentState = State.GoingIn;
					}
				} else if (timer > 0 && ++timer >= time) {
					timer = 0;
				}
				frameNum = Math.Min(timer / ticks_per_frame, 4);
				break;
				case State.GoingIn:
				if (timer.Warmup(4 * 5)) {
					SoundEngine.PlaySound(SoundID.Item131.WithVolume(0.4f), Position.ToWorldCoordinates());
					CurrentState = State.In;
					for (int i = 0; i < 3; i++) {
						Point pos = new(Position.X + i, Position.Y - 1);
						Tile tile = Framing.GetTileSafely(pos);
						if (TileID.Sets.ReplaceTileBreakUp[tile.TileType]) {
							WorldGen.KillTile(pos.X, pos.Y);
							continue;
						}
						TileObjectData data = TileObjectData.GetTileData(tile);
						if (data is null) continue;
						if (!data.AnchorBottom.type.HasFlag(AnchorType.SolidTile) && !data.AnchorBottom.type.HasFlag(AnchorType.SolidWithTop)) continue;
						int startX = TileObjectData.TopLeft(pos.X, pos.Y).X;
						for (int j = data.AnchorBottom.checkStart; j < data.AnchorBottom.tileCount; j++) {
							if (startX + j == pos.X) {
								WorldGen.KillTile(pos.X, pos.Y);
								break;
							}
						}
					}
					frameNum = 7;
				} else frameNum = Math.Max(4, 2 + timer / 4);
				break;
				case State.In:
				if (timer.Warmup(60 * 5)) CurrentState = State.ComingOut;
				frameNum = 7;
				break;
				case State.ComingOut:
				if (timer.Warmup(6 * 3)) {
					CurrentState = State.Out;
					SoundEngine.PlaySound(SoundID.Item97.WithPitch(1f).WithVolume(0.4f), Position.ToWorldCoordinates());
				} else frameNum = 7 - timer / 6;
				break;
			}
			frameNum = Math.Min(frameNum, 6);
			frameNum *= 34;
			for (int i = 0; i < 3; i++) {
				Framing.GetTileSafely(Position.X + i, Position.Y).TileFrameY = (short)frameNum;
			}
		}
		public override bool IsTileValidForEntity(int x, int y) {
			if (x < 0 || y < 0) return false;
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
