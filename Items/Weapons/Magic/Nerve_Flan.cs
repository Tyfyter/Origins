using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Items.Tools;
using Origins.Tiles.Defiled;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Nerve_Flan : ModItem, ICustomDrawItem {
		AutoLoadingAsset<Texture2D> useTexture = typeof(Nerve_Flan).GetDefaultTMLName() + "_Use";
		AutoLoadingAsset<Texture2D> useTangelaTexture = typeof(Nerve_Flan).GetDefaultTMLName() + "_Use_Tangela";
		public override void SetStaticDefaults() {
			useTexture.LoadAsset();
			useTangelaTexture.LoadAsset();
		}
		public override void SetDefaults() {
			Item.DefaultToMagicWeapon(ModContent.ProjectileType<Nerve_Flan_P>(), 50, Nerve_Flan_P.tick_motion, true);
			Item.useTime /= 10;
			Item.damage = 14;
			Item.mana = 16;
			Item.knockBack = 3;
			Item.UseSound = null;
			Item.crit -= 2;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Blue;
			Item.ArmorPenetration = 5;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Strange_String>(), 100)
			.AddIngredient(ModContent.ItemType<Tangela_Bramble_Item>())
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void UseItemFrame(Player player) {
			float offset;
			float timePerSwing = player.itemAnimationMax * 0.5f;
			if (player.itemAnimation > timePerSwing) {
				offset = (player.itemAnimation - timePerSwing) * -2 / timePerSwing + 1;
			} else {
				offset = (timePerSwing - player.itemAnimation) * -2 / timePerSwing + 1;
			}
			offset = Math.Clamp(offset * 2, -1, 1);
			player.SetCompositeArmFront(
				true,
				Player.CompositeArmStretchAmount.Full,
				player.itemRotation * player.gravDir - (MathHelper.PiOver2 + offset) * player.direction
			);
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			float offset;
			Player player = drawInfo.drawPlayer;
			float timePerSwing = player.itemAnimationMax * 0.5f;
			if (player.itemAnimation > timePerSwing) {
				offset = (player.itemAnimation - timePerSwing) * -2 / timePerSwing + 1;
			} else {
				offset = (timePerSwing - player.itemAnimation) * -2 / timePerSwing + 1;
			}
			const float open_time_factor = 10f;
			Rectangle frame = useTexture.Value.Frame(verticalFrames: 6, frameY: Math.Clamp((int)(open_time_factor - Math.Abs(offset * open_time_factor)), 0, 5));
			DrawData data = new(
				useTexture,
				player.GetCompositeArmPosition(false) - Main.screenPosition,
				frame,
				lightColor,
				player.compositeFrontArm.rotation + MathHelper.Pi - MathHelper.PiOver4 * player.direction * player.gravDir,
				new Vector2(11, 22).Apply(drawInfo.itemEffect, frame.Size()),
				1f,
				drawInfo.itemEffect
			);
			drawInfo.DrawDataCache.Add(data);
			data.texture = useTangelaTexture;
			data.shader = TangelaVisual.FakeShaderID;
			drawInfo.DrawDataCache.Add(data);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.ItemUsesThisAnimation is 1 or 5 or 9) return false;

			Projectile.NewProjectile(
				source,
				player.GetCompositeArmPosition(false),
				(velocity.RotatedByRandom(0.05f) + ((player.compositeFrontArm.rotation + MathHelper.PiOver2) * player.gravDir).ToRotationVector2() * 3).SafeNormalize(default) * velocity.Length(),
				type,
				damage,
				knockback
			);
			return false;
		}
	}
	public class Nerve_Flan_P : ModProjectile, ITangelaHaver {
		public const int tick_motion = 8;
		public override string Texture => "Origins/Projectiles/Weapons/Seam_Beam_P";
		public override void SetStaticDefaults() {
			const int max_length = 1200;
			ProjectileID.Sets.TrailCacheLength[Type] = max_length / tick_motion;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = max_length + 16;
			Origins.HomingEffectivenessMultiplier[Type] = 25f;
			OriginsSets.Projectiles.DuplicationAIVariableResets[Type].second = true;
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
		protected int startupDelay = 2;
		protected float randomArcing = 0.3f;
		public override void AI() {
			target ??= Projectile.Center + Projectile.velocity * 25 * (10 - Projectile.ai[2]);
			if (Projectile.numUpdates == -1 && ++Projectile.ai[2] >= 20) {
				Projectile.Kill();
				return;
			}
			if (Projectile.ai[0] != 1) {
				if ((Projectile.numUpdates + 1) % 5 == 0 && startupDelay <= 0) {
					float speed = Projectile.velocity.Length();
					if (speed != 0) Projectile.velocity = (target.Value - Projectile.Center).SafeNormalize(Projectile.velocity / speed).RotatedByRandom(randomArcing) * speed;
				}
				if (startupDelay > 0) {
					SoundEngine.PlaySound(Origins.Sounds.defiledKillAF.WithPitchRange(-1f, -0.2f).WithVolume(0.1f), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item60.WithPitchRange(-1f, -0.2f), Projectile.Center);
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
			Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
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
				Main.screenPosition
			);
			return false;
		}
		public override void OnKill(int timeLeft) {
			if (renderTarget is not null) {
				TangelaVisual.SendRenderTargetForDisposal(ref renderTarget);
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

		private static VertexStrip _vertexStrip = new();
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
				Lighting.AddLight(pos[i], 0.25f, 0f, 0.28f);
			}
			if (length == 0) return;
			//Dust.NewDustPerfect(pos[length - 1] + (new Vector2(unit.Y, -unit.X) * Main.rand.NextFloat(-4, 4)), DustID.BlueTorch, unit * 5).noGravity = true;
			Asset<Texture2D> texture = TextureAssets.Extra[ExtrasID.MagicMissileTrailShape];
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
