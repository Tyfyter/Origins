using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
using System;
using System.IO;
using Origins.Projectiles;
using Origins.Items.Weapons.Magic;
namespace Origins.Items.Weapons.Melee {
	public class Ripper_Lance : ModItem, ICustomWikiStat {
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			ItemID.Sets.Spears[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 20;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 52;
			Item.height = 56;
			Item.useTime = 35;
			Item.useAnimation = 35;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Ripper_Lance_P>();
			Item.shootSpeed = 3.75f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 9)
			.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool MeleePrefix() => true;
	}
	public class Ripper_Lance_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Ripper_Lance_P";
		static new AutoCastingAsset<Texture2D> GlowTexture;
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(base.GlowTexture);
			}
		}
		public override void Unload() {
			GlowTexture = null;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.timeLeft = 3600;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
			Projectile.scale = 1f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			projOwner.heldProj = Projectile.whoAmI;
			Projectile.direction = projOwner.direction;
			Projectile.spriteDirection = projOwner.direction;
			projOwner.itemTime = projOwner.itemAnimation;
			float totalTime = projOwner.itemAnimationMax / 2f;
			float progress = (projOwner.itemAnimation - totalTime) / totalTime;
			progress *= progress * progress;
			progress = MathHelper.Clamp(progress, -0.9f, 0.9f) / 0.9f;
			Vector2 direction = Projectile.velocity.RotatedBy(projOwner.direction * progress * -0.2f);
			
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			float movementFactor = (1 - Math.Abs(progress)) * 16f + 8;
			Projectile.Center = ownerMountedCenter + direction * movementFactor * Projectile.scale;
			if (progress >= 0 && Projectile.ai[2] == 0) {
				Projectile.ai[2] = 1;
				int type = ModContent.ProjectileType<Ripper_Lance_Spike>();
				for (int i = 0; i < 3; i++) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Projectile.velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(1.2f, 1.5f),
						type,
						Projectile.damage / 7,
						Projectile.knockBack,
						Projectile.owner
					);
				}
			}
			if (projOwner.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
			}
			Projectile.rotation = direction.ToRotation() + MathHelper.ToRadians(135f);
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.PiOver2;
			}
		}
		static bool handleHit = false;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			handleHit = false;
			if (projHitbox.Intersects(targetHitbox) || (handleHit = Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Main.player[Projectile.owner].MountedCenter + Projectile.velocity * 2, Projectile.Center))) {
				return true;
			}
			return null;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (handleHit) {
				modifiers.SourceDamage *= 0.65f;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition + Projectile.velocity * 3,
				new Rectangle(0, 0, 80, 84),
				lightColor,
				Projectile.rotation,
				new Vector2(40 + 40 * Projectile.spriteDirection, 0),
				Projectile.scale,
				Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			Main.EntitySpriteDraw(
				GlowTexture,
				Projectile.Center - Main.screenPosition + Projectile.velocity * 3,
				new Rectangle(0, 0, 80, 84),
				Color.White,
				Projectile.rotation,
				new Vector2(40 + 40 * Projectile.spriteDirection, 0),
				Projectile.scale,
				Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			return false;
		}
	}
	public class Ripper_Lance_Spike : Infusion_P {
		const int embed_duration = 30;
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 45;
		}
		public override void AI() {
			bool unembedded = EmbedTime <= 0;
			base.AI();
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			if (EmbedTime > 0) {
				if (unembedded) Projectile.timeLeft += 300;
				if (EmbedTime > 150) {
					Projectile.Kill();
				}
			}
		}
	}
}
