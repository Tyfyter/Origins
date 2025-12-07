using Origins.Dev;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Eyenade : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"ThrownExplosive",
			"IsBomb",
			"ExpendableWeapon",
			"ToxicSource"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 48;
			Item.shootSpeed *= 1.25f;
			Item.value += Item.value / 3;
			Item.shoot = ModContent.ProjectileType<Eyenade_P>();
			Item.ammo = ItemID.Grenade;
			Item.rare = ItemRarityID.Blue;
			//Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 3)
			.AddIngredient(ItemID.Grenade, 3)
			.AddIngredient(ItemID.Lens)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Eyenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Eyenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 135;
			Projectile.penetrate = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}
		public override void AI() {
			float targetWeight = 160;
			Vector2 targetPos = default;
			bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
				Vector2 currentPos = target.Center;
				float dist = Math.Abs(Projectile.Center.X - currentPos.X) + Math.Abs(Projectile.Center.Y - currentPos.Y);
				if (target is Player) dist *= 2.5f;
				if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
					targetWeight = dist;
					targetPos = currentPos;
					return true;
				}
				return false;
			});

			if (foundTarget) {
				float scaleFactor = 16f * Origins.HomingEffectivenessMultiplier[Projectile.type];

				Vector2 targetVelocity = (targetPos - Projectile.Center).SafeNormalize(-Vector2.UnitY) * scaleFactor;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, 0.020833334f);
				Projectile.rotation = targetVelocity.ToRotation() - MathHelper.PiOver2;
			}
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 80, sound: SoundID.Item14);
		}
	}
}
