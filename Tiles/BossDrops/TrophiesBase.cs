using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ThoriumMod.Items.Placeable.Relics;

namespace Origins.Tiles.BossDrops {
	[Autoload(false)]
	public class TrophyItem(TrophyTileBase tile) : ModItem {
		[field: CloneByReference]
		public TrophyTileBase Tile { get; } = tile;
		public override string Name => Tile.Name + "_Item";
		public override string Texture => Tile.Texture + "_Item";
		public override LocalizedText Tooltip => LocalizedText.Empty;
		protected override bool CloneNewInstances => true;
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 50000;
			Item.rare = ItemRarityID.Blue;
			Item.createTile = Tile.Type;
		}
	}
	public abstract class TrophyTileBase : ModTile {
		public static int ItemType<T>() where T : TrophyTileBase => ModContent.GetInstance<T>().Item.Type;
		public TrophyItem Item { get; private set; }
		public override void Load() {
			Mod.AddContent(Item = new TrophyItem(this));
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(120, 85, 60), Language.GetText("MapObject.Trophy"));
			DustType = DustID.WoodFurniture;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
		}
	}
	[Autoload(false)]
	public class RelicItem(RelicTileBase tile) : ModItem {
		[field: CloneByReference]
		public RelicTileBase Tile { get; } = tile;
		public override string Name => Tile.Name + "_Item";
		public override string Texture => Tile.RelicTextureName + "_Item";
		public override LocalizedText Tooltip => LocalizedText.Empty;
		protected override bool CloneNewInstances => true;
		public override void SetDefaults() {
			// Vanilla has many useful methods like these, use them! This substitutes setting Item.createTile and Item.placeStyle as well as setting a few values that are common across all placeable items
			// The place style (here by default 0) is important if you decide to have more than one relic share the same tile type (more on that in the tiles' code)
			Item.DefaultToPlaceableTile(Tile.Type, 0);

			Item.width = 30;
			Item.height = 40;
			Item.rare = ItemRarityID.Master;
			Item.master = true; // This makes "Master" display in the tooltip and overrides the rarity's text color
			Item.value = Item.buyPrice(0, 5);
		}
	}
	public abstract class RelicTileBase : ModTile {
		public static int ItemType<T>() where T : RelicTileBase => ModContent.GetInstance<T>().Item.Type;
		public RelicItem Item { get; private set; }
		public const int FrameHeight = 18 * 4;

		public Asset<Texture2D> RelicTexture;

		// Every relic has its own extra floating part, should be 50x50. Optional: Expand this sheet if you want to add more, stacked vertically
		// If you do not use the Item.placeStyle approach, and you extend from this class, you can override this to point to a different texture
		public virtual string RelicTextureName => base.Texture;
		public override string Texture => "Terraria/Images/Tiles_" + TileID.MasterTrophyBase;

		public override void Load() {
			Mod.AddContent(Item = new RelicItem(this));
			if (!Main.dedServ) RelicTexture = ModContent.Request<Texture2D>(RelicTextureName);
		}
		public override void SetStaticDefaults() {
			Main.tileShine[Type] = 400; // Responsible for golden particles
			Main.tileFrameImportant[Type] = true; // Any multitile requires this
			TileID.Sets.InteractibleByNPCs[Type] = true; // Town NPCs will palm their hand at this tile

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4); // Relics are 3x4
			TileObjectData.newTile.LavaDeath = false; // Does not break when lava touches it
			TileObjectData.newTile.DrawYOffset = 2; // So the tile sinks into the ground
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft; // Player faces to the left
			TileObjectData.newTile.StyleHorizontal = false; // Based on how the alternate sprites are positioned on the sprite (by default, true)

			// This controls how styles are laid out in the texture file. This tile is special in that all styles will use the same texture section to draw the pedestal.
			TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.styleLineSkipVisualOverride = 0; // This forces the tile preview to draw as if drawing the 1st style.

			// Register an alternate tile data with flipped direction
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile); // Copy everything from above, saves us some code
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight; // Player faces to the right
			TileObjectData.addAlternate(1);

			// Register the tile data itself
			TileObjectData.addTile(Type);

			// Register map name and color
			// "MapObject.Relic" refers to the translation key for the vanilla "Relic" text
			AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.Relic"));
			AdjTiles = [TileID.MasterTrophyBase];
		}

		public override bool CreateDust(int i, int j, ref int type) => false;

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			// Since this tile does not have the hovering part on its sheet, we have to animate it ourselves
			// Therefore we register the top-left of the tile as a "special point"
			// This allows us to draw things in SpecialDraw
			if (drawData.tileFrameX == 0 && drawData.tileFrameY == 0) {
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			// Take the tile, check if it actually exists
			Point p = new(i, j);
			Tile tile = Main.tile[p.X, p.Y];
			if (!tile.HasTile) return;

			// Get the initial draw parameters
			Texture2D texture = RelicTexture.Value;

			Rectangle frame = texture.Bounds;

			Vector2 origin = frame.Size() / 2f;
			Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);

			Color color = Lighting.GetColor(p.X, p.Y);

			bool direction = tile.TileFrameY / FrameHeight != 0; // This is related to the alternate tile data we registered before
			SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			// Some math magic to make it smoothly move up and down over time
			const float TwoPi = (float)Math.PI * 2f;
			float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);
			Vector2 drawPos = worldPos - Main.screenPosition + new Vector2(0f, -40f) + new Vector2(0f, offset * 4f);

			// Draw the main texture
			spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

			// Draw the periodic glow effect
			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 2f) * 0.3f + 0.7f;
			Color effectColor = color;
			effectColor.A = 0;
			effectColor = effectColor * 0.1f * scale;
			for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI)) {
				spriteBatch.Draw(texture, drawPos + (TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame, effectColor, 0f, origin, 1f, effects, 0f);
			}
		}

		// One drawback of relic tiles is that the placement preview doesn't show the relic itself (only the pedestal) since the relic would normally be manually drawn. We can use PostDrawPlacementPreview to draw the relic during tile placement.
		public override void PostDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, Rectangle frame, Vector2 position, Color color, bool validPlacement, SpriteEffects spriteEffects) {
			// Adjust the draw coordinates in case the preview is drawing the placement facing right.
			bool facingRight = frame.Y / FrameHeight != 0;
			spriteEffects = facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			frame.Y %= FrameHeight;

			// Convert from tile coordinates with tile padding to the corresponding RelicTexture coordinates.
			frame.X = frame.X / 18 * 16;
			frame.Y = frame.Y / 18 * 16;
			if (facingRight && frame.X != 16) {
				// We also need to swap the 1st and 3rd columns to render correctly.
				frame.X = frame.X == 0 ? 32 : 0;
			}

			// Finally, manually draw a section of the relic texture. Note that color will be White or Red so the individual sections that conflict with existing tiles will show as Red, just like the normal placement preview using the actual tile texture.
			spriteBatch.Draw(RelicTexture.Value, position, frame, color, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
		}
	}
}