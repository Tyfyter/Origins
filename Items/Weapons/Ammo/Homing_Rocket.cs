using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public interface IRocketItem : ICanisterAmmo {
		public static abstract int ExplosionSize { get; }
		public static virtual int TileDestructionRadius => 0;
		public static abstract Color InnerColor { get; }
		public static abstract Color OuterColor { get; }
		public sealed static void DoAI(Projectile projectile) {
			float targetWeight = 300;
			Vector2 targetPos = default;
			bool foundTarget = Main.player[projectile.owner].DoHoming((target) => {
				Vector2 currentPos = target.Center;
				float dist = Math.Abs(projectile.Center.X - currentPos.X) + Math.Abs(projectile.Center.Y - currentPos.Y);
				if (target is Player) dist *= 2.5f;
				if (dist < targetWeight && Collision.CanHit(projectile.position, projectile.width, projectile.height, target.position, target.width, target.height)) {
					targetWeight = dist;
					targetPos = currentPos;
					return true;
				}
				return false;
			});

			if (foundTarget) {
				float scaleFactor = 16f;
				float lerpValue = 0.083333336f * Origins.HomingEffectivenessMultiplier[projectile.type];

				Vector2 targetVelocity = (targetPos - projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
				projectile.velocity = Vector2.Lerp(projectile.velocity, targetVelocity, lerpValue);
			}
		}
		public new sealed static CanisterData GetCanisterData<TRocketItem>() where TRocketItem : IRocketItem => new(TRocketItem.OuterColor, TRocketItem.InnerColor, false);
	}
	public abstract class Homing_Rocket_Version<TRocketItem>(int launcherType, int projectileType) : ModProjectile where TRocketItem : ModItem, IRocketItem {
		private readonly int launcherType = launcherType;
		private readonly int projectileType = projectileType;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[launcherType][ModContent.ItemType<TRocketItem>()] = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(projectileType);
			AIType = projectileType;
		}
		public override void AI() => IRocketItem.DoAI(Projectile);
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return base.OnTileCollide(oldVelocity);
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, TRocketItem.ExplosionSize);
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			int tileDestructionRadius = TRocketItem.TileDestructionRadius;
			if (tileDestructionRadius > 0) {
				tileDestructionRadius *= 16;
				tileDestructionRadius = (int)Projectile.GetGlobalProjectile<ExplosiveGlobalProjectile>().modifierBlastRadius
					.CombineWith(Main.player[Projectile.owner].GetModPlayer<OriginPlayer>().explosiveBlastRadius)
					.Scale(0.5f)
					.ApplyTo(tileDestructionRadius);
				tileDestructionRadius /= 16;
				Vector2 center = Projectile.Center;
				int i = (int)(center.X / 16);
				int j = (int)(center.Y / 16);
				int minI = Math.Max(i - tileDestructionRadius, 0);
				int maxI = Math.Min(i + tileDestructionRadius, Main.maxTilesX);
				int minJ = Math.Max(j - tileDestructionRadius, 0);
				int maxJ = Math.Min(j + tileDestructionRadius, Main.maxTilesY);
				Projectile.ExplodeTiles(
					center,
					tileDestructionRadius,
					minI,
					maxI,
					minJ,
					maxJ,
					Projectile.ShouldWallExplode(center, tileDestructionRadius, minI, maxI, minJ, maxJ)
				);
			}
		}
	}
	public class Homing_Rocket_I : ModItem, IRocketItem {
		public static int ExplosionSize => 128;
		public static Color InnerColor => FromHexRGB(0xED1C24);
		public static Color OuterColor => Color.White;
		public CanisterData GetCanisterData => IRocketItem.GetCanisterData<Homing_Rocket_I>();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.Celeb2][Type] = ProjectileID.Celeb2Rocket;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.damage = 40;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = Item.CommonMaxStack;
			Item.ammo = AmmoID.Rocket;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.Red;
		}
		public void AI(Projectile projectile, bool child) => IRocketItem.DoAI(projectile);
	}
	public class Homing_Rocket_I_Rocket() : Homing_Rocket_Version<Homing_Rocket_I>(ItemID.RocketLauncher, ProjectileID.RocketI) {
		public override string Texture => typeof(Homing_Rocket_I).GetDefaultTMLName();
		public override void PostAI() {
			Projectile.rotation += MathHelper.PiOver2;
		}
	}
	public class Homing_Rocket_I_Grenade() : Homing_Rocket_Version<Homing_Rocket_I>(ItemID.GrenadeLauncher, ProjectileID.GrenadeI) {}
	public class Homing_Rocket_I_Mine() : Homing_Rocket_Version<Homing_Rocket_I>(ItemID.ProximityMineLauncher, ProjectileID.ProximityMineI) {}
	public class Homing_Rocket_I_Snowman() : Homing_Rocket_Version<Homing_Rocket_I>(ItemID.SnowmanCannon, ProjectileID.RocketSnowmanI) {}
	public class Homing_Rocket_II : ModItem, IRocketItem {
		public static int ExplosionSize => 128;
		public static int TileDestructionRadius => 3;
		public static Color InnerColor => FromHexRGB(0x434BE1);
		public static Color OuterColor => Color.White;
		public CanisterData GetCanisterData => IRocketItem.GetCanisterData<Homing_Rocket_II>();
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.Celeb2][Type] = ProjectileID.Celeb2RocketExplosive;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Item.damage = 40;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = Item.CommonMaxStack;
			Item.ammo = AmmoID.Rocket;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.Red;
		}
		public void AI(Projectile projectile, bool child) => IRocketItem.DoAI(projectile);
	}
	public class Homing_Rocket_II_Rocket() : Homing_Rocket_Version<Homing_Rocket_II>(ItemID.RocketLauncher, ProjectileID.RocketI) {
		public override string Texture => typeof(Homing_Rocket_II).GetDefaultTMLName();
		public override void PostAI() {
			Projectile.rotation += MathHelper.PiOver2;
		}
	}
	public class Homing_Rocket_II_Grenade() : Homing_Rocket_Version<Homing_Rocket_II>(ItemID.GrenadeLauncher, ProjectileID.GrenadeI) {}
	public class Homing_Rocket_II_Mine() : Homing_Rocket_Version<Homing_Rocket_II>(ItemID.ProximityMineLauncher, ProjectileID.ProximityMineI) {}
	public class Homing_Rocket_II_Snowman() : Homing_Rocket_Version<Homing_Rocket_II>(ItemID.SnowmanCannon, ProjectileID.RocketSnowmanI) {}
}
