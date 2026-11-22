using Fargowiltas.Projectiles.Explosives;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Projectiles {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class TOFargoGlobalProjectile : GlobalProjectile {
		public static Dictionary<int, DamageClass> projectileTypes = [];
		public override void SetStaticDefaults() {
			try {
				FargosProjectile(this);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOFargoGlobalProjectile), e)) throw;
			}
		}
		public override void SetDefaults(Projectile proj) {
			if (projectileTypes.ContainsKey(proj.type)) proj.MakeExplosive(projectileTypes[proj.type]);
		}

		[NoJIT]
		public static void FargosProjectile(GlobalProjectile self) {
			self.SetExplosiveData<ShurikenProj>(DamageClasses.ThrownExplosive, homingMulti: 0, priority: ProjectileExtensions.TurretTargetPriority.none);
		}
	}
}
