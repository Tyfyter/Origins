using Origins.Dusts;
using Origins.NPCs.Defiled;
using Origins.Projectiles.Weapons;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Items {
	public class Defiled_Prefix : OriginsPrefix, IOnHitNPCPrefix, IProjectileAIPrefix {
		public override bool HasDescription => true;
		public override PrefixCategory Category => PrefixCategory.Magic;
		public override void SetStaticDefaults() {
			Origins.SpecialPrefix[Type] = true;
		}
		public override bool CanRoll(Item item) => !item.CountsAsClass(DamageClass.Summon) && !OriginsSets.Items.InvalidForDefiledPrefix[item.type];
		public override float RollChance(Item item) {
			if (Main.LocalPlayer.InModBiome<Defiled_Wastelands>()) return 1;
			if (OriginSystem.Instance.hasDefiled) return 0.5f;
			return 0;
		}
		public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
			damageMult *= 1.2f;
			useTimeMult *= 1.1f;
			manaMult *= 1.1f;
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (OriginsSets.NPCs.TargetDummies[target.type]) return;
			if (projectile.ModProjectile is Defiled_Spike_Explosion or Defiled_Spike_Explosion_Spike) return;
			ref int timer = ref projectile.GetEffectTimer<Defiled_Prefix_Mana_Steal_Timer>();
			if (Main.rand.Next(120) < timer) {
				float mana = 5 + MathF.Pow(damageDone * 0.1f, 0.75f);
				if (target.ModNPC is IDefiledEnemy defiledEnemy) {
					mana = Math.Min(mana, defiledEnemy.Mana);
				}
				if (mana <= 0) return;
				timer = 0;
				Projectile.NewProjectile(
					projectile.GetSource_OnHit(target),
					target.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Defiled_Prefix_Orb>(),
					0,
					0,
					ai0: mana
				);
			}
		}
		public void ProjectileAI(Projectile projectile) {
			if (projectile.owner != Main.myPlayer || !projectile.friendly) return;
			int projType = ModContent.ProjectileType<Defiled_Spike_Explosion>();
			if (projectile.type == projType) return;
			if (projectile.type == ModContent.ProjectileType<Defiled_Spike_Explosion_Spike>()) return;
			ref int timer = ref projectile.GetEffectTimer<Defiled_Prefix_Spikes_Timer>();
			if (timer == int.MaxValue) timer = 0;
			if (Main.rand.Next(30, 120) < timer) {
				timer = 0;
				Projectile.NewProjectile(
					projectile.GetSource_FromAI(),
					projectile.Center,
					Vector2.Zero,
					projType,
					projectile.damage,
					projectile.knockBack,
					ai0: 7,
					ai2: 0.75f
				);
			}
		}
		public override void ModifyValue(ref float valueMult) {
			valueMult *= 1.25f;
		}
	}
	public class Defiled_Prefix_Mana_Steal_Timer : PrefixProjectileEffectTimer<Defiled_Prefix> {
		public override bool AppliesToEntity(Projectile projectile) => projectile.friendly && base.AppliesToEntity(projectile);
	}
	public class Defiled_Prefix_Spikes_Timer : PrefixProjectileEffectTimer<Defiled_Prefix> {
		public override bool AppliesToEntity(Projectile projectile) => projectile.friendly && projectile.ModProjectile is not Defiled_Spike_Explosion or Defiled_Spike_Explosion_Spike && base.AppliesToEntity(projectile);
	}
	public class Defiled_Prefix_Orb : ModProjectile {
		public override string Texture => "Terraria/Images/NPC_0";
		public override void SetDefaults() {
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.alpha = 255;
			Projectile.tileCollide = false;
			Projectile.extraUpdates = 10;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) {
				Projectile.Kill();
				return;
			}
			Vector2 direction = player.Center - Projectile.Center;
			float dist = direction.LengthSquared();
			if (dist < 50f && Projectile.Hitbox.Intersects(player.Hitbox)) {
				if (Projectile.owner == Main.myPlayer) {
					int mana = Math.Min(Main.rand.RandomRound(Projectile.ai[0]), player.statLifeMax2 - player.statMana);
					player.statMana += mana;
					CombatText.NewText(player.Hitbox, new(110, 110, 192), mana);
				}
				Projectile.Kill();
			} else {
				direction *= 4 / MathF.Sqrt(dist);
				Projectile.velocity = (Projectile.velocity * 15f + direction) / 16f;
				for (int i = 0; i < 3; i++) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Solution_D>(), 0f, 0f, 20, new(110, 110, 192), 1.1f);
					dust.noGravity = true;
					dust.velocity *= 0f;
					dust.position.X -= Projectile.velocity.X * 0.334f * i;
					dust.position.Y -= Projectile.velocity.Y * -0.334f * i;
				}
			}
		}
	}
}
