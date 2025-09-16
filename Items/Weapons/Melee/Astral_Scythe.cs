using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Armor.Aetherite;
using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;

namespace Origins.Items.Weapons.Melee {
	[LegacyName("Splitting_Image")]
	public class Astral_Scythe : ModItem {
		public override void SetStaticDefaults() {
			OriginsSets.Items.SwungNoMeleeMelees[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.damage = 64;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.crit = -2;
			Item.noMelee = true;
			Item.width = 100;
			Item.height = 98;
			Item.useTime = 42;
			Item.useAnimation = 42;
			Item.shoot = ModContent.ProjectileType<Astral_Scythe_Slash>();
			Item.shootSpeed = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 8;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item71.WithPitch(-1f);
			Item.autoReuse = false;
		}
		public override bool CanUseItem(Player player) {
			if (OriginsModIntegrations.CheckAprilFools()) return true;
			return !player.HasBuff<Astral_Scythe_Wait_Debuff>();
		}
		public override bool MeleePrefix() => true;
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Aetherite_Bar>(20)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool AltFunctionUse(Player player) {
			return !player.HasBuff<Astral_Scythe_Wait_Debuff>();
		}
		// alt fire sound here because it runs on all sides
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				SoundEngine.PlaySound(SoundID.Shatter, player.MountedCenter);
			}
			return base.UseItem(player);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				int frameReduction = player.itemAnimationMax / 2;
				player.itemTime -= frameReduction;
				player.itemTimeMax -= frameReduction;
				player.itemAnimation -= frameReduction;
				player.itemAnimationMax -= frameReduction;

