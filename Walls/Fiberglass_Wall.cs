using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Backgrounds;
using Origins.Items.Other.Dyes;
using Origins.Tiles.Other;
using PegasusLib;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Numerics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Walls {
	public class Fiberglass_Wall : ModWall {
		static Asset<Texture2D> maskTexture;
		static Stack<Point> drawWalls;
		public static ArmorShaderData MaskShader { get; internal set; }
		public static bool AnyWallsVisible { get; private set; }
		static bool[] matchingWalls;
		public override void SetStaticDefaults() {
			matchingWalls ??= WallID.Sets.Factory.CreateBoolSet();
			matchingWalls[Type] = true;
			WallID.Sets.Transparent[Type] = true;
			Main.wallLight[Type] = true;
			AddMapEntry(new Color(16, 83, 122));
			RegisterItemDrop(ItemType<Fiberglass_Wall_Item>());
			HitSound = SoundID.Shatter;
			DustType = DustID.Glass;
			maskTexture ??= Request<Texture2D>(Texture + "_Mask");

			if (GetType() != typeof(Fiberglass_Wall)) return;
			On_Main.RenderWalls += On_Main_RenderWalls;
			On_WallDrawing.GetTileDrawTexture += On_WallDrawing_GetTileDrawTexture;
			drawWalls = [];
		}
		public static RenderTarget2D BackgroundMaskTarget { get; private set; }
		static bool drawingFancyLight = false;
		static Texture2D On_WallDrawing_GetTileDrawTexture(On_WallDrawing.orig_GetTileDrawTexture orig, WallDrawing self, Tile tile, int tileX, int tileY) {
			if (drawingFancyLight) return matchingWalls[tile.WallType] ? maskTexture.Value : Asset<Texture2D>.DefaultValue;
			return orig(self, tile, tileX, tileY);
		}
		static void On_Main_RenderWalls(On_Main.orig_RenderWalls orig, Main self) {
			orig(self);
			if (!Lighting.NotRetro) return;
			if (drawWalls is null) return;
			if (OriginsModIntegrations.FancyLighting is not null) {
				RenderTarget2D wallTarget = Main.instance.wallTarget;
				AnyWallsVisible = drawWalls.Count > 0;
				if (!AnyWallsVisible) return;
				drawWalls.Clear();
				if (BackgroundMaskTarget is not null && (BackgroundMaskTarget.Width != Main.screenWidth || BackgroundMaskTarget.Height != Main.screenHeight)) {
					BackgroundMaskTarget.Dispose();
					BackgroundMaskTarget = null;
				}
				BackgroundMaskTarget ??= new(Main.instance.GraphicsDevice, Main.screenWidth + Main.offScreenRange * 2, Main.screenHeight + Main.offScreenRange * 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

				try {
					drawingFancyLight = true;
					Main.instance.wallTarget = BackgroundMaskTarget;
					Main.screenPosition += Vector2.One;
					orig(self);
				} finally {
					Main.screenPosition -= Vector2.One;
					drawingFancyLight = false;
					Main.instance.wallTarget = wallTarget;
				}
				return;
			}
			AnyWallsVisible = drawWalls.Count > 0;
			if (BackgroundMaskTarget is not null && (BackgroundMaskTarget.Width != Main.screenWidth || BackgroundMaskTarget.Height != Main.screenHeight)) {
				BackgroundMaskTarget.Dispose();
				BackgroundMaskTarget = null;
			}
			BackgroundMaskTarget ??= new(Main.instance.GraphicsDevice, Main.screenWidth + Main.offScreenRange * 2, Main.screenHeight + Main.offScreenRange * 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

			RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
			Main.graphics.GraphicsDevice.SetRenderTarget(BackgroundMaskTarget);
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);
			Main.tileBatch.Begin(Main.GameViewMatrix.ZoomMatrix);

			VertexColors vertices;
			VertexColors _glowPaintColors = new(Color.White);
			Rectangle frame = new(0, 0, 32, 32);
			Vector2 offScreenOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			while (drawWalls.TryPop(out Point pos)) {
				(int i, int j) = pos;
				Color color = Lighting.GetColor(i, j);
				Tile tile = Framing.GetTileSafely(pos);
				if (!tile.IsWallFullbright && color.R == 0 && color.G == 0 && color.B == 0 && i < Main.UnderworldLayer) {
					continue;
				}
				frame.X = tile.WallFrameX + 1;
				frame.Y = tile.WallFrameY + 1;
				if (Lighting.NotRetro && !WorldGen.SolidTile(tile)) {
					if (tile.IsWallFullbright) {
						vertices = _glowPaintColors;
					} else {
						Lighting.GetCornerColors(i, j, out vertices);
					}
					Main.tileBatch.Draw(maskTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - 8, j * 16 - (int)Main.screenPosition.Y - 8) + offScreenOffset, frame, vertices, Vector2.Zero, 1f, SpriteEffects.None);
				} else {
					Main.spriteBatch.Draw(maskTexture.Value, new Vector2(i * 16 - (int)Main.screenPosition.X - 8, j * 16 - (int)Main.screenPosition.Y - 8) + offScreenOffset, frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
				}
			}
			Main.tileBatch.End();
			Main.spriteBatch.End();
			Main.graphics.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!Lighting.NotRetro) return true;
			if (!drawingFancyLight && (OriginsModIntegrations.FancyLighting is null || drawWalls.Count <= 0)) drawWalls.Push(new(i, j));
			return true;
		}
	}
	public class Fiberglass_Wall_Safe : Fiberglass_Wall {
		public override string Texture => "Origins/Walls/Fiberglass_Wall";
		public override void Load() { }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.wallHouse[Type] = true;
		}
	}
	public class Fiberglass_Wall_Item : ModItem {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Fiberglass_Wall.MaskShader = GameShaders.Armor.BindShader(Type, new ArmorShaderData(Mod.Assets.Request<Effect>("Effects/Misc"), "MultiplyRGBA"));
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GlassWall);
			Item.createWall = WallType<Fiberglass_Wall_Safe>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemType<Fiberglass_Item>())
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemType<Fiberglass_Item>(), 1)
			.AddIngredient(Type, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	public class Enchanted_Fiberglass_Wall_Item : ModItem {
		public override string Texture => typeof(Fiberglass_Wall_Item).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ItemID.Sets.DrawUnsafeIndicator[Type] = true;
			ItemID.Sets.ShimmerTransformToItem[ItemType<Fiberglass_Wall_Item>()] = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GlassWall);
			Item.createWall = WallType<Fiberglass_Wall>();
		}
	}
}
