using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Buffs;
using Origins.Items.Tools;
using Origins.NPCs;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using PegasusLib;
using Origins.NPCs.Defiled;
using static Terraria.GameContent.TextureAssets;
using Origins.Graphics;

namespace Origins.Items.Weapons.Magic {
	public class Nerve_Flan : ModItem, ICustomWikiStat {
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ModContent.ProjectileType<Nerve_Flan_P>(), 30, Nerve_Flan_P.tick_motion, true);
			Item.useTime /= 3;
			Item.damage = 34;
			Item.mana = 14;
			Item.knockBack = 3;
			Item.UseSound = SoundID.Item1;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Green;
		}
	}
	public class Nerve_Flan_P : ModProjectile, ITangelaHaver {
		public const int tick_motion = 8;
		public override string Texture => "Origins/Projectiles/Weapons/Seam_Beam_P";
		public override void SetStaticDefaults() {
			const int max_length = 1200;
			ProjectileID.Sets.TrailCacheLength[Type] = max_length / tick_motion;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = max_length + 16;
			Origins.HomingEffectivenessMultiplier[Type] = 3.5f;
			Mitosis_P.aiVariableResets[Type][1] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 25;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(120, 210));
			float targetWeight = 160 + Math.Max(target.width, target.height);
			targetWeight *= targetWeight;
			Vector2 targetPos = default;
			if (Main.player[Projectile.owner].DoHoming((target) => {
				if (target is not NPC npc || Projectile.localNPCImmunity[npc.whoAmI] != 0) return false;
				Vector2 currentPos = target.Center;
				float distMult = (target.wet || npc.ModNPC is IDefiledEnemy) ? 0.5f : 1f;
				float dist = MathF.Pow((Projectile.Center.X - currentPos.X) * distMult, 2) + MathF.Pow((Projectile.Center.Y - currentPos.Y) * distMult, 2);
				if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
					targetWeight = dist;
					targetPos = currentPos;
					return true;
				}
				return false;
			}, false)) {
				Projectile.velocity = (targetPos - Projectile.Center).SafeNormalize(default) * Projectile.velocity.Length();
				this.target = Projectile.Center + Projectile.velocity * 25 * (10 - Projectile.ai[2]);
				Projectile.damage = (int)(Projectile.damage * 0.9f);
			} else {
				int index = Math.Min((int)++Projectile.ai[1], Projectile.oldPos.Length);
				Projectile.oldPos[^index] = Projectile.Center + Projectile.velocity;
				Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
				StopMovement();
			}
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Inflate(2, 2);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			for (int i = 1; i < Projectile.ai[1] && i < Projectile.oldPos.Length; i++) {
				Vector2 pos = Projectile.oldPos[^i];
				if (pos == default) {
					break;
				} else if (projHitbox.Recentered(pos).Intersects(targetHitbox)) {
					return true;
				}
			}
			return null;
		}
		protected Vector2? target = null;
		protected int startupDelay = 0;
		protected float randomArcing = 0.3f;
		public override void AI() {
			target ??= Projectile.Center + Projectile.velocity * 25 * (10 - Projectile.ai[2]);
			if (Projectile.numUpdates == -1 && ++Projectile.ai[2] >= 60) {
				Projectile.Kill();
				return;
			}
			if (Projectile.ai[0] != 1) {
				if ((Projectile.numUpdates + 1) % 5 == 0 && startupDelay <= 0) {
					float speed = Projectile.velocity.Length();
					if (speed != 0) Projectile.velocity = (target.Value - Projectile.Center).SafeNormalize(Projectile.velocity / speed).RotatedByRandom(randomArcing) * speed;
				}
				if (startupDelay > 0) {
					startupDelay--;
				} else {
					if (++Projectile.ai[1] > ProjectileID.Sets.TrailCacheLength[Type]) {
						StopMovement();
					} else {
						int index = (int)Projectile.ai[1];
						Projectile.oldPos[^index] = Projectile.Center;
						Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
					}
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 direction = oldVelocity.SafeNormalize(default);
			if (direction != default) {
				float[] samples = new float[3];
				Collision.LaserScan(
					Projectile.Center,
					direction,
					5,
					32,
					samples
				);
				if (samples.Average() > tick_motion * 0.5f) {
					Projectile.Center += direction * tick_motion;
					int index = Math.Min((int)++Projectile.ai[1], Projectile.oldPos.Length);
					Projectile.oldPos[^index] = Projectile.Center;
					Projectile.oldRot[^index] = oldVelocity.ToRotation();
				}
			}
			StopMovement();
			return false;
		}
		protected void StopMovement() {
			Projectile.velocity = Vector2.Zero;
			Projectile.ai[0] = 1;
			Projectile.extraUpdates = 0;
		}
		public int? TangelaSeed { get; set; }
		public override bool PreDraw(ref Color lightColor) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}
			Origins.shaderOroboros.Capture();
			Nerve_Flan_P_Drawer.Draw(Projectile);
			Origins.shaderOroboros.DrawContents(renderTarget);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = renderTarget.Size() * 0.5f;
			TangelaVisual.DrawTangela(
				this,
				renderTarget,
				center,
				null,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None,
				Main.screenPosition * 0.5f
			);
			return false;
		}
		public override void OnKill(int timeLeft) {
			if (renderTarget is not null) {
				Main.QueueMainThreadAction(renderTarget.Dispose);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
	public struct Nerve_Flan_P_Drawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new VertexStrip();
		public static void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Framed"];
			float uTime = (float)Main.timeForVisualEffects / 44;
			int length = proj.oldPos.Length;
			if (length <= 0) return;
			float[] rot = new float[length];
			Vector2[] pos = new Vector2[length];
			for (int i = 0; i < length / 2; i++) {
				Index reverseIndex = ^(i * 2 + 1);
				if (proj.oldPos[reverseIndex] == default) {
					length = i;
					Array.Resize(ref rot, length);
					Array.Resize(ref pos, length);
					break;
				}
				rot[i] = proj.oldRot[reverseIndex];
				pos[i] = proj.oldPos[reverseIndex] + GeometryUtils.Vec2FromPolar(Main.rand.NextFloat(-6, 6), rot[i] + MathHelper.PiOver2);
				Lighting.AddLight(pos[i], 0.1f, 0.75f, 1f);
			}
			if (length == 0) return;
			//Dust.NewDustPerfect(pos[length - 1] + (new Vector2(unit.Y, -unit.X) * Main.rand.NextFloat(-4, 4)), DustID.BlueTorch, unit * 5).noGravity = true;
			Asset<Texture2D> texture = TextureAssets.Extra[194];
			miscShaderData.UseImage0(texture);
			//miscShaderData.UseShaderSpecificData(new Vector4(Main.rand.NextFloat(1), 0, 1, 1));
			miscShaderData.Shader.Parameters["uAlphaMatrix0"]?.SetValue(new Vector4(1, 1, 1, 0));
			miscShaderData.Shader.Parameters["uSourceRect0"]?.SetValue(new Vector4(Main.rand.NextFloat(1), 0, 1, 1));
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(pos, rot, _ => new Color(0.1f, 0.75f, 1f, 1), _ => 24, -Main.screenPosition, length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			for (int i = 0; i < length / 2; i++) {
				pos[i] = pos[i] + GeometryUtils.Vec2FromPolar(Main.rand.NextFloat(-6, 6), rot[i] + MathHelper.PiOver2);
			}
			_vertexStrip.PrepareStrip(pos, rot, _ => new Color(0.3f, 0.85f, 1f, 1), _ => 18, -Main.screenPosition, length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
	}
}
