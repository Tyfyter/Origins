using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Shuckatana : ModItem {
		public static int MaxDecayFrames => 3;
		public static float DecayDuration => 4f * 60f;

		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			OriginsSets.Items.SwungNoMeleeMelees[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 7));
		}
		public override void SetDefaults() {
			Item.damage = 55;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 60;
			Item.height = 60;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.shoot = ModContent.ProjectileType<Shuckatana_Slash>();
			Item.shootSpeed = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
			Item.autoReuse = false;
		}
		public override bool AltFunctionUse(Player player) => player.OriginPlayer().shuckatanaDecay >= MaxDecayFrames;
		public override bool CanUseItem(Player player) {
			return true;
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) SoundEngine.PlaySound(SoundID.Item1, player.MountedCenter);
			else SoundEngine.PlaySound(SoundID.Item1.WithPitch(-0.3f), player.MountedCenter);
			return base.UseItem(player);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<Shuckatana_P>();
				velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX) * 10f;
			} else {
				const float sqrt_2 = 1.4142135623731f;
				velocity = new Vector2(sqrt_2 * player.direction, -sqrt_2) * velocity.Length();
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				player.OriginPlayer().shuckatanaDecay = 0;
				player.OriginPlayer().shuckatanaShootAnimationTimer = 18;
				Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<Shuckatana_ShootAnimation>(), 0, 0f);
				Projectile.NewProjectile(source, position, velocity, type, damage, 0f);
				return false;
			}
			foreach (Projectile p in Main.ActiveProjectiles) {
				if (p.owner == player.whoAmI && p.type == ModContent.ProjectileType<Shuckatana_Slash>()) {
					p.active = false;
					break;
				}
			}
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback);
			return false;
		}
		public override void HoldItem(Player player) {
			OriginPlayer op = player.OriginPlayer();
			if (op.shuckatanaShootAnimationTimer > 0) op.shuckatanaShootAnimationTimer--;
			if (player.altFunctionUse != 2 && player.itemAnimation > 0) {
				if (op.shuckatanaDecay < MaxDecayFrames) {
					op.shuckatanaDecay = Math.Min(MaxDecayFrames, op.shuckatanaDecay + MaxDecayFrames / DecayDuration);
				}
			}
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D texture = TextureAssets.Item[Type].Value;
			OriginPlayer op = Main.player[Main.myPlayer].OriginPlayer();
			int frameY;
			if (op.shuckatanaShootAnimationTimer > 0) {
				frameY = 4 + (int)((18 - op.shuckatanaShootAnimationTimer) / 6f);
			} else {
				frameY = (int)Math.Min(Math.Round(op.shuckatanaDecay), MaxDecayFrames);
			}
			frame = texture.Frame(verticalFrames: 7, frameY: frameY);
			spriteBatch.Draw(texture, position, frame, drawColor, 0, origin, scale * 1.2f, SpriteEffects.None, 0);
			return false;
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
			Texture2D texture = TextureAssets.Item[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: 7, frameY: 0);
			Vector2 origin = frame.Size() * 0.5f;
			spriteBatch.Draw(texture, Item.Center - Main.screenPosition, frame, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
	}
	public class Shuckatana_Slash : ModProjectile {
		public override string Texture => typeof(Shuckatana).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 7;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
			Projectile.width = 50;
			Projectile.height = 50;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || player.CCed || player.HeldItem.type != ModContent.ItemType<Shuckatana>()) {
				Projectile.active = false;
				return;
			}
			if (player.itemAnimation <= 0) {
				Projectile.active = false;
				return;
			}
			Projectile.velocity = new Vector2(player.direction, 0);
			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			Projectile.rotation = MathHelper.Lerp(-2.8f, 1.3f, swingFactor) * player.direction;
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates + 2;
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
			Projectile.EmitEnchantmentVisualsAt(Projectile.position, Projectile.width, Projectile.height);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Player player = Main.player[Projectile.owner];
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.7f;
			vel.Y *= player.gravDir;
			for (int j = 0; j <= 1; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * (j + 0.5f);
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) return true;
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			SpriteEffects effects = player.direction * player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (player.gravDir < 0) effects ^= SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			int frameY = Math.Min((int)Math.Round(player.OriginPlayer().shuckatanaDecay), Shuckatana.MaxDecayFrames);
			Rectangle frame = texture.Frame(verticalFrames: 7, frameY: frameY);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * player.direction * player.gravDir) - (player.gravDir < 0).Mul(MathHelper.PiOver2 * player.direction),
				new Vector2(8, 6).Apply(effects ^ SpriteEffects.FlipVertically, texture.Size() / new Vector2(1, 7)),
				Projectile.scale,
				effects
			);
			return false;
		}
	}
