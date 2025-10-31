using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Graphics;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Reflection;
using Origins.World.BiomeData;
using PegasusLib;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.MusicBoxes {
	#region base classes
	public abstract class Music_Box : ModTile {
		public virtual string[] Categories => [];
		public abstract Color MapColor { get; }
		public abstract int MusicSlot { get; }
		public virtual new int DustType => 0;
		public Music_Box_Item Item { get; private set; }
		public static int ItemType<TMusicBox>() where TMusicBox : Music_Box => GetInstance<TMusicBox>().Item.Type;
		static List<Music_Box> musicBoxes = [];
		public static ReadOnlySpan<Music_Box> MusicBoxes => CollectionsMarshal.AsSpan(musicBoxes);
		static FastStaticFieldInfo<MusicLoader, Dictionary<int, int>> musicToItem = new(nameof(musicToItem), BindingFlags.NonPublic);
		static FastStaticFieldInfo<MusicLoader, Dictionary<int, int>> itemToMusic = new(nameof(itemToMusic), BindingFlags.NonPublic);
		static FastStaticFieldInfo<MusicLoader, Dictionary<int, Dictionary<int, int>>> tileToMusic = new(nameof(tileToMusic), BindingFlags.NonPublic);
		/// <summary>
		/// For OriginsMusic
		/// </summary>
		public static void ReloadMusicAssociations() {
			Dictionary<int, int> musicToItem = Music_Box.musicToItem.Value;
			Dictionary<int, int> itemToMusic = Music_Box.itemToMusic.Value;
			Dictionary<int, Dictionary<int, int>> tileToMusic = Music_Box.tileToMusic.Value;
			foreach (Music_Box box in MusicBoxes) {
				if (itemToMusic.TryGetValue(box.Item.Type, out int oldTrack) && oldTrack >= MusicID.Count) {
					musicToItem.Remove(oldTrack);
					itemToMusic.Remove(box.Item.Type);
					tileToMusic.Remove(box.Type);
				}
				if (box.MusicSlot >= MusicID.Count) {
					musicToItem[box.MusicSlot] = box.Item.Type;
					itemToMusic[box.Item.Type] = box.MusicSlot;
					tileToMusic[box.Type] = new() { [0] = box.MusicSlot };
				}
			}
		}
		public override void Load() {
			Item = CreateItem();
			Mod.AddContent(Item);
			musicBoxes.Add(this);
		}
		public override void Unload() {
			musicBoxes = null;
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
			TileID.Sets.HasOutlines[Type] = true;
			AddMapEntry(MapColor, Language.GetOrRegister("Mods.Origins.Tiles." + Name, PrettyPrintName));
			RegisterItemDrop(Item.Type);
			base.DustType = this.DustType;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = Item.Type;
		}
		public virtual Music_Box_Item CreateItem() => new(this);
	}
	[Autoload(false)]
	public class Music_Box_Item(Music_Box tile) : ModItem(), ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.MusicBox,
			..tile.Categories
		];
		[CloneByReference]
		readonly Music_Box tile = tile;
		public override string Name => tile.Name + "_Item";
		public override LocalizedText DisplayName => Language.GetText("Mods.Origins.Tiles." + tile.Name);
		public override LocalizedText Tooltip => LocalizedText.Empty;
		protected sealed override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			ItemID.Sets.CanGetPrefixes[Type] = false; // music boxes can't get prefixes in vanilla
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.MusicBox; // recorded music boxes transform into the basic form in shimmer
		}
		public override void SetDefaults() {
			Item.DefaultToMusicBox(tile.Type, 0);
			Item.maxStack = 1;
		}
		public override bool? PrefixChance(int pre, UnifiedRandom rand) {
			return false;
		}
		public void ModifyWikiStats(JObject data) {
			Dictionary<string, int> musicByPath = MusicMethods.musicByPath.GetValue();
			Dictionary<string, string> musicExtensions = MusicMethods.musicExtensions.GetValue();
			foreach (KeyValuePair<string, int> item in musicByPath) {
				if (item.Value == tile.MusicSlot && item.Key.StartsWith("OriginsMusic/Sounds/Music/")) {
					data["Music"] = item.Key.Replace("OriginsMusic/Sounds/Music/", "") + musicExtensions[item.Key];
					break;
				}
			}
		}
		public bool CanExportStats => ModLoader.HasMod("OriginsMusic");
	}
	#endregion
	public class Music_Box_DW : Music_Box {
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
		public override Color MapColor => new Color(255, 255, 255);
		public override int MusicSlot => Origins.Music.UndergroundDefiled;
		public override int DustType => Defiled_Wastelands.DefaultTileDust;
	}
	public class Music_Box_RH : Music_Box, IGlowingModTile {
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
			if (ShouldGlow(tile)) color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowLightValue(tile));
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowColor = GlowColor;
			drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY, 18, 18);
			drawData.glowTexture = this.GetGlowTexture(drawData.tileCache.TileColor);
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
			} else if (tile.TileFrameNumber > 0) {
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
	public class Music_Box_BP : Music_Box, IGlowingModTile {
		public override Color MapColor => new Color(42, 112, 59);
		public override int MusicSlot => Origins.Music.BrinePool;
		public override int DustType => DustID.GreenMoss;
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public static float GlowLightValue(Tile tile) => tile.TileFrameX >= 36 ? 1 : 0;
		public Color GlowColor => Color.White;
		public static bool ShouldGlow(Tile tile) => tile.TileFrameX >= 36;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			TileID.Sets.HasSlopeFrames[Type] = true;
			Main.tileLighted[Type] = true;
			AnimationFrameHeight = 36;
			if (!Main.dedServ) {
				GlowTexture = Request<Texture2D>(Texture + "_Glow");
			}
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (ShouldGlow(tile)) color = Vector3.Max(color, new Vector3(0f, 0.45f, 0.2f) * GlowLightValue(tile));
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowColor = GlowColor;
			drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY + Main.tileFrame[Type] * AnimationFrameHeight, 18, 18);
			drawData.glowTexture = GlowTexture;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Tile tile = Main.tile[i, j];

			// If the torch is on
			if (ShouldGlow(tile)) {
				float glowLightValue = GlowLightValue(tile);
				r = 0f;
				g = 0.2f * glowLightValue;
				b = 0.05f * glowLightValue;
			}
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 8) {
				frameCounter = 0;
				frame = ++frame % 3;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			if (Main.tile[i, j].TileFrameX < 36) frameYOffset = 0;
		}
		public override void Load() {
			base.Load();
			this.SetupGlowKeys();
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Ancient_Music_Box_DW : Music_Box {
		public override string[] Categories => [
			WikiCategories.Hardmode
		];
		public override Color MapColor => new(255, 255, 255);
		public override int MusicSlot => Origins.Music.AncientDefiled;
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
	public class Ancient_Music_Box_RH : Music_Box, IGlowingModTile {
		public override string[] Categories => [
			WikiCategories.Hardmode
		];
		public override Color MapColor => new(255, 255, 255);
		public override int MusicSlot => Origins.Music.AncientRiven;
		public override int DustType => DustID.SolarFlare;
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public static float GlowLightValue(Tile tile) {
			int frameNumberOffset = tile.TileFrameX >= 36 ? 1 : 0;
			return 1 * ((frameNumberOffset + tile.TileFrameNumber) / 4f);
		}
		public Color GlowColor => Color.White;
		public static bool ShouldGlow(Tile tile) {
			if (tile.TileFrameY == 0) {
				if (tile.TileFrameX % 36 < 16) return false;
				return tile.TileFrameNumber > 1;
			} else {
				return tile.TileFrameNumber > 0 || tile.Slope > 0;
			}
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (ShouldGlow(tile)) color = Vector3.Max(color, new Vector3(0.912f, 0.879f, 0.394f) * GlowLightValue(tile));
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			TileID.Sets.HasSlopeFrames[Type] = true;
			Main.tileLighted[Type] = true;
			if (!Main.dedServ) {
				GlowTexture = Request<Texture2D>(Texture + "_Glow");
			}
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Tile tile = Main.tile[i, j];

			// If the torch is on
			if (ShouldGlow(tile)) {
				float glowLightValue = GlowLightValue(tile);
				r = 0.35f * glowLightValue;
				g = 0.32f * glowLightValue;
				b = 0.05f * glowLightValue;
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
			} else if (tile.TileFrameNumber > 0) {
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
	public class Ancient_Music_Box_BP : Music_Box, IGlowingModTile {
		public override Color MapColor => new Color(42, 112, 59);
		public override int MusicSlot => Origins.Music.AncientBrinePool;
		public override int DustType => DustID.GreenMoss;
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public static float GlowLightValue(Tile tile) {
			int frameNumberOffset = tile.TileFrameX >= 36 ? 1 : 0;
			return ((frameNumberOffset + tile.TileFrameNumber) / 3f);
		}
		public Color GlowColor => Color.White;
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
			if (ShouldGlow(tile)) color = Vector3.Max(color, new Vector3(0f, 0.912f, 0.394f) * GlowLightValue(tile));
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
				r = 0f;
				g = 0.35f * glowLightValue;
				b = 0.1f * glowLightValue;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			int frameTime = (int)tile.Slope;
			if (tile.TileFrameX >= 36) {
				frameYOffset += tile.TileFrameNumber * 36;
				if (tile.TileFrameNumber < 2) {
					if (frameTime >= 5) {
						tile.TileFrameNumber++;
						tile.Slope = 0;
					} else {
						tile.Slope = (SlopeType)(frameTime + 1);
					}
				}
			} else if (tile.TileFrameNumber > 0) {
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
	public class Music_Box_FU : Music_Box {
		public override Color MapColor => new Color(146, 253, 250);
		public override int MusicSlot => Origins.Music.Fiberglass;
		public override int DustType => DustID.Glass;
	}
	public class Music_Box_TD : Music_Box {
		public override Color MapColor => new(87, 35, 178);
		public override int MusicSlot => Origins.Music.TheDive;
		public override int DustType => DustID.GemAmethyst;
		public override Music_Box_Item CreateItem() => new Music_Box_TD_Item(this);
		public class Music_Box_TD_Item(Music_Box tile) : Music_Box_Item(tile) {
			AutoLoadingAsset<Texture2D> glowTexture = "Terraria/Images/Misc/Perlin";
			public static ArmorShaderData Shader { get; private set; }
			bool ArabelCage = false;
			public override void SetStaticDefaults() {
				Shader = new ArmorShaderData(
					Mod.Assets.Request<Effect>("Effects/Item_Caustics"),
					"The_Dive"
				);
			}
			public override void OnSpawn(IEntitySource source) => ArabelCage = source.Context == "ArabelCage";
			public override void Update(ref float gravity, ref float maxFallSpeed) {
				if (ArabelCage) {
					if (!NPC.AnyNPCs(NPCType<Shimmer_Construct>())) {
						Item.TurnToAir();
					} else if (!Item.shimmered) {
						Item.shimmered = true;
						Item.shimmerTime = 1;
					}
				}
			}
			public override bool OnPickup(Player player) {
				ArabelCage = false;
				return base.OnPickup(player);
			}
			public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
				if (ArabelCage) {
					SpriteBatchState state = spriteBatch.GetState();
					try {
						spriteBatch.Restart(state, sortMode: SpriteSortMode.Immediate);
						Texture2D texture = glowTexture;
						DrawData data = new() {
							texture = texture,
							position = Item.Center - Main.screenPosition,
							color = Color.Plum,
							rotation = 0f,
							scale = new Vector2(scale),
							origin = texture.Size() * 0.5f
						};
						Shader.Apply(Item, data);
						data.Draw(spriteBatch);
					} finally {
						spriteBatch.Restart(state);
					}
				}
			}
			public override void NetSend(BinaryWriter writer) {
				writer.Write(ArabelCage);
			}
			public override void NetReceive(BinaryReader reader) {
				ArabelCage = reader.ReadBoolean();
			}
		}
	}
	public class Otherworldly_Music_Box_DW : Music_Box {
		public override string[] Categories => [
			WikiCategories.Hardmode
		];
		public override Color MapColor => new(255, 255, 255);
		public override int MusicSlot => Origins.Music.OtherworldlyDefiled;
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
	public class Music_Box_AS : Music_Box, IGlowingModTile {
		public override Color MapColor => FromHexRGB(0x460013);
		public override int MusicSlot => Origins.Music.AshenScrapyard;
		public override int DustType => Defiled_Wastelands.DefaultTileDust;
		public override void Load() {
			base.Load();
			this.SetupGlowKeys();
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			if (!Main.dedServ) GlowTexture = Request<Texture2D>(Texture + "_Glow");
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Main.tile[i, j].TileFrameX > 2 * 18) {
				r = 1f;
				g = 0.1f;
				b = 0.05f;
			}
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (tile.TileFrameX > 2 * 18) color = Vector3.Max(color, new Vector3(0.5f, 0.31f, 0f));
		}
	}
	public class Music_Box_SS : Music_Box, IGlowingModTile {
		public override Color MapColor => FromHexRGB(0x0A0606);
		public override int MusicSlot => Origins.Music.SmogStorm;
		public override int DustType => DustID.Asphalt;
		public override void Load() {
			base.Load();
			this.SetupGlowKeys();
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			if (!Main.dedServ) GlowTexture = Request<Texture2D>(typeof(Music_Box_AS).GetDefaultTMLName() + "_Glow");
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (Main.tile[i, j].TileFrameX > 2 * 18) {
				r = 0.05f;
				g = 0.03f;
				b = 0.00f;
			}
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => Color.White;
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			if (tile.TileFrameX > 2 * 18) color = Vector3.Max(color, new Vector3(0.5f, 0.31f, 0f) * 0.5f);
		}
	}
}