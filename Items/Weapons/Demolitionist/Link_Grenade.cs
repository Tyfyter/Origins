using Origins.Projectiles;
using Origins.Tiles.Ashen;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Link_Grenade : ModItem {
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
		public override string Texture => typeof(Link_Grenade).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
			Hand_Grenade_Launcher.AltFireAction[Type] = LauncherAltFire;
		}
		public static void LauncherAltFire(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Link_Trigger_P>(), damage * 2, knockback, player.whoAmI);
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
				if (other.ModProjectile is Link_Grenade_P && other.whoAmI != projectile.whoAmI && other.Center.IsWithin(center, 16 * 12) && other.Colliding(other.Hitbox, targetHitbox)) {
					float factor = 1 / MathF.Pow(++n, 0.25f);
					modifiers.SourceDamage.Base += other.damage * factor;
					defFactor += factor * factor * factor;
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
		public class Link_Trigger_P : Link_Grenade_P, IIsExplodingProjectile {
			public override string Texture => typeof(Link_Grenade).GetDefaultTMLName();
			public bool IsExploding => Projectile.ai[0] != 0;
			public override void SetStaticDefaults() {
				Origins.MagicTripwireRange[Type] = 32;
			}
			public override void SetDefaults() {
				base.SetDefaults();
				Projectile.aiStyle = 0;
				Projectile.extraUpdates = 1;
			}
			public override void AI() {
				if (Projectile.ai[0] != 0) {
					Projectile.Kill();
					return;
				}
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch);
				Rectangle hitbox = Projectile.Hitbox;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (!npc.friendly && hitbox.Intersects(npc.Hitbox)) {
						Projectile.ai[0] = 1;
						Projectile.netUpdate = true;
						ProjectileLoader.ModifyDamageHitbox(Projectile, ref hitbox);
						break;
					}
				}
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				Projectile.ai[0] = 1;
				Projectile.netUpdate = true;
				Rectangle hitbox = Projectile.Hitbox;
				ProjectileLoader.ModifyDamageHitbox(Projectile, ref hitbox);
				return false;
			}
			public override bool? CanDamage() => false;
			public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) { }
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }
			public override bool PreDraw(ref Color lightColor) => false;
			public override void OnKill(int timeLeft) { }
		}
	}
}
