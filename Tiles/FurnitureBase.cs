using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles {
	public abstract class FurnitureBase : ModTile {
		public abstract int BaseTileID { get; }
		public abstract Color MapColor { get; }
		public virtual bool LavaDeath => true;
		public TileItem Item { get; protected set; }
		protected AutoLoadingAsset<Texture2D> glowTexture;
		public virtual Color GlowmaskColor => Color.White;
		protected int width, height;
		public sealed override void Load() {
			Mod.AddContent(Item = new TileItem(this));
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = LavaDeath;
			Main.tileContainer[Type] = Main.tileContainer[BaseTileID];

			Main.tileTable[Type] = Main.tileTable[BaseTileID];
			Main.tileSolidTop[Type] = Main.tileSolidTop[BaseTileID];
			Main.tileNoAttach[Type] = Main.tileNoAttach[BaseTileID];
			TileID.Sets.HasOutlines[Type] = TileID.Sets.HasOutlines[BaseTileID];
			TileID.Sets.DisableSmartCursor[Type] = TileID.Sets.DisableSmartCursor[BaseTileID];
			TileID.Sets.IgnoredByNpcStepUp[Type] = TileID.Sets.IgnoredByNpcStepUp[BaseTileID];

			TileID.Sets.DisableSmartCursor[Type] = TileID.Sets.DisableSmartCursor[BaseTileID];
			TileID.Sets.BasicDresser[Type] = TileID.Sets.BasicDresser[BaseTileID];
			TileID.Sets.AvoidedByNPCs[Type] = TileID.Sets.AvoidedByNPCs[BaseTileID];
			TileID.Sets.InteractibleByNPCs[Type] = TileID.Sets.InteractibleByNPCs[BaseTileID];

			TileID.Sets.CanBeSleptIn[Type] = TileID.Sets.CanBeSleptIn[BaseTileID];
			TileID.Sets.CanBeSatOnForPlayers[Type] = TileID.Sets.CanBeSatOnForPlayers[BaseTileID];
			TileID.Sets.CanBeSatOnForNPCs[Type] = TileID.Sets.CanBeSatOnForNPCs[BaseTileID];
			TileID.Sets.IsValidSpawnPoint[Type] = TileID.Sets.IsValidSpawnPoint[BaseTileID];

			TileID.Sets.CountsAsWaterSource[Type] = TileID.Sets.CountsAsWaterSource[BaseTileID];

			TileID.Sets.Platforms[Type] = TileID.Sets.Platforms[BaseTileID];

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(BaseTileID, 0));
			TileObjectData.newTile.LavaDeath = LavaDeath;
			if (TileObjectData.newTile.Direction != TileObjectDirection.None) {
				TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
				TileObjectData.newAlternate.Direction = TileObjectData.newTile.Direction ^ (TileObjectDirection.PlaceLeft | TileObjectDirection.PlaceRight);
				TileObjectData.addAlternate(1);
			}
			ModifyTileData();
			width = TileObjectData.newTile.Width;
			height = TileObjectData.newTile.Height;
			TileObjectData.addTile(Type);

			if (!Main.dedServ) {
				LocalizedText text = Lang._mapLegendCache.FromType(BaseTileID);
				if (string.IsNullOrEmpty(text.Value)) AddMapEntry(MapColor);
				else AddMapEntry(MapColor, text, MapName);
			}
			AdjTiles = [
				BaseTileID
			];
			if (TileID.Sets.RoomNeeds.CountsAsTable.Contains(BaseTileID)) AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			if (TileID.Sets.RoomNeeds.CountsAsChair.Contains(BaseTileID)) AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
			glowTexture = Texture + "_Glow";
		}
		public virtual void ModifyTileData() { }
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!glowTexture.Exists) return;
			OriginExtensions.DrawTileGlow(glowTexture, GlowmaskColor, i, j, spriteBatch);
		}
		public virtual string MapName(string name, int i, int j) => name;
	}
	public abstract class ChairBase : FurnitureBase {
		public override int BaseTileID => TileID.Chairs;
		private bool IsABench => BaseTileID == TileID.Benches;
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance); // Avoid being able to trigger it from long range
		}
		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;

			if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance)) { // Avoid being able to trigger it from long range
				player.GamepadEnableGrappleCooldown();
				player.sitting.SitDown(player, i, j);
			}

			return true;
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;

			if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance)) { // Match condition in RightClick. Interaction should only show if clicking it does something
				return;
			}

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = Item.Type;

			if (Main.tile[i, j].TileFrameX / 18 < 1) {
				player.cursorItemIconReversed = true;
			}
		}
		public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info) {
			// It is very important to know that this is called on both players and NPCs, so do not use Main.LocalPlayer for example, use info.restingEntity
			Tile tile = Framing.GetTileSafely(i, j);

			//info.directionOffset = info.restingEntity is Player ? 6 : 2; // Default to 6 for players, 2 for NPCs
			//info.visualOffset = Vector2.Zero; // Defaults to (0,0)
			if (IsABench) {
				info.TargetDirection = info.RestingEntity.direction;
			} else {
				info.TargetDirection = -1;

				if (tile.TileFrameX != 0) {
					info.TargetDirection = 1; // Facing right if sat down on the right alternate (added through addAlternate in SetStaticDefaults earlier)
				}
			}

			// The anchor represents the bottom-most tile of the chair. This is used to align the entity hitbox
			// Since i and j may be from any coordinate of the chair, we need to adjust the anchor based on that
			info.AnchorTilePosition.X = i; // Our chair is only 1 wide, so nothing special required
			info.AnchorTilePosition.Y = j;

			if (tile.TileFrameY % 40 == 0) {
				info.AnchorTilePosition.Y++; // Here, since our chair is only 2 tiles high, we can just check if the tile is the top-most one, then move it 1 down
			}

			// Finally, since this is a toilet, it should generate Poo while any tier of Well Fed is active
			info.ExtraInfo.IsAToilet = BaseTileID == TileID.Toilets;
		}
		public static void ToiletHitWire(int i, int j) {
			// Spawn the toilet effect here when triggered by a signal
			Tile tile = Main.tile[i, j];

			int spawnX = i;
			int spawnY = j - (tile.TileFrameY % 40) / 18;

			Wiring.SkipWire(spawnX, spawnY);
			Wiring.SkipWire(spawnX, spawnY + 1);

			if (Wiring.CheckMech(spawnX, spawnY, 60)) {
				Projectile.NewProjectile(Wiring.GetProjectileSource(spawnX, spawnY), spawnX * 16 + 8, spawnY * 16 + 12, 0f, 0f, ProjectileID.ToiletEffect, 0, 0f, Main.myPlayer);
			}
		}
	}
	public abstract class ClockBase : ModTile {
		public abstract Color MapColor { get; }
		public virtual bool LavaDeath => true;
		protected internal TileItem item;
		public sealed override void Load() {
			Mod.AddContent(item = new TileItem(this));
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = LavaDeath;
			TileID.Sets.Clock[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			AdjTiles = [TileID.GrandfatherClocks];

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
			TileObjectData.newTile.Origin = new(0, 4);
			TileObjectData.addTile(Type);

			// Etc
			AddMapEntry(MapColor, Language.GetText("ItemName.GrandfatherClock"));
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}
		public override bool RightClick(int x, int y) {
			string text = "AM";
			// Get current weird time
			double time = Main.time;
			if (!Main.dayTime) {
				// if it's night add this number
				time += 54000.0;
			}

			// Divide by seconds in a day * 24
			time = (time / 86400.0) * 24.0;
			// Dunno why we're taking 19.5. Something about hour formatting
			time = time - 7.5 - 12.0;
			// Format in readable time
			if (time < 0.0) {
				time += 24.0;
			}

			if (time >= 12.0) {
				text = "PM";
			}

			int intTime = (int)time;
			// Get the decimal points of time.
			double deltaTime = time - intTime;
			// multiply them by 60. Minutes, probably
			deltaTime = (int)(deltaTime * 60.0);
			// This could easily be replaced by deltaTime.ToString()
			string text2 = string.Concat(deltaTime);
			if (deltaTime < 10.0) {
				// if deltaTime is eg "1" (which would cause time to display as HH:M instead of HH:MM)
				text2 = "0" + text2;
			}
			if (OriginClientConfig.Instance.TwentyFourHourTime) {
				text = "";
			} else {
				if (intTime > 12) {
					// This is for AM/PM time rather than 24hour time
					intTime -= 12;
				}

				if (intTime == 0) {
					// 0AM = 12AM
					intTime = 12;
				}
			}

			// Whack it all together to get a HH:MM format
			Main.NewText($"Time: {intTime}:{text2} {text}", 255, 240, 20);
			return true;
		}
	}
	public abstract class DresserBase : FurnitureBase {
		public override int BaseTileID => TileID.Dressers;
		public override void ModifyTileData() {
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = [
				TileID.MagicalIceBlock,
				TileID.Boulder,
				TileID.BouncyBoulder,
				TileID.LifeCrystalBoulder,
				TileID.RollingCactus
			];
			_ = DefaultContainerName(0, 0);
		}

		public override string MapName(string name, int i, int j) {
			int left = i;
			int top = j;
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX % 36 != 0) {
				left--;
			}
			if (tile.TileFrameY != 0) {
				top--;
			}
			int chest = Chest.FindChest(left, top);
			if (chest < 0) {
				return Language.GetTextValue("LegacyDresserType.0");
			} else if (Main.chest[chest].name == "") {
				return name;
			} else {
				return name + ": " + Main.chest[chest].name;
			}
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) {
			width = 3;
			height = 1;
			extraY = 0;
		}
		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			int left = Main.tile[i, j].TileFrameX / 18;
			left %= 3;
			left = i - left;
			int top = j - Main.tile[i, j].TileFrameY / 18;
			if (Main.tile[i, j].TileFrameY == 0) {
				Main.CancelClothesWindow(true);
				Main.mouseRightRelease = false;
				player.CloseSign();
				player.SetTalkNPC(-1);
				Main.npcChatCornerItem = 0;
				Main.npcChatText = "";
				if (Main.editChest) {
					SoundEngine.PlaySound(SoundID.MenuTick);
					Main.editChest = false;
					Main.npcChatText = string.Empty;
				}
				if (player.editedChestName) {
					NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
					player.editedChestName = false;
				}
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					if (left == player.chestX && top == player.chestY && player.chest != -1) {
						player.chest = -1;
						Recipe.FindRecipes();
						SoundEngine.PlaySound(SoundID.MenuClose);
					} else {
						NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
						Main.stackSplit = 600;
					}
				} else {
					player.piggyBankProjTracker.Clear();
					player.voidLensChest.Clear();
					int chestIndex = Chest.FindChest(left, top);
					if (chestIndex != -1) {
						Main.stackSplit = 600;
						if (chestIndex == player.chest) {
							player.chest = -1;
							Recipe.FindRecipes();
							SoundEngine.PlaySound(SoundID.MenuClose);
						} else if (chestIndex != player.chest && player.chest == -1) {
							player.OpenChest(left, top, chestIndex);
							SoundEngine.PlaySound(SoundID.MenuOpen);
						} else {
							player.OpenChest(left, top, chestIndex);
							SoundEngine.PlaySound(SoundID.MenuTick);
						}
						Recipe.FindRecipes();
					}
				}
			} else {
				Main.playerInventory = false;
				player.chest = -1;
				Recipe.FindRecipes();
				player.SetTalkNPC(-1);
				Main.npcChatCornerItem = 0;
				Main.npcChatText = "";
				Main.interactedDresserTopLeftX = left;
				Main.interactedDresserTopLeftY = top;
				Main.OpenClothesWindow();
			}
			return true;
		}
		public void MouseOverNearAndFarSharedLogic(Player player, int i, int j) {
			Tile tile = Main.tile[i, j];
			int left = i;
			int top = j;
			left -= tile.TileFrameX % 54 / 18;
			if (tile.TileFrameY % 36 != 0) {
				top--;
			}
			int chestIndex = Chest.FindChest(left, top);
			player.cursorItemIconID = -1;
			if (chestIndex < 0) {
				player.cursorItemIconText = Language.GetTextValue("LegacyDresserType.0");
			} else {
				string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY); // This gets the ContainerName text for the currently selected language

				if (Main.chest[chestIndex].name != "") {
					player.cursorItemIconText = Main.chest[chestIndex].name;
				} else {
					player.cursorItemIconText = defaultName;
				}
				if (player.cursorItemIconText == defaultName) {
					player.cursorItemIconID = Item.Type;
					player.cursorItemIconText = "";
				}
			}
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
		}
		public override void MouseOverFar(int i, int j) {
			Player player = Main.LocalPlayer;
			MouseOverNearAndFarSharedLogic(player, i, j);
			if (player.cursorItemIconText == "") {
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = 0;
			}
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			MouseOverNearAndFarSharedLogic(player, i, j);
			if (Main.tile[i, j].TileFrameY > 0) {
				player.cursorItemIconID = ItemID.FamiliarShirt;
				player.cursorItemIconText = "";
			}
		}
	}
	public abstract class BedBase : FurnitureBase {
		public override int BaseTileID => TileID.Beds;
		public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) {
			// Because beds have special smart interaction, this splits up the left and right side into the necessary 2x2 sections
			width = 2; // Default to the Width defined for TileObjectData.newTile
			height = 2; // Default to the Height defined for TileObjectData.newTile
						//extraY = 0; // Depends on how you set up frameHeight and CoordinateHeights and CoordinatePaddingFix.Y
		}
		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			Tile tile = Main.tile[i, j];
			int spawnX = (i - (tile.TileFrameX / 18)) + (tile.TileFrameX >= 72 ? 5 : 2);
			int spawnY = j + 2;
			if (tile.TileFrameY != 0) {
				spawnY--;
			}
			if (!Player.IsHoveringOverABottomSideOfABed(i, j)) { // This assumes your bed is 4x2 with 2x2 sections. You have to write your own code here otherwise
				if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance)) {
					player.GamepadEnableGrappleCooldown();
					player.sleeping.StartSleeping(player, i, j);
				}
			} else {
				player.FindSpawn();
				if (player.SpawnX == spawnX && player.SpawnY == spawnY) {
					player.RemoveSpawn();
					Main.NewText(Language.GetTextValue("Game.SpawnPointRemoved"), 255, 240, 20);
				} else if (Player.CheckSpawn(spawnX, spawnY)) {
					player.ChangeSpawn(spawnX, spawnY);
					Main.NewText(Language.GetTextValue("Game.SpawnPointSet"), 255, 240, 20);
				}
			}
			return true;
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			if (!Player.IsHoveringOverABottomSideOfABed(i, j)) {
				if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance)) { // Match condition in RightClick. Interaction should only show if clicking it does something
					player.noThrow = 2;
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = ItemID.SleepingIcon;
				}
			} else {
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = Item.Type;
			}
		}
	}
	public abstract class LightFurnitureBase : FurnitureBase {
		public AutoLoadingAsset<Texture2D> flameTexture;
		public virtual int flameDust => -1;
		public bool IsOn(Tile tile) => tile.TileFrameX < width * 18;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileLighted[Type] = true;
			flameTexture = Texture + "_Flame";
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
		}
		public override void HitWire(int i, int j) {
			Tile tile = Main.tile[i, j];
			int leftX = i - ((tile.TileFrameX / 18) % width);
			int topY = j - ((tile.TileFrameY / 18) % height);
			short frameAdjustment = (short)((IsOn(tile) ? 18 : -18) * width);

			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Main.tile[leftX + x, topY + y].TileFrameX += frameAdjustment;
					Wiring.SkipWire(leftX + x, topY + y);
				}
			}

			// Avoid trying to send packets in singleplayer.
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, leftX, topY, width, height, TileChangeType.None);
			}
		}
		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (width == 1 && i % 2 == 1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (!glowTexture.Exists) return;

			int width = 16;
			int offsetY = 0;
			int height = 16;
			short frameX = drawData.tileFrameX;
			short frameY = drawData.tileFrameY;

			TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY);

			Rectangle glowRect = new(frameX, frameY, width, height + offsetY);
			drawData.glowTexture = glowTexture;
			drawData.glowColor = GlowmaskColor;
			drawData.glowSourceRect = glowRect;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!flameTexture.Exists) return;
			Tile tile = Main.tile[i, j];

			if (!TileDrawing.IsVisible(tile)) return;

			SpriteEffects effects = SpriteEffects.None;

			if (this.width == 1 && i % 2 == 1) {
				effects = SpriteEffects.FlipHorizontally;
			}

			Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

			if (Main.drawToScreen) {
				zero = Vector2.Zero;
			}

			int width = 16;
			int offsetY = 0;
			int height = 16;
			short frameX = tile.TileFrameX;
			short frameY = tile.TileFrameY;

			TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY);

			ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i); // Don't remove any casts.

			// We can support different flames for different styles here: int style = Main.tile[j, i].frameY / 54;
			Texture2D flame = flameTexture.Value;
			for (int c = 0; c < 7; c++) {
				float shakeX = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
				float shakeY = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;

				spriteBatch.Draw(flame, new Vector2(i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + shakeX, j * 16 - (int)Main.screenPosition.Y + offsetY + shakeY) + zero, new Rectangle(frameX, frameY, width, height), new Color(100, 100, 100, 0), 0f, default, 1f, effects, 0f);
			}
		}
		public override void EmitParticles(int i, int j, Tile tileCache, short tileFrameX, short tileFrameY, Color tileLight, bool visible) {
			if (Main.rand.NextBool(40) && tileFrameY < 54) {
				// The following math makes dust only spawn at the tile coordinates of the flames:
				// ---
				// O-O
				// ---

				int tileColumn = tileFrameX / 18 % 3;
				if (tileFrameY / 18 % 3 == 1 && tileColumn != 1) {
					if (flameDust != -1) {
						Dust dust = Dust.NewDustDirect(new Vector2(i * 16, j * 16 + 2), 14, 6, flameDust, 0f, 0f, 100);
						if (Main.rand.NextBool(3)) {
							dust.noGravity = true;
						}

						dust.velocity *= 0.3f;
						dust.velocity.Y -= 1.5f;
					}
				}
			}
		}
	}
	public abstract class DoorBase : ModTile {
		protected internal TileItem item;
		protected internal OpenDoor openDoorTile;
		public virtual bool LavaDeath => true;
		public abstract Color MapColor { get; }
		public sealed override void Load() {
			Mod.AddContent(openDoorTile = new OpenDoor(this));
			Mod.AddContent(item = new TileItem(this));
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = LavaDeath;
			TileID.Sets.NotReallySolid[Type] = true;
			TileID.Sets.DrawsWalls[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.OpenDoorID[Type] = openDoorTile.Type;

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

			AdjTiles = [TileID.ClosedDoor];

			// Names
			AddMapEntry(MapColor, Language.GetText("MapObject.Door"));

			// Placement
			// In addition to copying from the TileObjectData.Something templates, modders can copy from specific tile types. CopyFrom won't copy subtile data, so style specific properties won't be copied, such as how Obsidian doors are immune to lava.
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.ClosedDoor, 0));
			TileObjectData.newTile.LavaDeath = LavaDeath;
			TileObjectData.addTile(Type);
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 1;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = item.Type;
		}
	}
	[Autoload(false)]
	public class OpenDoor : ModTile {
		readonly DoorBase closedDoorTile;
		public override string Name => closedDoorTile.Name + "_Open";
		public override string Texture => closedDoorTile.Texture + "_Open";
		public OpenDoor(DoorBase closedDoor) : base() {
			closedDoorTile = closedDoor;
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileLavaDeath[Type] = closedDoorTile.LavaDeath;
			Main.tileNoSunLight[Type] = true;
			TileID.Sets.HousingWalls[Type] = true; // needed for non-solid blocks to count as walls
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.CloseDoorID[Type] = closedDoorTile.Type;

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

			DustType = closedDoorTile.DustType;
			AdjTiles = [TileID.OpenDoor];
			// Tiles usually drop their corresponding item automatically, but RegisterItemDrop is needed here since the ExampleDoor item places ExampleDoorClosed, not this tile.
			RegisterItemDrop(closedDoorTile.item.Type, 0);

			// Names
			AddMapEntry(closedDoorTile.MapColor, Language.GetText("MapObject.Door"));

			// Placement
			// In addition to copying from the TileObjectData.Something templates, modders can copy from specific tile types. CopyFrom won't copy subtile data, so style specific properties won't be copied, such as how Obsidian doors are immune to lava.
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(0, 1);
			TileObjectData.addAlternate(0);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(0, 2);
			TileObjectData.addAlternate(0);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(1, 0);
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
			TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(1);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(1, 1);
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
			TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(1);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Origin = new Point16(1, 2);
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, 1, 1);
			TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 1);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.addAlternate(1);
			TileObjectData.newTile.LavaDeath = closedDoorTile.LavaDeath;
			TileObjectData.addTile(Type);
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = 1;
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = closedDoorTile.item.Type;
		}
	}
	public abstract class Platform_Tile : FurnitureBase {
		public sealed override int BaseTileID => TileID.Platforms;
		public override Color MapColor => new(190, 141, 110);
		public override void SetStaticDefaults() {
			// Properties
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.Platforms[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			AdjTiles = [TileID.Platforms];

			// Placement
			TileObjectData.newTile.CoordinateHeights = [16];
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleMultiplier = 27;
			TileObjectData.newTile.StyleWrapLimit = 27;
			TileObjectData.newTile.UsesCustomCanPlace = false;
			TileObjectData.newTile.LavaDeath = LavaDeath;
			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

			if (!Main.dedServ) {
				LocalizedText text = Lang._mapLegendCache.FromType(BaseTileID);
				if (string.IsNullOrEmpty(text.Value)) AddMapEntry(MapColor);
				else AddMapEntry(MapColor, text, MapName);
			}

			glowTexture = Texture + "_Glow";
		}
	}
	public abstract class CageBase<TCritter>(int cage = ItemID.Terrarium) : CageBase(cage) where TCritter : ModItem {
		public override int IngredientItem => ModContent.ItemType<TCritter>();
	}
	public abstract class CageBase(int cage = ItemID.Terrarium) : ModTile {

		protected internal TileItem item;
		public abstract int IngredientItem {  get; }
		public virtual bool LavaDeath => true;
		public virtual Color MapColor => new(122, 217, 232);
		/// <inheritdoc	cref="TileID.Sets.CritterCageLidStyle"/>
		public abstract int LidType { get; }
		public abstract CageKinds CageKind { get; }
		private static readonly Dictionary<CageKinds, (int baseTileID, TileObjectData baseTileData, int drawYOffset)> CageKindMap = new() {
			[CageKinds.SmallCage] = (TileID.CageBuggy, TileObjectData.StyleSmallCage, 1),
			[CageKinds.BigCage] = (TileID.BunnyCage, TileObjectData.Style6x3, 2),
			[CageKinds.Jar] = (TileID.MonarchButterflyJar, TileObjectData.Style2x2, 1)
		};
		public virtual int[] FrameIndexArray => new int[Main.cageFrames];
		public int[] FrameCounter = new int[Main.cageFrames];
		protected AutoLoadingAsset<Texture2D> glowTexture;
		public virtual Color GlowmaskColor => Color.White;
		private int offsetY;
		public sealed override void Load() {
			Mod.AddContent(item = new TileItem(this).WithExtraDefaults(item => {
				item.width = 32;
				item.height = 32;
				item.value = 0;
			}).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(IngredientItem)
				.AddIngredient(cage)
				.Register();
			}));
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileLavaDeath[Type] = LavaDeath;
			TileID.Sets.CritterCageLidStyle[Type] = LidType;

			AdjTiles = [CageKindMap[CageKind].baseTileID];
			DustType = DustID.Glass;

			// Names
			if (!Main.dedServ) AddMapEntry(MapColor, item.DisplayName);

			// Placement
			// In addition to copying from the TileObjectData.Something templates, modders can copy from specific tile types. CopyFrom won't copy subtile data, so style specific properties won't be copied, such as how Obsidian doors are immune to lava.
			TileObjectData.newTile.CopyFrom(CageKindMap[CageKind].baseTileData);
			TileObjectData.newTile.LavaDeath = LavaDeath;
			TileObjectData.newTile.DrawYOffset = CageKindMap[CageKind].drawYOffset;
			AnimationFrameHeight = TileObjectData.newTile.CoordinateFullHeight;
			TileObjectData.addTile(Type);
			glowTexture = Texture + "_Glow";
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 6 : 3;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			ExtraAnimate();
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Main.critterCage = true;
			Tile tile = Framing.GetTileSafely(i, j);
			int frameIndex;
			if (CageKind != CageKinds.BigCage) frameIndex = TileDrawing.GetSmallAnimalCageFrame(i, j, tile.TileFrameX, tile.TileFrameY);
			else frameIndex = TileDrawing.GetBigAnimalCageFrame(i, j, tile.TileFrameX, tile.TileFrameY);
			frameYOffset = offsetY = FrameIndexArray[frameIndex] * AnimationFrameHeight;
		}
		public virtual void ExtraAnimate() { }
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (!glowTexture.Exists) return;
			Rectangle glowRect = new(drawData.tileFrameX, drawData.tileFrameY + offsetY, drawData.tileWidth, drawData.tileHeight);
			drawData.glowTexture = glowTexture;
			drawData.glowColor = GlowmaskColor;
			drawData.glowSourceRect = glowRect;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
		}
		public enum CageKinds {
			SmallCage,
			BigCage,
			Jar
		}
	}
	public abstract class ModGemLock : ModTile {
		protected internal TileItem item;
		public abstract int GemType { get; }
		public override void Load() {
			Mod.AddContent(item = new TileItem(this).WithExtraDefaults(item => {
				item.width = 20;
				item.height = 20;
				item.value = Item.sellPrice(0, 0, 1);
			}));
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileID.Sets.AvoidedByNPCs[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			AddMapEntry(new(55, 204, 212), CreateMapEntryName());

			// Placement
			// In addition to copying from the TileObjectData.Something templates, modders can copy from specific tile types. CopyFrom won't copy subtile data, so style specific properties won't be copied, such as how Obsidian doors are immune to lava.
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.addTile(Type);
		}
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			int self = TileLoader.GetItemDropFromTypeAndStyle(Type, 0);
			yield return new Item(self);
			if (Framing.GetTileSafely(i, j).TileFrameY >= 54) yield return new Item(GemType);
		}
		public override void MouseOver(int i, int j) {
			int type = GemType;
			Player player = Main.LocalPlayer;
			if (type != -1 && ((Main.tile[i, j].TileFrameY / 54) == 1 || player.HasItem(type))) {
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = type;
			}
		}
		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			int frameY = Main.tile[i, j].TileFrameY / 54;
			int type = GemType;

			if (type != -1) {
				if (frameY == 0 && player.HasItem(type) && player.selectedItem != 58) {
					player.GamepadEnableGrappleCooldown();
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						player.ConsumeItem(type);
						ToggleGemLock(i, j, on: true);
					} else {
						player.ConsumeItem(type);
						ModPacket packet = Origins.instance.GetPacket();
						packet.Write(Origins.NetMessageType.set_gem_lock);
						packet.Write((short)i);
						packet.Write((short)j);
						packet.Write(true);
						packet.Send();
					}
				} else if (frameY == 1) {
					player.GamepadEnableGrappleCooldown();
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						ToggleGemLock(i, j, on: false);
					} else {
						ModPacket packet = Origins.instance.GetPacket();
						packet.Write(Origins.NetMessageType.set_gem_lock);
						packet.Write((short)i);
						packet.Write((short)j);
						packet.Write(false);
						packet.Send();
					}
				}
			}
			return true;
		}
		public void ToggleGemLock(int i, int j, bool on) {

			bool alreadyOn = false;
			int type = GemType;
			Tile tile = Framing.GetTileSafely(i, j);
			if (!tile.HasTile || (tile.TileFrameY < 54 && !on)) return;
			if (tile.TileFrameY >= 54) alreadyOn = true;

			int xOffset = tile.TileFrameX % 54 / 18;
			int yOffset = tile.TileFrameY % 54 / 18;

			for (int k = i - xOffset; k < i - xOffset + 3; k++) {
				for (int l = j - yOffset; l < j - yOffset + 3; l++) {
					Main.tile[k, l].TileFrameY = (short)((on ? 54 : 0) + (l - j + yOffset) * 18);
				}
			}

			if (type != -1 && alreadyOn)
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32, type);

			WorldGen.SquareTileFrame(i, j);
			NetMessage.SendTileSquare(-1, i - xOffset, j - yOffset, 3, 3);
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i + 1 - xOffset, j + 1 - yOffset) * 16);
			Wiring.TripWire(i - xOffset, j - yOffset, 3, 3);
			NetMessage.SendData(MessageID.HitSwitch, -1, -1, null, i - xOffset, j - yOffset);
		}
	}
	[Autoload(false)]
	public class TileItem(ModTile tile) : ModItem() {
		readonly ModTile tile = tile;
		public override string Name => tile.Name + "_Item";
		public override string Texture => tile.Texture + "_Item";
		public event Action<Item> ExtraDefaults;
		public event Action<Item> OnAddRecipes;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(tile.Type);
			Item.width = 14;
			Item.height = 28;
			Item.value = 150;
			if (ExtraDefaults is not null) {
				ExtraDefaults(Item);
				ExtraDefaults = null;
			}
		}
		public override void AddRecipes() {
			if (OnAddRecipes is not null) {
				OnAddRecipes(Item);
				OnAddRecipes = null;
			}
		}
		public TileItem WithExtraDefaults(Action<Item> extra) {
			ExtraDefaults += extra;
			return this;
		}
		public TileItem WithOnAddRecipes(Action<Item> recipes) {
			OnAddRecipes += recipes;
			return this;
		}
	}
}
