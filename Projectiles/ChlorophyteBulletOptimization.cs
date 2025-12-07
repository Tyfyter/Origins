using MonoMod.Cil;
using Origins.Core;
using Origins.Dusts;
using Origins.Items.Weapons.Ammo.Canisters;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles {
	public class ChlorophyteBulletOptimization : GlobalProjectile {
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.ChlorophyteBullet;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[ProjectileID.ChlorophyteBullet] = 2;
			ProjectileID.Sets.TrailCacheLength[ProjectileID.ChlorophyteBullet] = 15;
		}
		public override void Load() {
			try {
				IL_Projectile.AI_001 += (il) => {
					ILLabel label = default;
					new ILCursor(il).GotoNext(MoveType.AfterLabel,
						i => i.MatchLdarg0(),
						i => i.MatchLdfld<Projectile>(nameof(Projectile.type)),
						i => i.MatchLdcI4(ProjectileID.ChlorophyteBullet),
						i => i.MatchBneUn(out label),

						i => i.MatchLdarg0(),
						i => i.MatchLdfld<Projectile>(nameof(Projectile.alpha)),
						i => i.MatchLdcI4(170),
						i => i.MatchBge(out ILLabel _label) && _label.Target == label.Target
					)
					.EmitCall(((Func<bool>)ShouldSkip).Method)
					.EmitBrtrue(label);
					static bool ShouldSkip() => OriginClientConfig.Instance.ImproveChlorophyteBulletsPerformance;
				};
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(NoBackfacesToCull), e)) throw;
			}
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			if (NetmodeActive.Server) return;
			if (projectile.owner != Main.myPlayer) {
				if (!projectile.hide) {
					projectile.hide = true;
					try {
						projectile.active = true;
						projectile.timeLeft = timeLeft;
						projectile.Update(projectile.whoAmI);
					} finally {
						projectile.active = false;
						projectile.timeLeft = 0;
					}
				}
				return;
			}
			Vector2[] oldPos = [.. projectile.oldPos];
			float[] oldRot = [.. projectile.oldRot];
			for (int i = 0; i < oldPos.Length; i++) {
				if (oldPos[i] == default) {
					Array.Resize(ref oldPos, i);
					Array.Resize(ref oldRot, i);
					break;
				}
				oldPos[i] += projectile.Size * 0.5f;
				oldRot[i] += MathHelper.PiOver2;
			}
			Dust.NewDustPerfect(
				Main.LocalPlayer.Center,
				ModContent.DustType<Vertex_Trail_Dust>(),
				Vector2.Zero
			).customData = new Vertex_Trail_Dust.TrailData(oldPos, oldRot, StripColors, StripWidth, projectile.extraUpdates);
			Color StripColors(float progressOnStrip) {
				if (float.IsNaN(progressOnStrip)) return Color.Transparent;
				float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				return Color.Lerp(new Color(80, 255, 12), new Color(180, 255, 12), lerpValue) * (1f - lerpValue * lerpValue) * projectile.Opacity;
			}
			float StripWidth(float progressOnStrip) {
				float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				return MathHelper.Lerp(0, 8, 1f - lerpValue * lerpValue) * projectile.Opacity;
			}
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(Projectile projectile, ref Color lightColor) {
			if (!OriginClientConfig.Instance.ImproveChlorophyteBulletsPerformance) return base.PreDraw(projectile, ref lightColor);
			MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
			Vector2[] oldPos = [..projectile.oldPos];
			float[] oldRot = [..projectile.oldRot];
			for (int i = 0; i < oldPos.Length; i++) {
				if (oldPos[i] == default) {
					Array.Resize(ref oldPos, i);
					Array.Resize(ref oldRot, i);
					break;
				}
				oldRot[i] += MathHelper.PiOver2;
			}
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(4f);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, oldRot, StripColors, StripWidth, -Main.screenPosition + projectile.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			Color StripColors(float progressOnStrip) {
				if (float.IsNaN(progressOnStrip)) return Color.Transparent;
				float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				return Color.Lerp(new Color(80, 255, 12), new Color(180, 255, 12), lerpValue) * (1f - lerpValue * lerpValue) * projectile.Opacity;
			}
			float StripWidth(float progressOnStrip) {
				float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				return MathHelper.Lerp(0, 8, 1f - lerpValue * lerpValue) * projectile.Opacity;
			}
			return false;
		}
	}
}
