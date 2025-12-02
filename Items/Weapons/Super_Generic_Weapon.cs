using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Origins.Tiles.Other;
using PegasusLib;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons {
	public class Super_Generic_Weapon : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = Type;
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClass.Generic;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.damage = 30;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.width = 20;
			Item.height = 14;
			Item.shoot = ModContent.ProjectileType<Super_Generic_Weapon_P>();
			Item.shootSpeed = 0f;
			Item.knockBack = 2.5f;
			Item.value = Item.sellPrice(copper: 1);
			Item.rare = ItemRarityID.White;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8, 2);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position = Main.MouseWorld;
		}
	}
	public class Super_Generic_Weapon_P : ModProjectile {
		public override string Texture => typeof(Generic_Weapon).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Generic;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.ignoreWater = false;
			Projectile.extraUpdates = 0;
			Projectile.penetrate = -1;
			Projectile.width = 20;
			Projectile.height = 14;
			Projectile.timeLeft = 5 * 60;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(Projectile.Center.X - target.Center.X);
		}
		public override Color? GetAlpha(Color lightColor) => new(255, 255, 255, 211);
		public override void AI() {
			foreach (NPC target in Main.ActiveNPCs) {
				if ((target.CanBeChasedBy() || (target.lifeMax == 1 && !target.dontTakeDamage))) {
					Vector2 direction = (target.Center - Projectile.Center).Normalized(out float dist);
					float distSQ = float.Pow(dist / 16, 1.5f);
					float force = (2 / distSQ) * (1 - (1 - target.knockBackResist) * 0.95f);
					if (float.IsNaN(force)) force = 0;
					Min(ref force, dist);
					if (force > 0.1f) {
						target.velocity -= direction * force;
					}
				}
			}
		}
	}
}
