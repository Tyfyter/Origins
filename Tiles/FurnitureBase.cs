using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
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
		protected internal TileItem item;
		public sealed override void Load() {
			Mod.AddContent(item = new TileItem(this));
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = LavaDeath;

			Main.tileTable[Type] = Main.tileTable[BaseTileID];
			Main.tileSolidTop[Type] = Main.tileSolidTop[BaseTileID];
			Main.tileNoAttach[Type] = Main.tileNoAttach[BaseTileID];
			TileID.Sets.HasOutlines[Type] = TileID.Sets.HasOutlines[BaseTileID];
			TileID.Sets.DisableSmartCursor[Type] = TileID.Sets.DisableSmartCursor[BaseTileID];
			TileID.Sets.IgnoredByNpcStepUp[Type] = TileID.Sets.IgnoredByNpcStepUp[BaseTileID];

			TileID.Sets.CanBeSleptIn[Type] = TileID.Sets.CanBeSleptIn[BaseTileID];
			TileID.Sets.CanBeSatOnForPlayers[Type] = TileID.Sets.CanBeSatOnForPlayers[BaseTileID];
			TileID.Sets.CanBeSatOnForNPCs[Type] = TileID.Sets.CanBeSatOnForNPCs[BaseTileID];
			//TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(BaseTileID, 0));
			TileObjectData.newTile.LavaDeath = LavaDeath;
			if (TileObjectData.newTile.Direction != TileObjectDirection.None) {
				TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
				TileObjectData.newAlternate.Direction = TileObjectData.newTile.Direction ^ (TileObjectDirection.PlaceLeft | TileObjectDirection.PlaceRight);
				TileObjectData.addAlternate(1);
			}
			TileObjectData.addTile(Type);
			
			if (!Main.dedServ) AddMapEntry(MapColor, Lang._mapLegendCache.FromType(BaseTileID));
			AdjTiles = new int[] {
				BaseTileID
			};
			if (TileID.Sets.RoomNeeds.CountsAsTable.Contains(BaseTileID)) AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			if (TileID.Sets.RoomNeeds.CountsAsChair.Contains(BaseTileID)) AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
		}
	}
	public abstract class ChairBase : FurnitureBase {
		public override int BaseTileID => TileID.Chairs;
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
			player.cursorItemIconID = item.Type;

			if (Main.tile[i, j].TileFrameX / 18 < 1) {
				player.cursorItemIconReversed = true;
			}
		}
		public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info) {
			// It is very important to know that this is called on both players and NPCs, so do not use Main.LocalPlayer for example, use info.restingEntity
			Tile tile = Framing.GetTileSafely(i, j);

			//info.directionOffset = info.restingEntity is Player ? 6 : 2; // Default to 6 for players, 2 for NPCs
			//info.visualOffset = Vector2.Zero; // Defaults to (0,0)

			info.TargetDirection = -1;

			if (tile.TileFrameX != 0) {
				info.TargetDirection = 1; // Facing right if sat down on the right alternate (added through addAlternate in SetStaticDefaults earlier)
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

			AdjTiles = new int[] { TileID.GrandfatherClocks };

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
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
	public abstract class LightFurnitureBase : FurnitureBase {
		int width, height;
		AutoLoadingAsset<Texture2D> flameTexture;
		public bool IsOn(Tile tile) => tile.TileFrameX < width * 18;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.tileLighted[Type] = true;
			TileObjectData data = TileObjectData.GetTileData(Type, 0);
			width = data.Width;
			height = data.Height;
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

			AdjTiles = new int[] { TileID.ClosedDoor };

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
			AdjTiles = new int[] { TileID.OpenDoor };
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
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
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
	[Autoload(false)]
	public class TileItem : ModItem {
		readonly ModTile tile;
		public override string Name => tile.Name + "_Item";
		public override string Texture => tile.Texture + "_Item";
		public event Action<Item> ExtraDefaults;
		public event Action<Item> OnAddRecipes;
		protected override bool CloneNewInstances => true;
		public TileItem(ModTile tile) : base() {
			this.tile = tile;
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
	}
}
