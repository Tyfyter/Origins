using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Projectiles;
using static Terraria.ModLoader.ModContent;
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
			self.SetExplosiveData(0, 0, PE.TurretTargetPriority.low,
				ProjectileType<JavelinClusterPro>(),
				ProjectileType<CannonBoom>(),
				ProjectileType<BuffaloLauncherClusterPro>(),
				ProjectileType<BuffaloLauncherPro3>(),
				ProjectileType<LaunchJumperPro2>(),
				ProjectileType<PhantomArmCannonPro>(),
				ProjectileType<PhantomArmCannonPro4>(),
				ProjectileType<TheMassacrePro>(),
				ProjectileType<TheMassacrePro4>(),
				ProjectileType<MicroRocketClusterPro>(),
				ProjectileType<TorpedoPro2>()
			);
			self.SetExplosiveData<JavelinPro>(16);
			self.SetExplosiveData<SeethingChargePro>(48);
			self.SetExplosiveData<BuffaloLauncherPro>(48, 1);
			self.SetExplosiveData<BuffaloLauncherPro2>(16, 0, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<LaunchJumperPro>(20);
			self.SetExplosiveData<PhantomArmCannonPro3>(48, 0, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<PhantomArmCannonPro5>(48, 0, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<SleekRocketPro>(48, 1);
			self.SetExplosiveData<TheMassacrePro2>(32);
			self.SetExplosiveData<TheMassacrePro3>(32);
			self.SetExplosiveData<DreadBomb1>(28);
			self.SetExplosiveData<IllumiteRocketPro>(48, 1, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<IllumiteRocketClusterPro>(24, 1, PE.TurretTargetPriority.shiny);
			self.SetExplosiveData<MicroRocketPro>(36, 1);
			self.SetExplosiveData<TerrariumBomberPro>(36, 1);
			self.SetExplosiveData<TorpedoPro>(28);
		}
	}
}
