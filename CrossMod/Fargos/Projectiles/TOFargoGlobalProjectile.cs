using Fargowiltas.Projectiles.Explosives;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Items {
	[ExtendsFromMod(nameof(Fargowiltas))]
	public class TOFargoGlobalProjectile: GlobalProjectile {
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.type == ModContent.ProjectileType<ShurikenProj>();
		}
		public override void SetDefaults(Projectile entity) {
			entity.DamageType = DamageClasses.Explosive;
		}
	}
}