public class Shuckatana_ShootAnimation : ModProjectile {
	public override string Texture => typeof(Shuckatana).GetDefaultTMLName();
	public override void SetStaticDefaults() {
		Main.projFrames[Type] = 7;
	}
	public override void SetDefaults() {
		Projectile.DamageType = DamageClass.Melee;
		Projectile.friendly = false;
		Projectile.width = 1;
		Projectile.height = 1;
		Projectile.aiStyle = 0;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 18;
	}
	public override bool ShouldUpdatePosition() => false;
	public override void AI() {
		Player player = Main.player[Projectile.owner];
		if (player.dead || player.CCed || player.HeldItem.type != ModContent.ItemType<Shuckatana>()) {
			Projectile.active = false;
			return;
		}
		player.heldProj = Projectile.whoAmI;
		Projectile.velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
		Projectile.rotation = Projectile.velocity.ToRotation();
		float realRotation = Projectile.rotation;
		player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
		Projectile.Center = player.GetCompositeArmPosition(false);
	}
	public override bool? CanHitNPC(NPC target) => false;
	public override bool PreDraw(ref Color lightColor) {
		Player player = Main.player[Projectile.owner];
		SpriteEffects effects = player.direction * player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
		if (player.gravDir < 0) effects ^= SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
		Texture2D texture = TextureAssets.Projectile[Type].Value;
		int frameY = Math.Clamp(4 + (int)((18 - Projectile.timeLeft) / 6f), 4, 6);
		Rectangle frame = texture.Frame(verticalFrames: 7, frameY: frameY);
		Main.EntitySpriteDraw(
			texture,
			Projectile.Center - Main.screenPosition,
			frame,
			lightColor,
			Projectile.rotation + (0.25f * player.direction) + (MathHelper.PiOver4 * player.direction * player.gravDir) - (player.gravDir < 0).Mul(MathHelper.PiOver2 * player.direction),
			new Vector2(8, 6).Apply(effects ^ SpriteEffects.FlipVertically, texture.Size() / new Vector2(1, 7)),
			Projectile.scale,
			effects
		);
		return false;
	}
}
	public class Shuckatana_P : ModProjectile {
		public static int LingerTime => 180;
		public static int CrumbleFrames => 3;
		bool Lingering {
			get => Projectile.ai[1] == 1f;
			set => Projectile.ai[1] = value ? 1f : 0f;
		}
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 6;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 60 * 3;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
		}
		public override void AI() {
			if (!Lingering) {
				Projectile.velocity.Y += 0.05f;
				Projectile.rotation = Projectile.velocity.ToRotation();
				if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
					Lingering = true;
					Projectile.timeLeft = LingerTime + CrumbleFrames * 6;
					Projectile.penetrate = -1;
					Projectile.velocity = Vector2.Zero;
					SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
				}
			}
			if (Lingering) {
				Projectile.velocity = Vector2.Zero;
				if (Projectile.ai[0] > 0) {
					NPC target = Main.npc[(int)Projectile.ai[0] - 1];
					if (target.active) {
						Projectile.Center = target.Center + new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
					} else {
						Projectile.active = false;
					}
				}
				if (Projectile.timeLeft <= CrumbleFrames * 6) {
					int frameIndex = CrumbleFrames - 1 - (int)((Projectile.timeLeft / (float)(CrumbleFrames * 6)) * CrumbleFrames);
					Projectile.frame = Math.Clamp(1 + frameIndex, 1, 3);
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Lingering) return;
			Lingering = true;
			Projectile.timeLeft = LingerTime + CrumbleFrames * 6;
			Projectile.penetrate = -1;
			Projectile.velocity = Vector2.Zero;
			Projectile.ai[0] = target.whoAmI + 1;
			Projectile.localAI[0] = Projectile.Center.X - target.Center.X;
			Projectile.localAI[1] = Projectile.Center.Y - target.Center.Y;
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: 4, frameY: Projectile.frame);
			Vector2 origin = new Vector2(frame.Width * 0.75f, frame.Height * 0.5f);
			if (!Lingering) {
				for (int i = 0; i < Projectile.oldPos.Length; i++) {
					float opacity = (1f - i / (float)Projectile.oldPos.Length) * 0.5f;
					Main.EntitySpriteDraw(
						texture,
						Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition,
						frame,
						lightColor * opacity,
						Projectile.oldRot[i],
						origin,
						Projectile.scale,
						SpriteEffects.None
					);
				}
			}
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				origin,
				Projectile.scale,
				SpriteEffects.None
			);
			return false;
		}
	}
}