using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Dusts;
using ReLogic.Content;
using ReLogic.Threading;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Graphics {
	[ReinitializeDuringResizeArrays]
	public class EfficientDust : ILoadable {
		public static Action<Dust>[] UpdateDustCallback = DustID.Sets.Factory.CreateCustomSet<Action<Dust>>(null);
		public static Asset<Texture2D>[] DustTexture = DustID.Sets.Factory.CreateCustomSet<Asset<Texture2D>>(null);
		public static DrawDustFunc[] DustDrawFuncs = DustID.Sets.Factory.CreateCustomSet<DrawDustFunc>(null);
		public static Action<Dust>[] SpawnDustCallback = DustID.Sets.Factory.CreateCustomSet<Action<Dust>>(null);
		static Dust[] dust = new Dust[Main.maxDust].Select(_ => new Dust()).ToArray();
		public void Load(Mod mod) {
			if (NetmodeActive.Server) {
				dust = null;
				return;
			}
			IL_Main.DrawDust += (il) => new ILCursor(il).EmitCall(((Action)DrawDust).Method);
			IL_Dust.UpdateDust += (il) => new ILCursor(il).EmitCall(((Action)UpdateDust).Method);
		}
		internal static void SetupDefaults() {
			for (int i = 0; i < UpdateDustCallback.Length; i++) {
				ModDust modDust = DustLoader.GetDust(i);
				if (modDust is null) {
					DustDrawFuncs[i] = (dust) => {
						Color lightColor = Lighting.GetColor((int)(dust.position.X + 4f) / 16, (int)(dust.position.Y + 4f) / 16);
						lightColor = dust.GetAlpha(lightColor);
						if (lightColor == Color.Black) return;
						Main.spriteBatch.Draw(TextureAssets.Dust.Value, dust.position - Main.screenPosition, dust.frame, lightColor, dust.GetVisualRotation(), new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
						if (dust.color.PackedValue != 0) {
							lightColor = dust.GetColor(lightColor);
							if (lightColor.PackedValue != 0) {
								Main.spriteBatch.Draw(TextureAssets.Dust.Value, dust.position - Main.screenPosition, dust.frame, lightColor, dust.GetVisualRotation(), new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
							}
						}
					};
				} else {
					UpdateDustCallback[i] ??= dust => modDust.Update(dust);
					DustDrawFuncs[i] ??= (dust) => {
						if (!modDust.PreDraw(dust)) return;
						Color lightColor = Lighting.GetColor((int)(dust.position.X + 4f) / 16, (int)(dust.position.Y + 4f) / 16);
						bool isBlack = lightColor == Color.Black;
						lightColor = dust.GetAlpha(lightColor);
						if (isBlack && lightColor == Color.Black) return;
						Main.spriteBatch.Draw(modDust.Texture2D.Value, dust.position - Main.screenPosition, dust.frame, lightColor, dust.rotation, new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
						if (dust.color != default) {
							Main.spriteBatch.Draw(modDust.Texture2D.Value, dust.position - Main.screenPosition, dust.frame, dust.GetColor(lightColor), dust.rotation, new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
						}
					};
				}
			}
			UpdateDustCallback[DustID.Smoke] = Smoke;
			UpdateDustCallback[DustID.Water] = Water;
			UpdateDustCallback[DustID.Water_Corruption] = Water;
			UpdateDustCallback[DustID.Water_Jungle] = Water;
			UpdateDustCallback[DustID.Water_Hallowed] = Water;
			UpdateDustCallback[DustID.Water_Snow] = Water;
			UpdateDustCallback[DustID.Water_Desert] = Water;
			UpdateDustCallback[DustID.Water_Space] = Water;
			UpdateDustCallback[DustID.Water_Cavern] = Water;
			UpdateDustCallback[DustID.Water_BloodMoon] = Water;
			UpdateDustCallback[DustID.Water_Crimson] = Water;
			UpdateDustCallback[DustID.DesertWater2] = Water;
			UpdateDustCallback[DustID.Lava] = Lava;
			UpdateDustCallback[DustID.Honey] = Honey;

			SpawnDustCallback[DustID.Water] = SpawnWater;
			SpawnDustCallback[DustID.UnholyWater] = SpawnWater;
			SpawnDustCallback[DustID.SomethingRed] = SpawnWater;
			SpawnDustCallback[DustID.Water_Corruption] = SpawnWater;
			SpawnDustCallback[DustID.Water_Jungle] = SpawnWater;
			SpawnDustCallback[DustID.Water_Hallowed] = SpawnWater;
			SpawnDustCallback[DustID.Water_Snow] = SpawnWater;
			SpawnDustCallback[DustID.Water_Desert] = SpawnWater;
			SpawnDustCallback[DustID.Water_Space] = SpawnWater;
			SpawnDustCallback[DustID.Water_Cavern] = SpawnWater;
			SpawnDustCallback[DustID.Water_BloodMoon] = SpawnWater;
		}
		static void Smoke(Dust dust) {
			dust.position += dust.velocity;
			dust.velocity.Y *= 0.98f;
			dust.velocity.X *= 0.98f;
			switch (dust.customData) {
				case float f:
				dust.velocity.Y += f;
				break;

				case NPC npc: {
					dust.position += npc.position - npc.oldPosition;
					if (dust.noGravity) {
						dust.velocity *= 1.02f;
					}
					dust.alpha -= 70;
					if (dust.alpha < 0) {
						dust.alpha = 0;
					}
					dust.scale *= 0.97f;
					if (dust.scale <= 0.01f) {
						dust.scale = 0.0001f;
						dust.alpha = 255;
					}
				}
				break;

				default:
				if (dust.noGravity) {
					dust.velocity *= 1.02f;
					dust.scale += 0.02f;
					dust.alpha += 4;
					if (dust.alpha > 255) {
						dust.scale = 0.0001f;
						dust.alpha = 255;
					}
				}
				break;
			}
			dust.velocity.X *= 0.99f;
			dust.rotation += dust.velocity.X * 0.5f;
			if (dust.fadeIn > 0f && dust.fadeIn < 100f) {
				dust.scale += 0.03f;
				if (dust.scale > dust.fadeIn) dust.fadeIn = 0f;
			} else {
				dust.scale -= 0.01f;
			}
			if (dust.noGravity) {
				dust.velocity *= 0.92f;
				if (dust.fadeIn == 0f) dust.scale -= 0.04f;
			}
		}
		static void Water(Dust dust) {
			dust.position += dust.velocity;
			dust.velocity.Y += 0.1f;
			if (dust.velocity.X == 0f) {
				if (Collision.SolidCollision(dust.position, 2, 2)) dust.scale = 0f;
				dust.rotation += 0.5f;
				dust.scale -= 0.01f;
			}
			if (Collision.WetCollision(new Vector2(dust.position.X, dust.position.Y), 4, 4)) {
				dust.alpha += 20;
				dust.scale -= 0.1f;
			}
			dust.alpha += 2;
			dust.scale -= 0.005f;
			if (dust.alpha > 255) dust.scale = 0f;
			if (dust.velocity.Y > 4f) dust.velocity.Y = 4f;
			if (dust.noGravity) {
				dust.rotation += float.CopySign(0.2f, dust.velocity.X);
				dust.scale += 0.03f;
				dust.velocity.X *= 1.05f;
				dust.velocity.Y += 0.15f;
			}
			dust.velocity.X *= 0.99f;
			if (dust.fadeIn > 0f && dust.fadeIn < 100f) {
				dust.scale += 0.03f;
				if (dust.scale > dust.fadeIn) dust.fadeIn = 0f;
			} else {
				dust.scale -= 0.01f;
			}
			if (dust.noGravity) {
				dust.velocity *= 0.92f;
				if (dust.fadeIn == 0f) dust.scale -= 0.04f;
			}
		}
		static void SpawnWater(Dust dust) {
			dust.alpha = 170;
			dust.velocity *= 0.5f;
			dust.velocity.Y += 1f;
		}
		static void Lava(Dust dust) {
			Dust.lavaBubbles++;
			dust.position += dust.velocity;
			dust.velocity.Y += 0.1f;
			if (!Collision.WetCollision(new Vector2(dust.position.X, dust.position.Y - 8f), 4, 4)) {
				dust.scale = 0f;
			} else {
				dust.alpha += Main.rand.Next(2);
				if (dust.alpha > 255) dust.scale = 0f;
				dust.velocity.Y = -0.5f;
				dust.alpha++;
				dust.scale -= 0.01f;
				dust.velocity.Y = -0.2f;
				dust.velocity.X += Main.rand.Next(-10, 10) * 0.002f;
				Clamp(ref dust.velocity.X, -0.25f, 0.25f);
			}
			if (dust.noGravity) {
				dust.scale += 0.03f;
				if (dust.scale < 1f) dust.velocity.Y += 0.075f;
				dust.velocity.X *= 1.08f;
				dust.rotation += float.CopySign(0.01f, dust.velocity.X);
				float lightScale = Math.Min(dust.scale * 0.6f, 1);
				Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f + 1f), lightScale, lightScale * 0.3f, lightScale * 0.1f);
			}
			dust.velocity.X *= 0.99f;
			dust.rotation += dust.velocity.X * 0.5f;
			dust.scale -= 0.01f;
		}
		static void Honey(Dust dust) {
			dust.position += dust.velocity;
			dust.velocity.Y += 0.1f;
			if (!Collision.WetCollision(new Vector2(dust.position.X, dust.position.Y - 8f), 4, 4)) {
				dust.scale = 0f;
			} else {
				dust.alpha += Main.rand.Next(2);
				if (dust.alpha > 255) dust.scale = 0f;
				dust.velocity.Y = -0.5f;
				dust.alpha++;
				dust.scale -= 0.01f;
				dust.velocity.Y = -0.2f;
				dust.velocity.X += Main.rand.Next(-10, 10) * 0.002f;
				Clamp(ref dust.velocity.X, -0.25f, 0.25f);
			}
			dust.velocity.X *= 0.99f;
			dust.velocity *= 0.92f;
			dust.scale -= 0.05f;
		}
		static void DrawDust() {
			if (NetmodeActive.Server) return;
			Rectangle rectangle = new((int)Main.screenPosition.X - 500 - 4, (int)Main.screenPosition.Y - 50 - 4, Main.screenWidth + 1000, Main.screenHeight + 100);
			Color defaultColor = default;
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
			for (int i = 0; i < dust.Length; i++) {
				Dust dust = EfficientDust.dust[i];
				if (!dust.active || !rectangle.Contains(dust.position)) continue;
				if (DustDrawFuncs[dust.type] is DrawDustFunc func) {
					func(dust);
					continue;
				}
				Color lightColor = Lighting.GetColor((int)(dust.position.X + 4f) / 16, (int)(dust.position.Y + 4f) / 16);
				bool isBlack = lightColor == Color.Black;
				lightColor = dust.GetAlpha(lightColor);
				if (isBlack && lightColor == Color.Black) continue;
				Texture2D texture = DustTexture[dust.type].Value;
				Main.spriteBatch.Draw(texture, dust.position - Main.screenPosition, dust.frame, lightColor, dust.rotation, new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
				if (dust.color != defaultColor) {
					Main.spriteBatch.Draw(texture, dust.position - Main.screenPosition, dust.frame, dust.GetColor(lightColor), dust.rotation, new Vector2(4f, 4f), dust.scale, SpriteEffects.None, 0f);
				}
			}
			Main.spriteBatch.End();
		}
		static void UpdateDust() {
			if (NetmodeActive.Server) return;
			reachedFakeDust = false;
			FastParallel.For(0, dust.Length, (fromInclusive, toExclusive, _) => {
				for (int i = fromInclusive; i < toExclusive; i++) {
					Dust dust = EfficientDust.dust[i];
					if (dust.active) {
						try {
							UpdateDustCallback[dust.type](dust);
							dust.active &= dust.scale > 0 && dust.alpha < 256;
						} catch (Exception) {
							dust.active = false;
						}
					}
				}
			});
		}
		static readonly Dust fakeDust = new();
		static bool reachedFakeDust = false;
		public static Dust NewDustDirect(Vector2 Position, int Width, int Height, int Type, float SpeedX = 0f, float SpeedY = 0f, int Alpha = 0, Color newColor = default, float Scale = 1f) {
			if (UpdateDustCallback[Type] is null) return Dust.NewDustDirect(Position, Width, Height, Type, SpeedX, SpeedY, Alpha, newColor, Scale);
			if (Main.gameMenu || Main.gamePaused || WorldGen.gen || NetmodeActive.Server || reachedFakeDust) return fakeDust;
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
						dust.frame.Y = 10 * Main.rand.Next(3);
						modDust.OnSpawn(dust);
					} else {
						dust.frame.X = 10 * Type;
						dust.frame.Y = 10 * Main.rand.Next(3);
						int looper = Type;
						while (looper >= 100) {
							looper -= 100;
							dust.frame.X -= 1000;
							dust.frame.Y += 30;
						}
					}

					SpawnDustCallback[Type]?.Invoke(dust); 
					return dust;
				}
			}
			reachedFakeDust = true;
			return fakeDust;
		}
		public void Unload() { }
	}
	public delegate void DrawDustFunc(Dust dust);
}
