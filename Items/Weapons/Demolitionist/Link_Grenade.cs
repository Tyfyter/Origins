using Origins.Dev;
using Origins.Projectiles;
using Origins.Tiles.Ashen;
using PegasusLib;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Link_Grenade : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "ThrownExplosive",
			"IsGrenade",
            "ExpendableWeapon"
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 55;
			Item.useTime = (int)(Item.useTime * 0.75);
			Item.useAnimation = (int)(Item.useAnimation * 0.75);
			Item.shoot = ModContent.ProjectileType<Link_Grenade_P>();
			Item.shootSpeed *= 1.25f;
			Item.ammo = ItemID.Grenade;
			Item.value = Item.sellPrice(copper: 35);
			Item.rare = ItemRarityID.Blue;
        }
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddIngredient(ItemID.Grenade, 8)
			.AddIngredient(ModContent.ItemType<Sanguinite_Ore_Item>())
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Link_Grenade_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Link_Grenade";
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.timeLeft = 60 * 20;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			if (Projectile.timeLeft <= 3) return;
			Vector2 center = Projectile.Center;
			for (int i = 0; i < ExplosiveGlobalProjectile.explodingProjectiles.Count; i++) {
				if (ExplosiveGlobalProjectile.explodingProjectiles[i].IsWithin(center, 16 * 12)) {
					Projectile.timeLeft = 3;
					break;
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.timeLeft == 0 && !Projectile.IsNPCIndexImmuneToProjectileType(Type, target.whoAmI)) return false;
			return null;
		}
		public static void AccumulateDamageFromKin(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			Vector2 center = projectile.Center;
			int n = 1;
			float defFactor = 1;
			Rectangle targetHitbox = target.Hitbox;
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.type == projectile.type && other.whoAmI != projectile.whoAmI && other.Center.IsWithin(center, 16 * 12) && other.Colliding(other.Hitbox, targetHitbox)) {
					float factor = 1 / MathF.Pow(++n, 0.5f);
					modifiers.SourceDamage.Base += other.damage * factor;
					defFactor += factor * factor;
				}
			}
			modifiers.DefenseEffectiveness *= defFactor;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			AccumulateDamageFromKin(Projectile, target, ref modifiers);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.perIDStaticNPCImmunity[Type][target.whoAmI] = Main.GameUpdateCount + 1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
		}
	}
}
