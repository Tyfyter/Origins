using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Graphics {
	[ReinitializeDuringResizeArrays]
	public class EfficientDust : ILoadable {
		public static Action<Dust>[] UpdateDustCallback = DustID.Sets.Factory.CreateCustomSet<Action<Dust>>(null);
		public static Asset<Texture2D>[] DustTexture = DustID.Sets.Factory.CreateCustomSet<Asset<Texture2D>>(null);
		static readonly Dust[] dust = new Dust[Main.maxDust].Select(_ => new Dust()).ToArray();
		public void Load(Mod mod) {
			IL_Main.DrawDust += (il) => new ILCursor(il).EmitCall(((Action)DrawDust).Method);
			IL_Dust.UpdateDust += (il) => new ILCursor(il).EmitCall(((Action)UpdateDust).Method);
		}
		static void DrawDust() {
			Rectangle rectangle = new((int)Main.screenPosition.X - 500 - 4, (int)Main.screenPosition.Y - 50 - 4, Main.screenWidth + 1000, Main.screenHeight + 100);
			Color defaultColor = default;
			Color black = Color.Black;
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
			for (int i = 0; i < dust.Length; i++) {
				Dust dust = EfficientDust.dust[i];
				if (!dust.active || !rectangle.Contains(dust.position)) continue;
				Color lightColor = Lighting.GetColor((int)(dust.position.X + 4f) / 16, (int)(dust.position.Y + 4f) / 16);
				bool isBlack = lightColor == black;
				lightColor = dust.GetAlpha(lightColor);
				if (isBlack && lightColor == black) continue;
				Texture2D texture = DustTexture[dust.type].Value;
				Main.spriteBatch.Draw(texture, dust.position - Main.screenPosition, dust.frame, lightColor, dust.rotation, new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
				if (dust.color != defaultColor) {
					Main.spriteBatch.Draw(texture, dust.position - Main.screenPosition, dust.frame, dust.GetColor(lightColor), dust.rotation, new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
				}
			}
			Main.spriteBatch.End();
		}
		static void UpdateDust() {
			for (int i = 0; i < dust.Length; i++) {
				Dust dust = EfficientDust.dust[i];
				if (dust.active) UpdateDustCallback[dust.type](dust);
			}
		}
		static readonly Dust fakeDust = new();
		public static Dust NewDustDirect(Vector2 Position, int Width, int Height, int Type, float SpeedX = 0f, float SpeedY = 0f, int Alpha = 0, Color newColor = default, float Scale = 1f) {
			if (Main.gameMenu || Main.gamePaused || WorldGen.gen || NetmodeActive.Server) return fakeDust;
			Rectangle rectangle = new((int)Main.screenPosition.X - 500 - 4, (int)Main.screenPosition.Y - 50 - 4, Main.screenWidth + 1000, Main.screenHeight + 100);
			bool perfect = Width == 0 && Height == 0;
			if (!perfect) Position += new Vector2(4f);
			if (!rectangle.Intersects(new((int)Position.X, (int)Position.Y, Width, Height))) return fakeDust;
			if (Width < 5) Width = 5;
			if (Height < 5) Height = 5;
			for (int i = 0; i < dust.Length; i++) {
				Dust dust = EfficientDust.dust[i];
				if (!dust.active) {
					dust.fadeIn = 0f;
					dust.active = true;
					dust.type = Type;
					dust.noGravity = false;
					dust.color = newColor;
					dust.alpha = Alpha;
					dust.position.X = Position.X + Main.rand.Next(Width - 4);
					dust.position.Y = Position.Y + Main.rand.Next(Height - 4);
					if (perfect) {
						dust.velocity.X = 0;
						dust.velocity.Y = 0;
					} else {
						dust.velocity.X = Main.rand.Next(-20, 21) * 0.1f + SpeedX;
						dust.velocity.Y = Main.rand.Next(-20, 21) * 0.1f + SpeedY;
					}
					dust.shader = null;
					dust.customData = null;
					dust.noLightEmittence = false;
					dust.frame.Width = 8;
					dust.frame.Height = 8;
					dust.rotation = 0f;
					dust.scale = 1f + Main.rand.Next(-20, 21) * 0.01f;
					dust.scale *= Scale;
					dust.noLight = false;
					dust.firstFrame = true;

					if (DustLoader.GetDust(dust.type) is ModDust modDust) {
						dust.frame.X = 0;
						dust.frame.Y %= 30;
						modDust.OnSpawn(dust);
					}
					return dust;
				}
			}
			return fakeDust;
		}

		public void Unload() { }
	}
}
