using ModLiquidLib.Utils;
using Origins.Buffs;
using Origins.Items.Weapons.Melee;
using Origins.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine.Boss {
	public class Lost_Diver_Depth_Charge : Depth_Charge_P_Alt {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = false;
			Projectile.hostile = true;
		}
		public override Entity Owner => Main.npc[(int)Projectile.ai[2]];
		public override int ExplosionType => ModContent.ProjectileType<Lost_Diver_Depth_Charge_Explosion>();
		public override bool? CanHitNPC(NPC target) {
			if (Mildew_Creeper.FriendlyNPCTypes.Contains(target.type)) return false;
			return null;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += Brine_Pool_NPC.ScalingArmorPenetrationToCompensateForTSNerf;
		}
	}
	public class Lost_Diver_Depth_Charge_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.hide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProjectile && parentProjectile.usesLocalNPCImmunity) {
				for (int i = 0; i < parentProjectile.localNPCImmunity.Length; i++) {
					if (parentProjectile.localNPCImmunity[i] != 0) {
						Projectile.localNPCImmunity[i] = Projectile.localNPCHitCooldown;
					}
				}
			}
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
				Projectile.ai[0] = 1;
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Mildew_Creeper.FriendlyNPCTypes.Contains(target.type)) return false;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.GetWet(Liquids.Brine.ID)) target.AddBuff(Cavitation_Debuff.ID, 90);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += Brine_Pool_NPC.ScalingArmorPenetrationToCompensateForTSNerf;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (target.GetWet(Liquids.Brine.ID)) target.AddBuff(Cavitation_Debuff.ID, 90);
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
}
