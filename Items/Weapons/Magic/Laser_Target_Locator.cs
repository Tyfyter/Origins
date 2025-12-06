using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Tools;
using Origins.Items.Weapons.Melee;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Empowerments;

namespace Origins.Items.Weapons.Magic {
	public class Laser_Target_Locator : ModItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ItemID.Sets.IsSpaceGun[Type] = true;
		}
		public override void SetDefaults() {
			Item.autoReuse = false;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item157;
			Item.damage = 80;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.shootSpeed = 1f;
			Item.noMelee = true;
			Item.width = 26;
			Item.height = 18;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.knockBack = 16;
			Item.mana = 60;
			Item.shoot = ModContent.ProjectileType<Laser_Target_Locator_P>();
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Lime;
			Item.channel = true;
		}
		public override void OnConsumeMana(Player player, int manaConsumed) {
			if (player.ownedProjectileCounts[Item.shoot] <= 0) player.statMana += manaConsumed;
		}
	}
	public class Laser_Target_Locator_P : ModProjectile {
		public override string Texture => typeof(Laser_Target_Locator).GetDefaultTMLName();
		public float ChargeTime => Projectile.localAI[0] * 2;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600 + 64;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			DrawHeldProjInFrontOfHeldItemAndArms = true;
		}
		public override bool ShouldUpdatePosition() => false;
		public Vector2 TargetPos {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set => (Projectile.ai[0], Projectile.ai[1]) = value;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent { Entity : Player player }) Projectile.localAI[0] = player.itemAnimationMax;
			Projectile.velocity = Projectile.velocity.Normalized(out Projectile.localAI[1]);
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.heldProj = Projectile.whoAmI;
			SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(Projectile.ai[2]/10).WithVolume(0.24f), player.position);
			if (!player.channel) {
				if (Projectile.IsLocallyOwned() && Projectile.ai[2] < 0) {
					SoundEngine.SoundPlayer.Play(SoundID.ResearchComplete.WithPitch(-2f).WithVolume(0.5f), TargetPos);
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						TargetPos,
						default,
						ModContent.ProjectileType<Laser_Target_Locator_Marker>(),
						Projectile.damage,
						Projectile.knockBack,
						ai1: Projectile.localAI[1],
						ai2: Projectile.localAI[0]
					);
				}
				Projectile.Kill();
			} else {
				player.SetDummyItemTime(5);
				if (Projectile.IsLocallyOwned()) {
					Vector2 simpleDir = Main.MouseWorld - player.MountedCenter;
					simpleDir.Y *= player.gravDir;
					float aimDownFix = simpleDir.Y < 0 ? simpleDir.Normalized(out _).Y * -4 : 0;
					Vector2 position = player.MountedCenter - new Vector2(aimDownFix * player.direction, 2f * player.gravDir);
					Vector2 direction = Main.MouseWorld - position;

					Vector2 velocity = Vector2.Normalize(direction);
					if (velocity.HasNaNs()) velocity = -Vector2.UnitY;
					if (Projectile.velocity != velocity) {
						Projectile.velocity = velocity;
						Projectile.netUpdate = true;
					}
					Projectile.position = position + velocity * (28 - aimDownFix);
				}
				Vector2 newTarget = Projectile.position + Projectile.velocity * Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64);
				Projectile.ai[2] -= float.Pow(Projectile.ai[0] - newTarget.X, 2) * 0.01f;
				Max(ref Projectile.ai[2], 0);
				TargetPos = newTarget;
				player.ChangeDir(Math.Sign(Projectile.velocity.X));
				player.itemRotation = Projectile.velocity.ToRotation();
				if (player.direction == -1) player.itemRotation = MathHelper.WrapAngle(player.itemRotation + MathHelper.Pi);
				if (++Projectile.ai[2] >= ChargeTime && player.CheckMana(player.HeldItem, pay: true)) {
					player.reuseDelay = 60;
					player.channel = false;
					Projectile.ai[2] = -1;
				}
			}
		}
		public override void OnKill(int timeLeft) { }
		public override bool PreDraw(ref Color lightColor) {
			if (!Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;
			Vector2 diff = TargetPos - Projectile.position;
			Vector2 position = Projectile.position;
			position -= Main.screenPosition;
			float rotation = diff.ToRotation();
			float dist = diff.Length();
			const float scale = 1f / 256f;
			DrawData data = new(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,//TextureAssets.MagicPixel.Value,
				position,
				null,
				new(0, 255, 0, 0),
				rotation,
				Vector2.UnitY * 128,
				new Vector2(dist * scale, 8 * scale),
				0
			);
			data.Draw(Main.spriteBatch);
			float progress = Projectile.ai[2] / ChargeTime;
			Min(ref progress, 1);
			data.color *= progress;
			Vector2 offset = (rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - progress) * 8;
			data.position = position + offset;
			data.scale.X = Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist * 1.15f + 16) * scale;
			data.Draw(Main.spriteBatch);
			data.position = position - offset;
			data.scale.X = Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist * 1.15f + 16) * scale;
			data.Draw(Main.spriteBatch);
			return false;
		}
		public float Raymarch(Vector2 position, Vector2 direction, float maxLength = float.PositiveInfinity) {
			float dist = CollisionExt.Raymarch(position, direction, maxLength);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (dist < 16) return dist;
				if (npc.friendly) continue;
				if (position.Clamp(npc.Hitbox).DistanceSQ(position) >= dist * dist) continue;
				float collisionPoint = 1;
				if (Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, position + direction * dist, 1, ref collisionPoint)) {
					Min(ref dist, collisionPoint);
				}
			}
			Player owner = Main.player[Projectile.owner];
			if (owner.hostile) {
				int team = owner.team;
				if (team == 0) team = -1;
				foreach (Player player in Main.ActivePlayers) {
					if (dist < 16) return dist;
					if (player.whoAmI == Projectile.owner) continue;
					if (!player.hostile || player.team == team) continue;
					if (position.Clamp(player.Hitbox).DistanceSQ(position) >= dist * dist) continue;
					float collisionPoint = 1;
					if (Collision.CheckAABBvLineCollision(player.position, player.Size, position, position + direction * dist, 1, ref collisionPoint)) {
						Min(ref dist, collisionPoint);
					}
				}
			}
			return dist;
		}
	}
	public class Laser_Target_Locator_Marker : ModProjectile {
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = 36;
			Projectile.height = 32;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if ((int)(++Projectile.ai[0] / Projectile.ai[2]) > Projectile.localAI[1] && Projectile.IsLocallyOwned()) {
				const float spawn_dist = 2400;
				Vector2 position = Projectile.Center;
				position.X += Main.rand.NextFloat(16 * 5) * Main.rand.NextBool().ToDirectionInt();
				Vector2 direction = Vector2.UnitY.RotatedByRandom(0.2f);
				float speed = 32 * Projectile.ai[1];
				SoundEngine.SoundPlayer.Play(SoundID.Item92.WithPitch(-1.2f).WithPitchVarience(1f).WithVolume(0.6f), position);
				SoundEngine.SoundPlayer.Play(SoundID.Item103.WithPitch(-1.2f).WithPitchVarience(1f), position);
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					position - direction * spawn_dist,
					direction * speed,
					ModContent.ProjectileType<Laser_Target_Locator_Missile>(),
					Projectile.damage,
					Projectile.knockBack,
					ai0: (spawn_dist - CollisionExt.Raymarch(position, -direction, spawn_dist)) / speed
				);
				if (++Projectile.localAI[1] >= 16) Projectile.Kill();
			}
		}
		public override Color? GetAlpha(Color lightColor) => Color.White * Utils.Remap(float.Sin(Projectile.ai[0] * (MathHelper.Pi / Projectile.ai[2])), -1, 1, 0.25f, 1f);
	}
	public class Laser_Target_Locator_Missile : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = Projectile.height = 34;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
		}
		Vector2 HitboxMovement => new Vector2(16 * Projectile.ai[1], 0).RotatedBy(Projectile.ai[1] * Projectile.ai[0]);
		public override void AI() {
			Dust.NewDustPerfect(Projectile.Top, DustID.Torch, Projectile.velocity * 0.85f).noGravity = true;
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (++Projectile.frameCounter >= 12) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
			}
			Projectile.tileCollide = --Projectile.ai[0] <= 0;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => null;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.life <= 0) Projectile.penetrate++;
		}
		public override void OnKill(int timeLeft) {
			Main.instance.CameraModifiers.Add(new CameraShakeModifier(
				Projectile.Center, 10f, 3f, 23, 500f, -1f, nameof(Laser_Target_Locator)
			));
			ExplosiveGlobalProjectile.DoExplosion(
				Projectile,
				128,
				sound: SoundID.Item62.WithPitch(-0.5f)
			);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				TextureAssets.Projectile[Type].Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				lightColor,
				Projectile.rotation,
				new(112 - 17, 17),
				1,
				SpriteEffects.None
			);
			return false;
		}
	}
}
