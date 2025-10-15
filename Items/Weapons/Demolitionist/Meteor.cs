using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Items.Weapons.Melee;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Meteor : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Launcher,
			WikiCategories.CanistahUser
		];
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Meteor_P>(18, 32, 12.3f, 44, 24);
			Item.knockBack = 4;
			Item.reuseDelay = 6;
			Item.value = Item.sellPrice(silver:50);
			Item.rare = ItemRarityID.Orange;
			if (OriginsModIntegrations.CheckAprilFools()) Item.UseSound = null;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.MeteoriteBar, 20)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool? UseItem(Player player) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				SoundEngine.PlaySound(SoundID.Camera, player.MountedCenter);
				SoundEngine.PlaySound(SoundID.Item171.WithPitchOffset(1), player.MountedCenter);
			}
			return null;
		}
		static void DoFindAngle(Vector2 position, ref Vector2 velocity, ref int type) {
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
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
			if (OriginsModIntegrations.CheckAprilFools()) {
				velocity = new(velocity.Length() * player.direction, 0);
			} else {
				DoFindAngle(position, ref velocity, ref type);
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				position.X += player.direction * 16;
				DoFindAngle(position, ref velocity, ref type);
				Projectile.NewProjectile(
					source,
					position,
					velocity,
					type,
					damage,
					knockback
				);
				return false;
			}
			return true;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-2, 0);
		}
	}
	public class Meteor_P : ModProjectile, IIsExplodingProjectile, ICanisterProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Meteor_P_Cooling";
		AutoLoadingAsset<Texture2D> hotTexture = "Origins/Items/Weapons/Demolitionist/Meteor_P";
		public AutoLoadingAsset<Texture2D> OuterTexture => TextureAssets.Projectile[Type];
		public AutoLoadingAsset<Texture2D> InnerTexture => hotTexture;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
		}
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
				this.DoGravity(0.12f);
				Projectile.rotation += Projectile.velocity.X * 0.075f;
			} else if (++Projectile.ai[1] > 180) {
				/*Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Meteor_Explosion_2>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner
				);*/
				Projectile.Kill();
			}
			Lighting.AddLight(Projectile.Center, new Vector3(1.0f, 0.72f, 0.64f) * (1 - Projectile.ai[1] / 180));
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			behindNPCsAndTiles.Add(index);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
			if (Projectile.velocity.Y > 0 && (target.velocity.Y < 0 || !target.collideY) && !CritType.ModEnabled) {
				modifiers.CritDamage += Projectile.CritChance / 100f;
				modifiers.SetCrit();
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.velocity -= target.velocity * target.knockBackResist;
			if (!float.IsNaN(hit.Knockback)) target.velocity += Projectile.velocity.SafeNormalize(default) * hit.Knockback * (Projectile.velocity.Y > 0 ? 2 : 0.5f);
			target.SyncCustomKnockback();
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
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			if (canisterData.HasSpecialEffect) {
				Dust dust = Dust.NewDustDirect(
					projectile.position,
					projectile.width,
					projectile.height,
					DustID.RainbowMk2,
					-projectile.velocity.X,
					-projectile.velocity.Y,
					newColor: canisterData.InnerColor
				);
				dust.velocity *= 0.5f;
				dust.noGravity = true;
			}
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				projectile.Center - Main.screenPosition,
				null,
				lightColor,
				projectile.rotation,
				new Vector2(15, 13),
				projectile.scale,
				SpriteEffects.None
			);
			float factor = 1 - projectile.ai[1] / 180;
			Main.EntitySpriteDraw(
				hotTexture,
				projectile.Center - Main.screenPosition,
				null,
				new Color(factor, factor, factor, factor * 0.5f),
				projectile.rotation,
				new Vector2(15, 13),
				projectile.scale,
				SpriteEffects.None
			);
		}
		public bool IsExploding() => false;
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.timeLeft == 0) {
				//return target.;
			}
			return base.CanHitNPC(target);
		}
		public void DefaultExplosion(Projectile projectile, int fireDustType = DustID.Torch, int size = 96) {
			projectile.ResetLocalNPCHitImmunity();
			CanisterGlobalProjectile.DefaultExplosion(projectile, false);
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
			Projectile.tileCollide = false;
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
	public class Meteor_Crit_Type : CritType<Meteor> {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => projectile.velocity.Y > 0 && (target.velocity.Y < 0 || !target.collideY);
		public override float CritMultiplier(Player player, Item item) => 1.8f;
	}
}
