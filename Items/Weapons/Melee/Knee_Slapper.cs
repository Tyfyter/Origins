using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Projectiles;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Knee_Slapper : ModItem, ICustomWikiStat {
		static short glowmask;
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.damage = 45;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 30;
			Item.height = 36;
			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Knee_Slapper_P>();
			Item.shootSpeed = 16f;
			Item.useTurn = true;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.glowMask = glowmask;
		}
		public override bool MeleePrefix() => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, player.itemAnimationMax, velocity.ToRotation());
			return false;
		}
	}
	public class Knee_Slapper_P : ModProjectile {
		static AutoLoadingAsset<Texture2D> HeadTexture = "Origins/Items/Weapons/Melee/Knee_Slapper_Head";
		static AutoLoadingAsset<Texture2D> HeadGlowTexture = "Origins/Items/Weapons/Melee/Knee_Slapper_Head_Glow";
		static AutoLoadingAsset<Texture2D> BodyTexture = "Origins/Items/Weapons/Melee/Knee_Slapper_Body";
		static AutoLoadingAsset<Texture2D> BodyGlowTexture = "Origins/Items/Weapons/Melee/Knee_Slapper_Body_Glow";
		static AutoLoadingAsset<Texture2D> TailTexture = "Origins/Items/Weapons/Melee/Knee_Slapper_Tail";
		static AutoLoadingAsset<Texture2D> TailGlowTexture = "Origins/Items/Weapons/Melee/Knee_Slapper_Tail_Glow";
		static bool lastSlapDir = false;
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";
		public List<PolarVec2> nodes;
		PolarVec2 GetSwingStartOffset => new(0, Projectile.ai[1] - Projectile.direction * 0.35f);
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.timeLeft = 40;
			Projectile.localNPCHitCooldown = 15;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 3;
			Projectile.ownerHitCheck = false;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (nodes is null) {
				Projectile.timeLeft = (int)(Projectile.ai[0] * 4);
				Projectile.localNPCHitCooldown = Projectile.timeLeft;
				Projectile.localAI[1] = 16f / Projectile.ai[0];
				Projectile.localAI[0] = owner.direction;
				Projectile.ai[0] = Projectile.direction = (lastSlapDir = !lastSlapDir) ? 1 : -1;
				float offset = Projectile.direction * 0.1f * Projectile.localAI[1];
				nodes = [
					new(24 * Projectile.scale, -offset),
					new(24 * Projectile.scale, -offset * 2),
					new(24 * Projectile.scale, -offset * 3),
					new(24 * Projectile.scale, -offset * 4),
					new(36 * Projectile.scale, -offset * 5)
				];
			}
			owner.direction = (int)Projectile.localAI[0];
			Projectile.direction = (int)Projectile.ai[0];
			Vector2 basePosition = owner.MountedCenter;
			PolarVec2 position = GetSwingStartOffset;
			for (int i = 0; i < nodes.Count; i++) {
				PolarVec2 vec = nodes[i];
				position.R += vec.R;
				position.Theta += vec.Theta;
				vec.Theta += Projectile.direction * 0.015f * Projectile.localAI[1];// / (i + 1);
				nodes[i] = vec;
			}
			owner.heldProj = Projectile.whoAmI;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (nodes is null) return false;
			Projectile.direction = (int)Projectile.ai[0];
			Player owner = Main.player[Projectile.owner];
			Vector2 basePosition = owner.MountedCenter;
			PolarVec2 position = GetSwingStartOffset;
			Vector2 lastPosition = basePosition;
			for (int i = 0; i < nodes.Count; i++) {
				PolarVec2 vec = nodes[i];
				position.R += vec.R;
				position.Theta += vec.Theta;
				float collisionPoint = 0;
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lastPosition, basePosition + (Vector2)position, 8, ref collisionPoint)) {
					return true;
				}
				lastPosition = basePosition + (Vector2)position;
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Projectile.direction = (int)Projectile.ai[0];
			Player owner = Main.player[Projectile.owner];
			Vector2 basePosition = owner.MountedCenter;
			PolarVec2 position = GetSwingStartOffset;
			Vector2 lastPosition = basePosition;
			PolarVec2 diff = default;
			for (int i = 0; i < nodes.Count - 1; i++) {
				PolarVec2 vec = nodes[i];
				position.R += vec.R;
				position.Theta += vec.Theta;
				diff = (PolarVec2)(basePosition + (Vector2)position - lastPosition);
				if (i == 0) {
					Main.EntitySpriteDraw(
						TailTexture,
						lastPosition - Main.screenPosition,
						null,
						new Color(Lighting.GetSubLight(lastPosition)),
						diff.Theta,
						new Vector2(-8, 21),
						new Vector2(diff.R / 30f, 0.9f) * Projectile.scale,
						SpriteEffects.None,
					0);
					TangelaVisual.DrawTangela(
						TailGlowTexture,
						lastPosition - Main.screenPosition,
						null,
						diff.Theta,
						new Vector2(-8, 21),
						new Vector2(diff.R / 30f, 0.9f) * Projectile.scale,
						SpriteEffects.None,
						i + Projectile.owner
					);
				} else {
					Main.EntitySpriteDraw(
						BodyTexture,
						lastPosition - Main.screenPosition,
						null,
						new Color(Lighting.GetSubLight(lastPosition)),
						diff.Theta,
						new Vector2(2, 21),
						new Vector2(diff.R / 30f, 0.9f) * Projectile.scale,
						(i % 2 == 0) ? SpriteEffects.None : SpriteEffects.FlipVertically,
					0);
					TangelaVisual.DrawTangela(
						BodyGlowTexture,
						lastPosition - Main.screenPosition,
						null,
						diff.Theta,
						new Vector2(2, 21),
						new Vector2(diff.R / 30f, 0.9f) * Projectile.scale,
						(i % 2 == 0) ? SpriteEffects.None : SpriteEffects.FlipVertically,
						i + Projectile.owner
					);
				}
				lastPosition = basePosition + (Vector2)position;
			}

			Main.EntitySpriteDraw(
				HeadTexture,
				lastPosition - Main.screenPosition,
				null,
				new Color(Lighting.GetSubLight(lastPosition)),
				diff.Theta,
				new Vector2(2, 21),
				new Vector2(1, 0.9f) * Projectile.scale,
				SpriteEffects.None,
			0);
			Main.EntitySpriteDraw(
				HeadGlowTexture,
				lastPosition - Main.screenPosition,
				null,
				Color.White,
				diff.Theta,
				new Vector2(2, 21),
				new Vector2(1, 0.9f) * Projectile.scale,
				SpriteEffects.None,
			0);
			return false;
		}
	}
}
