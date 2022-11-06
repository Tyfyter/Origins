using Terraria.GameContent.Creative;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Items.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
	public class Parasitic_Influence : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Parasitic Influence");
			Tooltip.SetDefault("Amebic tentacles will protect you from projectiles");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.YoYoGlove);
			Item.handOffSlot = -1;
			Item.handOnSlot = -1;
			Item.damage = 20;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 3;
			Item.useTime = Item.useAnimation = 30;
			Item.shoot = ModContent.ProjectileType<Parasitic_Influence_Tentacle>();
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateAccessory(Player player, bool isHidden) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.parasiticInfluenceCooldown > 0) return;
			const float maxDist = 64 * 64;
			Vector2 target = default;
			float bestWeight = 0;
			Vector2 currentPos;
			Vector2 diff;
			NPC npc;
			for (int i = 0; i < Main.maxNPCs; i++) {
				npc = Main.npc[i];
				if (npc.CanBeChasedBy() && npc.aiStyle != NPCAIStyleID.Spell) {
					currentPos = npc.Hitbox.ClosestPointInRect(player.MountedCenter);
					diff = player.Hitbox.ClosestPointInRect(npc.Center) - currentPos;
					float dist = diff.LengthSquared();
					if (dist > maxDist) continue;
					float currentWeight = Vector2.Dot(npc.velocity, diff.SafeNormalize(default)) * dist;
					if (currentWeight > bestWeight) {
						bestWeight = currentWeight;
						target = currentPos;
					}
				}
			}
			if (bestWeight > 0) {
				Projectile.NewProjectile(
					player.GetSource_Accessory(Item),
					player.MountedCenter,
					(target - player.MountedCenter).SafeNormalize(default) * 4.2f,
					Item.shoot,
					player.GetWeaponDamage(Item),
					player.GetWeaponKnockback(Item),
					player.whoAmI
				);
				originPlayer.parasiticInfluenceCooldown = (byte)Item.useTime;
			}
		}
		public override int ChoosePrefix(UnifiedRandom rand) {
			if (!Item.noUseGraphic) {
				Item.noUseGraphic = true;
				Item.accessory = rand.NextBool();
				Item.Prefix(-2);
				Item.accessory = true;
				Item.noUseGraphic = false;
				return Item.prefix;
			}
			return -1;
		}
		public override bool MeleePrefix() => true;
		public override bool WeaponPrefix() => false;
	}
	public class Parasitic_Influence_Tentacle : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Riven/Flagellash_P";
		public override string GlowTexture => Texture;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Parasitic Influence");
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ItemID.Spear);
			Projectile.timeLeft = 30;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 0;
			Projectile.tileCollide = false;
			Projectile.alpha = 150;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);

			Projectile.Center = ownerMountedCenter;

			if (movementFactor == 0f) {
				movementFactor = 5.1f;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft < 15) {
				movementFactor -= 1f;
			} else {
				movementFactor += 1f;
			}

			Projectile.position += Projectile.velocity * movementFactor;

			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.PiOver2;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (projHitbox.Intersects(targetHitbox) || Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Main.player[Projectile.owner].MountedCenter + Projectile.velocity * 2, Projectile.Center)) {
				return true;
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Vector2 diff = Projectile.Center - ownerMountedCenter;
			int dist = (int)Math.Min(diff.Length(), 180);
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				ownerMountedCenter - Main.screenPosition,
				new Rectangle(0, 180 - dist, 6, dist),
				Color.White,
				Projectile.rotation,
				new Vector2(3, 0),
				Projectile.scale,
				SpriteEffects.None,
			0);
			return false;
		}
	}
}
