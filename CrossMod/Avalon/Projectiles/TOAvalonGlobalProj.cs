using Avalon.Items.Weapons.Blah.Blahncher;
using Avalon.ModSupport.MLL.Projectiles;
using Avalon.Projectiles.Magic.Other;
using Avalon.Projectiles.Magic.Wands;
using MonoMod.Cil;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using PE = Origins.ProjectileExtensions;

namespace Origins.CrossMod.Avalon.Projectiles {
	[ExtendsFromMod("Avalon")]
	public class TOAvalonGlobalProj : GlobalProjectile {
		public static Dictionary<int, DamageClass> projectileTypes = [];
		public override void SetStaticDefaults() {
			if (ModContent.TryFind("Avalon/BoomlashSeed", out ModProjectile proj)) {
				this.SetExplosiveData(proj.Type, homingMulti: 0, priority: PE.TurretTargetPriority.none);
			} else {
				MonoModHooks.Modify(typeof(BoomlashProj).GetMethod(nameof(ModProjectile.OnKill)), iL => {
					ILCursor c = new(iL);
					c.GotoNext(MoveType.After, inst => inst.MatchCall<DamageClass>("get_Magic"));
					c.EmitDelegate(SetExplosive);
				});
			}
			Projectiles(this);
		}
		public override void SetDefaults(Projectile proj) {
			if (projectileTypes.ContainsKey(proj.type)) proj.MakeExplosive(projectileTypes[proj.type]);
		}
		[NoJIT]
		public static void Projectiles(GlobalProjectile self) {
			self.SetExplosiveData<Blahcket>();
			self.SetExplosiveData<BoomlashProj>();
			self.SetExplosiveData<MagicGrenadeProj>();
			self.SetExplosiveData<MagicGrenadeBoom>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<AcidBombProj>(DamageClasses.ThrownExplosive);
			self.SetExplosiveData<BloodBombProj>(DamageClasses.ThrownExplosive);
			self.SetExplosiveData<AcidGrenadeProj>();
			self.SetExplosiveData<AcidMineProj>();
			self.SetExplosiveData<AcidRocketProj>();
			self.SetExplosiveData<AcidSnowmanRocketProj>();
			self.SetExplosiveData<BloodGrenadeProj>();
			self.SetExplosiveData<BloodMineProj>();
			self.SetExplosiveData<BloodRocketProj>();
			self.SetExplosiveData<BloodSnowmanRocketProj>();
		}
		public static DamageClass SetExplosive(DamageClass damageClass) => DamageClasses.ExplosiveVersion[damageClass];
	}
}
