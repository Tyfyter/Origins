using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Dusts;
using Origins.Graphics;
using Origins.Items.Other.Testing;
using Origins.Items.Tools;
using Origins.Tiles.Brine;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	[LegacyName("Sulphur_Stone_Wall", "Dolomite_Wall")]
	public class Baryte_Wall : ModWall {
		Asset<Texture2D> perlin;
		float[,] odds;
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				perlin = Request<Texture2D>("Terraria/Images/Misc/Perlin");
			}
			Origins.WallHammerRequirement[Type] = 70;
			Origins.WallBlocksMinecartTracks[Type] = true;
			WallID.Sets.CannotBeReplacedByWallSpread[Type] = true;
			AddMapEntry(new Color(6, 26, 19));
			DustType = DustID.GreenMoss;
		}
		public override void RandomUpdate(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			if (j >= Main.worldSurface - 50 && (tile.LiquidAmount == 0 || (tile.LiquidAmount < 255 && tile.LiquidType == LiquidID.Water))) {
				tile.LiquidAmount = 255;
				tile.LiquidType = LiquidID.Water;
				WorldGen.SquareTileFrame(i, j);
			}
			if (!tile.HasTile && tile.LiquidAmount >= 200 && tile.LiquidType == LiquidID.Water && WorldGen.genRand.NextBool(4)) {
				int coral = TileType<Venus_Coral>();
				static bool IsSolid(Tile tile) {
					return tile.HasTile && Main.tileSolid[tile.TileType];
				}
				const int max_dist = 5;
				int k = 1;
				for (; k < max_dist; k++) {
					if (IsSolid(Framing.GetTileSafely(i + k, j))) break;
					if (IsSolid(Framing.GetTileSafely(i - k, j))) break;
					if (IsSolid(Framing.GetTileSafely(i, j + k))) break;
					if (IsSolid(Framing.GetTileSafely(i, j - k))) break;
				}
				if (k < max_dist && WorldGen.genRand.NextBool(k * k * 10)) WorldGen.PlaceTile(i, j, coral, true);
			}
		}
		public List<(Vector2 pos, float size)> hydrolanterns = [];
		public override void AnimateWall(ref byte frame, ref byte frameCounter) {
			hydrolanterns.Clear();
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.ModProjectile is Hydrolantern_Use) {
					hydrolanterns.Add((proj.position / 16, 10 * 10));
				}
			}
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (Main.dedServ || Main.gamePaused) return;
			Tile tile = Framing.GetTileSafely(i, j);
			if (tile.LiquidAmount < 200 || tile.HasFullSolidTile()) return;
			if (perlin.IsLoaded && odds is null) {
				Texture2D perlin = this.perlin.Value;
				Color[] color = new Color[perlin.Width * perlin.Height];
				perlin.GetData(color);
				odds = new float[perlin.Width, perlin.Height];
				for (int k = 0; k < color.Length; k++) {
					odds[k % perlin.Width, k / perlin.Width] = color[k].R / 255f;
				}
			}
			if (odds is null) return;
			int width = odds.GetLength(0);
			int height = odds.GetLength(1);
			float GetColor(float x, float y) {
				float flooredX = MathF.Floor(x);
				if (x == flooredX) {
					float flooredY = MathF.Floor(y);
					float factor = y - flooredY;
					return odds[(int)x % width, ((int)flooredY) % height] * (1 - factor) + odds[(int)x % width, ((int)flooredY + 1) % height] * factor;
				} else {
					float factor = x - flooredX;
					float flooredY = MathF.Floor(y);
					if (y == flooredY) {
						return odds[(int)flooredX % width, (int)y % height] * (1 - factor) + odds[((int)flooredX + 1) % width, (int)y % height] * factor;
					} else {
						return GetColor(flooredX, y) * (1 - factor) + GetColor(flooredX + 1, y) * factor;
					}
				}
			}
			const float inverse_scale = 0.8f;
			float localValue = GetColor(i * inverse_scale, j * inverse_scale);
			float lowestLanternDist = 1;
			Vector2 lanternPos = new(i + 0.5f, j + 0.5f);
			for (int k = 0; k < hydrolanterns.Count; k++) {
				(Vector2 pos, float size) = hydrolanterns[k];
				float dist = pos.DistanceSQ(lanternPos) / size;
				if (dist < lowestLanternDist) {
					lowestLanternDist = dist;
				}
			}
			float brightness = Lighting.Brightness(i, j) * 0.5f;
			brightness *= brightness;
			if (lowestLanternDist > 1 - brightness) lowestLanternDist = 1 - brightness;
			if (lowestLanternDist != 1) localValue *= lowestLanternDist;
			if (Main.rand.NextFloat(1000) < Main.gfxQuality * 100 * localValue * localValue * localValue * localValue) {
				EfficientDust.NewDustDirect(new Vector2(i - 1, j) * 16, 16, 16, Main.rand.Next(Brine_Cloud_Dust.dusts), newColor: new(65, 217, 169)).velocity *= 0.1f;
				//Gore.NewGorePerfect(Entity.GetSource_None(), new Vector2(i, j + Main.rand.NextFloat()) * 16, Vector2.UnitX * Main.rand.NextFloat(-1, 1), GoreID.LightningBunnySparks);
			}
		}
	}
	public class Brine_Cloud_Dust : ModDust {
		public override string Texture => "Terraria/Images/Gore_" + GoreID.AmbientFloorCloud1;
		public static List<int> dusts = []; 
		public override void SetStaticDefaults() {
			dusts.Add(Type);
			Deprioritized_Dust.Set[Type] = 1;
			EfficientDust.DustTexture[Type] = Request<Texture2D>(Texture);
			EfficientDust.UpdateDustCallback[Type] = DoUpdate;
		}
		public override void OnSpawn(Dust dust) {
			dust.alpha = 254;
			dust.fadeIn = 240;
			dust.frame = Texture2D.Frame();
		}
		public static void DoUpdate(Dust dust) {
			dust.fadeIn -= GoreID.Sets.DisappearSpeed[GoreID.AmbientFloorCloud1];
			if (dust.fadeIn <= 0) {
				dust.active = false;
				return;
			}
			bool flag = false;
			Tile tile = Main.tile[dust.position.ToTileCoordinates()];
			if (tile == null) {
				dust.active = false;
				return;
			}
			if (WorldGen.SolidTile(tile)) {
				flag = true;
			}
			if (dust.fadeIn <= 30) {
				flag = true;
			}
			dust.velocity *= 0.99f;
			if (!flag) {
				if (dust.alpha > 220) {
					dust.alpha--;
				}
			} else {
				dust.alpha++;
				if (dust.alpha >= 255) {
					dust.active = false;
					return;
				}
			}
			dust.position += dust.velocity;
		}
		public override bool Update(Dust dust) {
			DoUpdate(dust);
			return false;
		}
		public override bool MidUpdate(Dust dust) {
			return false;
		}
		public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor.MultiplyRGBA(dust.color) * (((255 - dust.alpha) / 255f) * 1.5f);
	}
	public class Brine_Cloud_Dust2 : Brine_Cloud_Dust {
		public override string Texture => "Terraria/Images/Gore_" + GoreID.AmbientFloorCloud2;
	}
	public class Brine_Cloud_Dust3 : Brine_Cloud_Dust {
		public override string Texture => "Terraria/Images/Gore_" + GoreID.AmbientFloorCloud3;
	}
	[LegacyName("Sulphur_Stone_Wall_Safe", "Dolomite_Wall_Safe")]
	public class Baryte_Wall_Safe : Baryte_Wall {
		public override string Texture => "Origins/Walls/Baryte_Wall";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
		public override void RandomUpdate(int i, int j) { }
		public override void AnimateWall(ref byte frame, ref byte frameCounter) { }
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) { }
	}
	[LegacyName("Sulphur_Stone_Wall_Item", "Dolomite_Wall_Item")]
	public class Baryte_Wall_Item : ModItem {
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = WallType<Baryte_Wall_Safe>();
		}
		public override void AddRecipes() {
			CreateRecipe(4)
			.AddIngredient<Baryte_Item>()
			.AddTile(TileID.WorkBenches)
			.Register();
			Recipe.Create(ModContent.ItemType<Baryte_Item>())
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	public class Replacement_Wall : ModWall {
		public override string Texture => "Terraria/Images/Wall_1";
		public override bool WallFrame(int i, int j, bool randomizeFrame, ref int style, ref int frameNumber) {
			Tile tile = Main.tile[i, j];
			tile.WallType = (ushort)tile.Get<TemporaryWallData>().value;
			return false;
		}
	}
	public class Replacement_Wall_Item : TestingItem {
		public override string Texture => "Terraria/Images/Wall_1";
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.StoneWall);
			Item.createWall = -1;
		}
		public override bool? UseItem(Player player) {
			Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
			ushort type = (ushort)WallType<Replacement_Wall>();
			if (tile.WallType != type) {
				tile.Get<TemporaryWallData>().value = tile.WallType;
				tile.WallType = type;
			}
			return true;
		}
	}
	public struct TemporaryWallData : ITileData {
		public int value;
	}
}