				damage = (int)(damage * 0.3f);
				player.OriginPlayer().scytheHitCombo = 0;
				player.AddBuff(Astral_Scythe_Wait_Debuff.ID, 3 * 60);
				type = ModContent.ProjectileType<Astral_Scythe_Star>();
			} else {
				const float sqrt_2 = 1.4142135623731f;
				velocity = new Vector2(sqrt_2 * player.direction, -sqrt_2) * velocity.Length();
				if (OriginsModIntegrations.CheckAprilFools() && player.HasBuff<Astral_Scythe_Wait_Debuff>()) damage = (int)(damage * 0.5f);
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				for (int i = 0; i < 8; i++) {
					Projectile.NewProjectile(
						source,
						position,
						velocity * 12 + Main.rand.NextVector2Circular(1, 1) * 8,
						type,
						damage,
						knockback
					);
				}
				return false;
			}
			int ai0 = 0;
			if (OriginsModIntegrations.CheckAprilFools() && player.HasBuff<Astral_Scythe_Wait_Debuff>()) ai0 = 2;
			else if (player.OriginPlayer().scytheHitCombo >= OriginPlayer.maxScytheHitCombo) ai0 = 1;

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai0: ai0);
			return false;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D texture = TextureAssets.Item[Type].Value;
			Player player = Main.LocalPlayer;
			bool hasDebuff = player.HasBuff<Astral_Scythe_Wait_Debuff>();

			int variant = 0;
			if (hasDebuff) variant = 1;
			else if (player.OriginPlayer().scytheHitCombo >= OriginPlayer.maxScytheHitCombo) variant = 2;

			frame = texture.Frame(verticalFrames: 3, frameY: variant);
			spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, drawColor, 0, origin, scale, SpriteEffects.None, 0);
			return false;
		}
	}
	public class Astral_Scythe_Slash : ModProjectile {
		public override string Texture => typeof(Astral_Scythe).GetDefaultTMLName();
		public static int ExtraHitboxes => 1;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 60;
			Projectile.height = 60;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.noEnchantmentVisuals = true;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Projectile.ai[0] == 2 && !OriginsModIntegrations.CheckAprilFools()) Projectile.Kill();
			if (Projectile.ai[2] == 2) Projectile.ai[2] = 1;
			if (Projectile.ai[2] == 1) {
				Projectile.hide = true;
				Projectile.timeLeft = 2;
				if (Projectile.ai[1] > 1) {
					Projectile.ai[1] = 1;
				} else if (Projectile.ai[1] > 0) {
					Projectile.ai[1] = 0;
				} else {
					Projectile.Kill();
				}
				return;
			}
			Player player = Main.player[Projectile.owner];
			if (player.dead || player.CCed) {
				Projectile.active = false;
				return;
			}

			if (Projectile.ai[0] >= 1) Projectile.Size = new(60 / 1.4f);
			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			Projectile.rotation = MathHelper.Lerp(-2f, 1.3f, swingFactor) * player.direction;
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates + 2;
			if (Projectile.timeLeft <= 3) {
				Projectile.ai[2] = 2;
			}
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetCompositeArmPosition(false);
			if (swingFactor < 0.4f) {
				player.bodyFrame.Y = player.bodyFrame.Height * 1;
			} else if (swingFactor < 0.7f) {
				player.bodyFrame.Y = player.bodyFrame.Height * 2;
				Projectile.position.X += 6 * player.direction * (1 - (swingFactor - 0.4f) / 0.6f);
			} else {
				player.bodyFrame.Y = player.bodyFrame.Height * 3;
				Projectile.position.X += 3 * player.direction;
				Projectile.position.Y += 8;
			}

			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation * player.gravDir - MathHelper.PiOver4 * (player.gravDir - 1) * player.direction) * Projectile.width * 0.13f;
			for (int j = 0; j <= ExtraHitboxes; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
			}
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.ai[0] == 1) {
				Projectile.NewProjectile(
					source,
					Projectile.position,
					Projectile.velocity,
					ModContent.ProjectileType<Astral_Scythe_Blade>(),
					(int)(Projectile.damage * 0.2f),
					Projectile.knockBack,
					ai2: Projectile.identity
				);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.ai[2] == 1) return false;
			Player player = Main.player[Projectile.owner];
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.45f;
			vel.Y *= player.gravDir;
			int boxes = ExtraHitboxes;// - (Projectile.ai[0] >= 1 ? 1 : 0);
			for (int j = 0; j <= boxes; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * (j + 0.5f);
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Player player = Main.player[Projectile.owner];
			player.OriginPlayer().scytheHitCombo++;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Player player = Main.player[Projectile.owner];
			player.OriginPlayer().scytheHitCombo++;
		}
		public override void CutTiles() {
			Player player = Main.player[Projectile.owner];
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * new Vector2(1, player.gravDir) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			SpriteEffects effects = player.direction * player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (player.gravDir < 0) effects ^= SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: 3, frameY: Projectile.ai[0] >= 1 ? 1 : 0);

			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * player.direction * player.gravDir) - (player.gravDir < 0).Mul(MathHelper.PiOver2 * player.direction),
				new Vector2(8, 6).Apply(effects ^ SpriteEffects.FlipVertically, texture.Size() / new Vector2(1, 3)),
				Projectile.scale,
				effects
			);
			return false;
		}
	}
	public class Astral_Scythe_Blade : ModProjectile, IPreDrawSceneProjectile, ITriggerSCBackground {
		public override string Texture => "Origins/Gores/NPCs/Shimmer_Construct_Piece10";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 54;
			Projectile.height = 54;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.WhipSettings.Segments = 10;
			Projectile.WhipSettings.RangeMultiplier = 2;
			Projectile.WhipPointsForCollision.Clear();
		}
		public override bool ShouldUpdatePosition() => false;
		public void SetupControlPoints() {
			Projectile owner = Projectile.GetRelatedProjectile(2);
			if (owner?.active != true) {
				Projectile.Kill();
				return;
			}
			Player player = Main.player[Projectile.owner];
			Projectile.velocity = owner.velocity.RotatedBy(owner.rotation).Normalized(out _) * Projectile.velocity.Length() * player.direction;
			OriginExtensions.SwapClear(ref Projectile.WhipPointsForCollision, ref oldControlPoints);
			Projectile.spriteDirection = player.direction;
			Projectile.rotation = Projectile.velocity.ToRotation() + player.direction * MathHelper.PiOver2;
			Projectile.FillWhipControlPoints(
				owner.Center + new Vector2(57 * owner.direction, 58).RotatedBy(owner.rotation - MathHelper.PiOver2) * owner.scale * player.direction,
				Projectile.WhipPointsForCollision,
				player.itemTimeMax,
				(player.itemTimeMax - player.itemTime * 0.5f) - player.itemTimeMax * 0.15f,
				42
			);
		}
		(Vector2 start, Vector2 end)[] hitboxLines;
		int hitboxCacheTime = int.MinValue;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (projHitbox.Intersects(targetHitbox)) return true;
			targetHitbox.Inflate(4, 4); // it's easier to inflate a box than an unknown polygon
			if (oldControlPoints.Count == 0) {
				for (int i = 0; i < Projectile.WhipPointsForCollision.Count - 1; i++) {
					if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.WhipPointsForCollision[i], Projectile.WhipPointsForCollision[i  + 1])) return true;
				}
				return false;
			}
			if (hitboxCacheTime.TrySet(Projectile.timeLeft)) {
				hitboxLines = new (Vector2 start, Vector2 end)[Projectile.WhipPointsForCollision.Count + oldControlPoints.Count];
				for (int i = 0; i < Projectile.WhipPointsForCollision.Count - 1; i++) {
					hitboxLines[i] = (Projectile.WhipPointsForCollision[i], Projectile.WhipPointsForCollision[i + 1]);
				}
				hitboxLines[Projectile.WhipPointsForCollision.Count - 1] = (Projectile.WhipPointsForCollision[0], oldControlPoints[0]);

				int offset = Projectile.WhipPointsForCollision.Count;
				for (int i = 0; i < oldControlPoints.Count - 1; i++) {
					hitboxLines[i + offset] = (oldControlPoints[i], oldControlPoints[i + 1]);
				}
				hitboxLines[^1] = (Projectile.WhipPointsForCollision[^1], oldControlPoints[^1]);
			}
			return CollisionExt.PolygonIntersectsRect(hitboxLines, targetHitbox);
		}
		List<Vector2> oldControlPoints = [];
		public override void AI() {
			SetupControlPoints();
			Projectile.Center = Projectile.WhipPointsForCollision[^1];
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			SpriteEffects effects = player.direction * player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (player.gravDir < 0) effects ^= SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				(Projectile.WhipPointsForCollision[^1] - Projectile.WhipPointsForCollision[^2]).ToRotation() + MathHelper.Pi,
				new Vector2(21, 15).Apply(effects ^ SpriteEffects.FlipVertically, texture.Size()),
				Projectile.scale,
				effects
			);
			return false;
		}
		internal RenderTarget2D renderTarget;
		private static VertexStrip _vertexStrip = new();
		public void PreDrawScene() {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			Origins.shaderOroboros.Capture();

			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Identity"];
			miscShaderData.UseImage0(TextureAssets.Extra[197]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 0, 0, 0));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 0, 1, 1));
			float[] oldRot = new float[Projectile.WhipPointsForCollision.Count];
			for (int i = 0; i < Projectile.WhipPointsForCollision.Count - 1; i++) {
				oldRot[i] = (Projectile.WhipPointsForCollision[i + 1] - Projectile.WhipPointsForCollision[i]).ToRotation();
			}
			oldRot[^1] = Projectile.rotation;
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(Projectile.WhipPointsForCollision.ToArray(), oldRot, _ => Color.White, _ => 16, -Main.screenPosition, Projectile.WhipPointsForCollision.Count, includeBacksides: true);
			_vertexStrip.DrawTrail();

			Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = renderTarget.Size() * 0.5f;
			SC_Phase_Three_Midlay.DrawDatas.Add(new(
				renderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None
			));
		}
		public override void OnKill(int timeLeft) {
			if (renderTarget is not null) {
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref renderTarget);
				Main.OnResolutionChanged -= Resize;
			}
		}
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref renderTarget);
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
	public class Astral_Scythe_Star : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			ProjectileID.Sets.NoLiquidDistortion[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
			Projectile.timeLeft = 60 * 7;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = false;
			Projectile.scale = 0.85f;
		}
		public override void AI() {
			Projectile.velocity *= 0.94f;

			float angle = Projectile.velocity.ToRotation();
			float targetOffset = 0.9f;
			float targetAngle = 1;
			float dist = float.BitIncrement(16 * 10);

			bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
				Vector2 toHit = (Projectile.Center.Clamp(target.Hitbox.Add(target.velocity)) - Projectile.Center);
				if (!Collision.CanHitLine(Projectile.Center + Projectile.velocity, 1, 1, Projectile.Center + toHit, 1, 1)) return false;
				float tdist = toHit.Length();
				float ta = (float)Math.Abs(GeometryUtils.AngleDif(toHit.ToRotation(), angle, out _));
				if (target is Player) {
					tdist *= 2.5f;
					ta *= 2.5f;
				}
				if (tdist <= dist && ta <= targetOffset) {
					targetAngle = ((target.Center + target.velocity) - Projectile.Center).ToRotation();
					targetOffset = ta;
					dist = tdist;
					return true;
				}
				return false;
			});
			if (foundTarget) Projectile.velocity += new Vector2(1, 0).RotatedBy(targetAngle);
			if (Projectile.timeLeft < 15) {
				Projectile.Opacity = Projectile.timeLeft / 15f;
			}
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			int direction = Math.Sign(target.Center.X - Projectile.Center.X);
			if (direction == 0) direction = Main.rand.NextBool().ToDirectionInt();
			modifiers.HitDirectionOverride = direction;
			modifiers.KnockbackImmunityEffectiveness *= 0.8f;
			modifiers.Knockback.Base += 6;
		}
		public override bool PreDraw(ref Color lightColor) {
			Shimmerstar_Staff_P.DrawShimmerstar(Projectile);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Astral_Scythe_Wait_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			ID = Type;
		}
	}
}