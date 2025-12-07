using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Projectiles;
using Origins.Projectiles.Weapons;
using PegasusLib;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Empowerments;
using ThoriumMod.Projectiles.Minions;

namespace Origins.Items.Weapons.Demolitionist {
	public class Sharknade_O : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Grenade);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Item.maxStack = 1;
			Item.damage = 80;
			Item.value = 50000;
			Item.shootSpeed *= 1.5f;
			Item.shoot = ModContent.ProjectileType<Sharknade_O_P>();
			Item.ammo = ItemID.Grenade;
			Item.rare = ItemRarityID.Yellow;
			Item.consumable = false;
		}
	}
	public class Sharknade_O_P : ModProjectile {
		public override string Texture => typeof(Sharknade_O).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 135;
			Projectile.penetrate = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Grenade;
			return true;
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(
				Projectile,
				96,
				sound: SoundID.Item14,
				fireDustAmount: 0,
				smokeDustAmount: 0,
				smokeGoreAmount: 0
			);
			for (int i = 0; i < 40; i++) {
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Water,
					0f,
					0f,
					100,
					default,
					1.5f
				);
				dust.velocity *= 1.4f;
				dust.velocity += (dust.position - Projectile.Center).Normalized(out _) * 4;
			}

			Projectile.SpawnProjectile(
				Projectile.GetSource_Death(),
				Projectile.Center,
				Vector2.Zero,
				 ModContent.ProjectileType<Explosive_Sharknado>(),
				(Projectile.damage * 2) / 3,
				Projectile.knockBack
			);
		}
	}
	public class Explosive_Sharknado : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Tempest;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
			Main.projFrames[Type] = Main.projFrames[ProjectileID.Tempest];
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Projectile.netImportant = true;
			Projectile.width = 42;
			Projectile.height = 40;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
		}
		public override void AI() {
			if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
				Projectile.alpha += 20;
				if (Projectile.alpha > 150)
					Projectile.alpha = 150;
			} else {
				Projectile.alpha -= 50;
				if (Projectile.alpha < 60)
					Projectile.alpha = 60;
			}
			if (++Projectile.ai[0] > 20) {
				float distanceFromTarget = 2000f * 2000f;
				Vector2 targetCenter = Projectile.position;
				int target = -1;
				if (Main.player[Projectile.owner].OriginPlayer().GetMinionTarget(targetingAlgorithm)) {
					Projectile.ai[0] = 0;
					Vector2 dir = Projectile.Center.DirectionTo(targetCenter);
					Projectile.SpawnProjectile(null,
						Projectile.Center,
						dir * 8,
						ModContent.ProjectileType<Explosive_Sharkron>(),
						Projectile.damage,
						Projectile.knockBack
					);
				}
				void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
					if (!isPriorityTarget && distanceFromTarget > 700 * 700) {
						distanceFromTarget = 700 * 700;
					}
					if (!npc.CanBeChasedBy(Projectile)) return;
					float between = Vector2.DistanceSquared(npc.Center, Projectile.Center);
					if (between < distanceFromTarget) {
						distanceFromTarget = between;
						target = npc.whoAmI;
						targetCenter = npc.Center;
						foundTarget = true;
					}
				}
			}
			if (++Projectile.frameCounter > 3) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
		}
	}
	public class Explosive_Sharkron : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MiniSharkron;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
			Main.projFrames[Type] = Main.projFrames[ProjectileID.MiniSharkron];
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.MiniSharkron);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			AIType = ProjectileID.MiniSharkron;
		}

		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, sound: SoundID.Item62);
		}
	}
}
