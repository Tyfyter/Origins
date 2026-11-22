using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Projectiles;
using PE = Origins.ProjectileExtensions;

namespace Origins.CrossMod.Thorium.Projectiles {
	[ExtendsFromMod("ThoriumMod")]
	public class TOThoriumGlobalProj : GlobalProjectile {
		public static HashSet<int> projectileTypes = [];
		public override void SetStaticDefaults() {
			try {
				Projectiles(this);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOThoriumGlobalProj), e)) throw;
			}
		}
		public override void SetDefaults(Projectile proj) {
			if (projectileTypes.Contains(proj.type)) proj.MakeExplosive();
		}
		[NoJIT]
		public static void Projectiles(GlobalProjectile self) {
			self.SetExplosiveData<JavelinPro>(16);
			self.SetExplosiveData<JavelinClusterPro>(homingMulti: 0.5f);
			self.SetExplosiveData<CannonBoom>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<SeethingChargePro>(48);
			self.SetExplosiveData<BuffaloLauncherPro>(48, 1);
			self.SetExplosiveData<BuffaloLauncherPro2>(16, 0, 0.3f, 0, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<BuffaloLauncherPro3>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<BuffaloLauncherClusterPro>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<LaunchJumperPro>(20);
			self.SetExplosiveData<LaunchJumperPro2>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<PhantomArmCannonPro>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<PhantomArmCannonPro3>(48, 0, 0.8f, 0, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<PhantomArmCannonPro4>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<PhantomArmCannonPro5>(48, 0, 1, 0, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<SleekRocketPro>(48, 1);
			self.SetExplosiveData<TheMassacrePro>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<TheMassacrePro2>(32);
			self.SetExplosiveData<TheMassacrePro3>(32);
			self.SetExplosiveData<TheMassacrePro4>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<DreadBomb1>(28);
			self.SetExplosiveData<IllumiteRocketPro>(48, 1, 1, 0, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<IllumiteRocketClusterPro>(24, 1, 1, 0, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<MicroRocketPro>(36, 1);
			self.SetExplosiveData<MicroRocketClusterPro>(priority: PE.TurretTargetPriority.none);
			self.SetExplosiveData<TerrariumBomberPro>(36, 1);
			self.SetExplosiveData<TorpedoPro>(28);
			self.SetExplosiveData<TorpedoPro2>(homingMulti: 0, priority: PE.TurretTargetPriority.none);
		}
	}
}
