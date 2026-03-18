using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Wind_Turbine : ModTile, IGlowingModTile {
		public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Wind_Turbine).GetDefaultTMLName() + "_Glow";
		public Color GlowColor => Color.White;
		AutoCastingAsset<Texture2D> IGlowingModTile.GlowTexture => GlowTexture;
		public override void Load() {
			new TileItem(this)
			.WithExtraDefaults(item => {
				item.CloneDefaults(ItemID.Sawmill);
				item.createTile = Type;
				//item.rare++;
				item.value += Item.buyPrice(gold: 1);
			}).WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.Cog, 5)
				.AddRecipeGroup(ALRecipeGroups.SilverBars)
				.AddIngredient<Scrap>(18)
				.AddTile<Metal_Presser>()
				.Register();
			}).RegisterItem();
			this.SetupGlowKeys();
		}
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			switch (tile.TileFrameY) {
				case 0:
				if (tile.TileFrameX == 18) break;
				return;
				case 18 * 8:
				case 18 * 9:
				break;
				default:
				return;
			}
			color.DoFancyGlow(Color.OrangeRed.ToVector3(), tile.TileColor);
		}
		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			drawData.glowTexture = GlowTexture;
			drawData.glowSourceRect = new Rectangle(drawData.tileFrameX, drawData.tileFrameY + drawData.addFrY, 16, 16);
			drawData.glowColor = GlowColor;
		}
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.SetHeight(10);
			TileObjectData.newTile.SetOriginBottomCenter();
			this.SetAnimationHeight();
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(40, 30, 18), this.GetTileItem().DisplayName);
			DustType = Ashen_Biome.DefaultTileDust;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
			frameYOffset = Wind_Turbine_TE.GetAnimation(new(left, top)).frame * AnimationFrameHeight;
		}
		public override void PlaceInWorld(int i, int j, Item item) {
			TileUtils.GetMultiTileTopLeft(i, j, TileObjectData.GetTileData(Main.tile[i, j]), out int left, out int top);
			ModContent.GetInstance<Wind_Turbine_TE>().AddTileEntity(new(left, top));
		}
		public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects spriteEffects) {
			position.Y += 2;
			return base.PreDrawPlacementPreview(i, j, spriteBatch, ref frame, ref position, ref color, validPlacement, ref spriteEffects);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = 2;
		}
		public class Wind_Turbine_TE : TESystem {
			Dictionary<Point16, Turbine_Animation> animations;
			public override void PreUpdateEntities() {
				animations ??= [];
				int turbine = ModContent.TileType<Wind_Turbine>();
				for (int i = 0; i < tileEntityLocations.Count; i++) {
					Point16 pos = tileEntityLocations[i];
					Tile tile = Main.tile[pos];
					if (tile.HasTile && tile.TileType == turbine) {
						GetAnimation(pos).Update(pos);
					} else {
						tileEntityLocations.RemoveAt(i--);
					}
				}
				foreach (Point16 item in animations.Keys.Except(tileEntityLocations).ToArray()) {
					animations.Remove(item);
				}
			}
			public static Turbine_Animation GetAnimation(Point16 position) {
				Dictionary<Point16, Turbine_Animation> openDoors = ModContent.GetInstance<Wind_Turbine_TE>().animations;
				openDoors.TryAdd(position, new());
				return openDoors[position];
			}
			public override void LoadWorldData(TagCompound tag) {
				base.LoadWorldData(tag);
				animations ??= [];
				int turbine = ModContent.TileType<Wind_Turbine>();
				for (int i = 0; i < tileEntityLocations.Count; i++) {
					Point16 pos = tileEntityLocations[i];
					Tile tile = Main.tile[pos];
					if (tile.HasTile && tile.TileType == turbine) {
						GetAnimation(pos).Update(pos, true);
					} else {
						tileEntityLocations.RemoveAt(i--);
					}
				}
			}
			public class Turbine_Animation {
				public int frame = 0;
				public float frameCounter = 0;
				public float speed = 0;
				public bool ShouldSupplyPower => Math.Abs(speed) > 0.15f;
				const float wind_speed_range = 0.8f;
				public static float WindSpeed => float.Clamp(Main.windSpeedCurrent / wind_speed_range, -1, 1);
				public void Update(Point16 position, bool instant = false) {
					TileObjectData data = TileObjectData.GetTileData(Main.tile[position]);
					TileUtils.GetMultiTileTopLeft(position.X, position.Y, data, out int left, out int top);
					int openWalls = 0;
					for (int j = 0; j < 5; j++) {
						for (int i = 0; i < data.Width; i++) {
							if (top + j > Main.worldSurface) continue;
							if (Main.tile[left + i, top + j].WallType == WallID.None) openWalls++;
						}
					}
					float targetSpeed = openWalls * WindSpeed / (data.Width * 5);
					if (instant) {
						speed = targetSpeed;
					} else {
						MathUtils.LinearSmoothing(ref speed, targetSpeed, Math.Abs(targetSpeed) > Math.Abs(speed) ? Math.Abs(targetSpeed) * 0.01f : 0.01f);
					}
					for (int j = 0; j < data.Height; j++) {
						for (int i = 0; i < data.Width; i++) {
							Ashen_Wire_Data.SetTilePowered(left + i, top + j, ShouldSupplyPower);
						}
					}
					const int min_frames = 4;
					const int frame_num = 5;
					switch (Math.Sign(speed)) {
						case -1:
						if (frameCounter.CycleDown(min_frames, Math.Abs(speed))) frame.CycleDown(frame_num - 1);
						break;
						case 1:
						if (frameCounter.CycleUp(min_frames, speed)) frame.CycleUp(frame_num);
						break;
					}
				}
			}
		}
		public CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}
