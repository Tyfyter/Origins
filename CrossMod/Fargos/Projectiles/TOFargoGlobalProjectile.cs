using Fargowiltas.Projectiles.Explosives;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Projectiles {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class TOFargoGlobalProjectile : GlobalProjectile {
		public static HashSet<int> projectileTypes = [];
		public override void SetStaticDefaults() {
			try {
				FargosProjectile(this);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOFargoGlobalProjectile), e)) throw;
			}
		}
		public override void SetDefaults(Projectile proj) {
			if (projectileTypes.Contains(proj.type)) proj.MakeExplosive(DamageClasses.ThrownExplosive);
		}

		[NoJIT]
		public static void FargosProjectile(GlobalProjectile self) {
			self.SetExplosiveData(ModContent.ProjectileType<ShurikenProj>());
		}
	}
}
