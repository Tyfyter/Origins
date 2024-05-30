using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.MusicBoxes {
	#region base classes
	public abstract class Music_Box : ModTile {
		public abstract Color MapColor { get; }
		public abstract int MusicSlot { get; }
		public virtual new int DustType => 0;
		protected int itemType = -1;
		public override void Load() {
			Music_Box_Item item = new(this);
			Mod.AddContent(item);
			itemType = item.Type;
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile(Type);
			TileID.Sets.DisableSmartCursor[Type] = true;
			AddMapEntry(MapColor, Language.GetOrRegister("Mods.Origins.Tiles." + Name));

			// The following code links the music box's item and tile with a music track:
			//   When music with the given ID is playing, equipped music boxes have a chance to change their id to the given item type.
			//   When an item with the given item type is equipped, it will play the music that has musicSlot as its ID.
			//   When a tile with the given type and Y-frame is nearby, if its X-frame is >= 36, it will play the music that has musicSlot as its ID.
			// When getting the music slot, you should not add the file extensions!
			MusicLoader.AddMusicBox(Mod, MusicSlot, itemType, Type);
			RegisterItemDrop(itemType);
			base.DustType = this.DustType;
		}
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = itemType;
		}
	}
	[Autoload(false)]
	public class Music_Box_Item : ModItem {
		[CloneByReference]
		Music_Box tile;
		public override string Name => tile.Name + "_Item";
		public override LocalizedText DisplayName => Language.GetOrRegister("Mods.Origins.Tiles." + tile.Name);
		public override LocalizedText Tooltip => LocalizedText.Empty;
		protected override bool CloneNewInstances => true;
		public Music_Box_Item(Music_Box tile) : base() {
			this.tile = tile;
		}
		public override void SetStaticDefaults() {
			ItemID.Sets.CanGetPrefixes[Type] = false; // music boxes can't get prefixes in vanilla
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox; // recorded music boxes transform into the basic form in shimmer
		}

		public override void SetDefaults() {
			Item.DefaultToMusicBox(tile.Type, 0);
		}
	}
	#endregion
	public class Music_Box_DW : Music_Box {
		public string[] Categories => new string[] {
			"MusicBox"
		};
		public override Color MapColor => new Color(255, 255, 255);
		public override int MusicSlot => Origins.Music.Defiled;
		public override int DustType => Defiled_Wastelands.DefaultTileDust;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			AnimationFrameHeight = 36;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 8) {
				frameCounter = 0;
				frame = ++frame % 4;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			if (Main.tile[i, j].TileFrameX < 36) frameYOffset = 0;
		}
	}
	public class Music_Box_DC : Music_Box {
		public string[] Categories => new string[] {
			"MusicBox"
		};
		public override Color MapColor => new Color(255, 255, 255);
		public override int MusicSlot => Origins.Music.UndergroundDefiled;
		public override int DustType => Defiled_Wastelands.DefaultTileDust;
	}
	public class Music_Box_RH : Music_Box, IGlowingModTile {
		public string[] Categories => new string[] {
			"MusicBox"
		};
		public override Color MapColor => new Color(42, 59, 112);
		public override int MusicSlot => Origins.Music.Riven;
		public override int DustType => Riven_Hive.DefaultTileDust;
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public static float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public static float GlowLightValue(Tile tile) {
			int frameNumberOffset = tile.TileFrameX >= 36 ? 1 : 0;
			return GlowValue * ((frameNumberOffset + tile.TileFrameNumber) / 4f);
		}
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public static bool ShouldGlow(Tile tile) {
			if (tile.TileFrameY == 0) {
				if (tile.TileFrameX % 36 < 16) return false;
				return tile.TileFrameNumber > 1;
			} else {
				return tile.TileFrameNumber > 0 || tile.Slope > 0;
			}
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			TileID.Sets.HasSlopeFrames[Type] = true;
			Main.tileLighted[Type] = true;
			if (!Main.dedServ) {
				GlowTexture = Request<Texture2D>(Texture + "_Glow");
			}
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (ShouldGlow(tile)) color = new Vector3(0.394f, 0.879f, 0.912f) * GlowLightValue(tile);
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowColor = GlowColor;
			drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY, 18, 18);
			drawData.glowTexture = GlowTexture;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Tile tile = Main.tile[i, j];

			// If the torch is on
			if (ShouldGlow(tile)) {
				float glowLightValue = GlowLightValue(tile);
				r = 0.05f * glowLightValue;
				g = 0.35f * glowLightValue;
				b = 0.32f * glowLightValue;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			int frameTime = (int)tile.Slope;
			if (tile.TileFrameX >= 36) {
				frameYOffset += tile.TileFrameNumber * 36;
				if (tile.TileFrameNumber < 3) {
					if (frameTime >= 5) {
						tile.TileFrameNumber++;
						tile.Slope = 0;
					} else {
						tile.Slope = (SlopeType)(frameTime + 1);
					}
				}
			} else if(tile.TileFrameNumber > 0) {
				frameXOffset += 36;
				frameYOffset += (tile.TileFrameNumber - 1) * 36;
				if (tile.TileFrameNumber > 0) {
					if (frameTime >= 5) {
						tile.TileFrameNumber--;
						tile.Slope = 0;
					} else {
						tile.Slope = (SlopeType)(frameTime + 1);
					}
				}
			}
		}
		public override void Load() {
			base.Load();
			this.SetupGlowKeys();
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}