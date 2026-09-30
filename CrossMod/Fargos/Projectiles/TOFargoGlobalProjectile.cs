using Fargowiltas.Projectiles.Explosives;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Projectiles {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class TOFargoGlobalProjectile : GlobalProjectile {
		public override void SetDefaults(Projectile proj) {
			try {
				FargosProjectile(proj);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOFargoGlobalProjectile), e)) throw;
			}
		}

		[NoJIT]
		public static void FargosProjectile(Projectile proj) {
			if (proj.IsType<ShurikenProj>()) proj.MakeExplosive(DamageClasses.ThrownExplosive);
		}
	}
}
