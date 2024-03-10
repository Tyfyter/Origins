using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Demolitionist {
	public class Meteor : ModItem {
		public string[] Categories => new string[] {
			"Launcher",
			"MineUser"
		};
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 12;
			Item.crit = 0;
			Item.useAnimation = 32;
			Item.useTime = 32;
			Item.useAmmo = ModContent.ItemType<Ammo.Resizable_Mine_One>();
			Item.shoot = ModContent.ProjectileType<Meteor_P>();
			Item.shootSpeed = 8.3f;
			Item.reuseDelay = 6;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(silver:50);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.MeteoriteBar, 20)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
			float speed = velocity.Length();
			Vector2 target = Main.MouseWorld - position;
			float dist = Math.Abs(target.X);
			if (dist < 300) {
				speed *= (300 * 4 + dist) / (300 * 5);
			}
			int tries = 0;
			loop:
			float? angle = GeometryUtils.AngleToTarget(target, speed, 0.12f, true);
			if (!angle.HasValue) {
				if (++tries > 100) {
					type = ModContent.ProjectileType<Meteor_Explosion_1>();
					return;
				}
				target.Y += 8;
				goto loop;
			}
			velocity = new Vector2(speed, 0).RotatedBy(angle.Value);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-2, 0);
		}
	}
	public class Meteor_P : ModProjectile {
		public override string Texture => base.Texture + "_Cooling";
		protected override bool CloneNewInstances => true;
		AutoLoadingAsset<Texture2D> hotTexture = "Origins/Items/Weapons/Demolitionist/Meteor_P";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 600;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.width = 30;
			Projectile.height = 26;
			Projectile.hide = true;
			Projectile.extraUpdates = 1;
			Projectile.localNPCHitCooldown = 60;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.friendly = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				Projectile.velocity.Y += 0.12f;
			} else if (++Projectile.ai[1] > 180) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Meteor_Explosion_2>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner
				);
				Projectile.Kill();
			}
			Lighting.AddLight(Projectile.Center, new Vector3(1.0f, 0.72f, 0.64f) * (1 - Projectile.ai[1] / 180));
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
			if (Projectile.velocity.Y > 0 && (target.velocity.Y < 0 || !target.collideY)) {
				modifiers.CritDamage += Projectile.CritChance / 100f;
				modifiers.SetCrit();
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.velocity -= target.velocity * target.knockBackResist;
			target.velocity += Projectile.velocity.SafeNormalize(default) * hit.Knockback * (Projectile.velocity.Y > 0 ? 2 : 0.5f);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[0] = 1;
			Projectile.tileCollide = false;
			float len = oldVelocity.Length();
			Projectile.position += Projectile.velocity + oldVelocity * (8 / len);
			Projectile.velocity = default;
			if (Main.myPlayer == Projectile.owner) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Meteor_Explosion_1>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner
				);
			}
			Projectile.friendly = false;
			return false;
		}
		public override void PostDraw(Color lightColor) {
			float factor = (1 - Projectile.ai[1] / 180);
			Main.EntitySpriteDraw(
				hotTexture,
				Projectile.Center - Main.screenPosition,
				null,
				new Color(factor, factor, factor, factor * 0.5f),
				Projectile.rotation,
				new Vector2(15, 13),
				Projectile.scale,
				SpriteEffects.None
			);
		}
	}
	public class Meteor_Explosion_1 : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GeyserTrap;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 5;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.hide = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(
					Projectile,
					true,
					sound: SoundID.Item62
				);
				Projectile.ai[0] = 1;
			}
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding() => true;
	}
	public class Meteor_Explosion_2 : Meteor_Explosion_1 {
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(
					Projectile,
					true,
					sound: SoundID.Item62
				);
				Vector2 center = Projectile.Center;
				int radius = 4;
				int minI = (int)(center.X / 16f - radius);
				int maxI = (int)(center.X / 16f + radius);
				int minJ = (int)(center.Y / 16f - radius);
				int maxJ = (int)(center.Y / 16f + radius);
				if (minI < 0) minI = 0;
				if (maxI > Main.maxTilesX) maxI = Main.maxTilesX;
				if (minJ < 0) minJ = 0;
				if (maxJ > Main.maxTilesY) maxJ = Main.maxTilesY;
				Projectile.ExplodeTiles(
					center,
					radius,
					minI,
					maxI,
					minJ,
					maxJ,
					Projectile.ShouldWallExplode(center, radius, minI, maxI, minJ, maxJ)
				);
				Projectile.ai[0] = 1;
			}
		}
	}
}
