using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Misc;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace Origins.Layers {
	public class Wishing_Glass_Layer : PlayerDrawLayer {
		AutoLoadingAsset<Texture2D> texture = "Origins/Items/Accessories/Wishing_Glass_Use";
		public static int StartAnimationDuration => 60;
		public static int CooldownEndAnimationDuration => 45;
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return !drawInfo.drawPlayer.dead && drawInfo.drawPlayer.OriginPlayer().wishingGlassVisible;
		}
		public override Position GetDefaultPosition() => new Multiple {
			{ new(null, PlayerDrawLayers.SolarShield), drawInfo => GetLayerPosition(drawInfo.drawPlayer.OriginPlayer().wishingGlassAnimation) == 0 },
			{ PlayerDrawLayers.BeforeFirstVanillaLayer, drawInfo => GetLayerPosition(drawInfo.drawPlayer.OriginPlayer().wishingGlassAnimation) == 1 },
			{ PlayerDrawLayers.AfterLastVanillaLayer, drawInfo => GetLayerPosition(drawInfo.drawPlayer.OriginPlayer().wishingGlassAnimation) == 2 },
		};
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			OriginPlayer originPlayer = drawInfo.drawPlayer.OriginPlayer();
			int wishingGlassAnimation = originPlayer.wishingGlassAnimation;
			if (wishingGlassAnimation == CooldownEndAnimationDuration + 1) return;
			Vector2 position = drawInfo.Position + drawInfo.drawPlayer.Size * 0.5f + originPlayer.wishingGlassOffset;
			Vector2 posScale = new(drawInfo.drawPlayer.direction * 1.5f, 1.25f);
			if (wishingGlassAnimation > CooldownEndAnimationDuration) {
				position += drawInfo.drawPlayer.Size * GetPosition(wishingGlassAnimation).XY() * posScale;
				ArmorShaderData shader = originPlayer.wishingGlassDye < 0 ? null : GameShaders.Armor.GetSecondaryShader(originPlayer.wishingGlassDye, drawInfo.drawPlayer);
				Dust dust = Dust.NewDustDirect(
					position,
					0,
					0,
					ModContent.DustType<Following_Shimmer_Dust>(),
					0,
					0,
					100,
					default,
					2f
				);
				dust.noGravity = true;
				Vector2 offset = (position - Main.rand.NextVector2FromRectangle(drawInfo.drawPlayer.Hitbox));
				dust.velocity = drawInfo.drawPlayer.velocity * 0.25f - offset.SafeNormalize(default) * 0.5f;
				dust.customData = new Following_Shimmer_Dust.FollowingDustSettings(drawInfo.drawPlayer, FollowAmount: 0.75f);
				dust.shader = shader;
				drawInfo.DustCache.Add(dust.dustIndex);
			} else {
				ArmorShaderData shader = originPlayer.wishingGlassDye < 0 ? null : GameShaders.Armor.GetSecondaryShader(originPlayer.wishingGlassDye, drawInfo.drawPlayer);
				position += drawInfo.drawPlayer.Size * new Vector2(-1, -0.5f) * posScale;
				if (wishingGlassAnimation > 0) {
					if (wishingGlassAnimation < 10 || Main.rand.NextBool(wishingGlassAnimation / 10)) {
						Vector2 velocity = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(0.85f, 1) * 2;
						Dust dust = Dust.NewDustDirect(
							position - (velocity - drawInfo.drawPlayer.velocity * 0.02f) * MathF.Log(1 / velocity.Length(), 0.97f) / 1.85f - Vector2.One * 4,
							0,
							0,
							ModContent.DustType<Following_Shimmer_Dust>(),
							0,
							0,
							100,
							default,
							1.5f
						);
						dust.noGravity = true;
						dust.velocity = drawInfo.drawPlayer.velocity * 0.05f + velocity;
						dust.customData = new Following_Shimmer_Dust.FollowingDustSettings(drawInfo.drawPlayer, FollowAmount: 0.95f);
						dust.shader = shader;
						drawInfo.DustCache.Add(dust.dustIndex);
					}
					return;
				}
			}
			drawInfo.DrawDataCache.Add(new(
				texture,
				(position - Main.screenPosition).Floor(),
				null,
				drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow),
				0,
				texture.Value.Size() * 0.5f,
				1f,
				SpriteEffects.None
			) {
				shader = originPlayer.wishingGlassDye
			});
		}
		public static Vector3 GetPosition(int wishingGlassAnimation) {
			if (wishingGlassAnimation <= CooldownEndAnimationDuration) return Vector3.Zero;
			float progress = (wishingGlassAnimation - (CooldownEndAnimationDuration + 1)) / (float)(StartAnimationDuration);
			float spinAngle = progress * MathHelper.Pi * 4;
			return new(-MathF.Cos(spinAngle), MathF.Acos(progress * 2 - 1) / MathHelper.Pi - 0.5f, MathF.Sin(spinAngle));
		}
		public static int GetLayerPosition(int wishingGlassAnimation) {
			if (wishingGlassAnimation == 0) return 0;
			return GetPosition(wishingGlassAnimation).Z <= 0 ? 1 : 2;
		}
		public static void StartAnimation(ref int wishingGlassAnimation) {
			wishingGlassAnimation = StartAnimationDuration + CooldownEndAnimationDuration + 1;
		}
		public static bool UpdateAnimation(ref int wishingGlassAnimation, int wishingGlassCooldown) {
			return wishingGlassAnimation.Cooldown(wishingGlassCooldown <= CooldownEndAnimationDuration ? 0 : (CooldownEndAnimationDuration + 1));
		}
	}
}
