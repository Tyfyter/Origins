using Avalon.Items.Weapons.Blah.Blahncher;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.Thorium.Projctiles {
	[ExtendsFromMod("Avalon")]
	public class TOAvalonGlobalProj : GlobalProjectile {
		public override void SetDefaults(Projectile proj) {
			void MakeExplo() {
				if (proj.DamageType == DamageClass.Default) proj.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				else proj.DamageType = DamageClasses.ExplosiveVersion[proj.DamageType];
			}

			if (proj.IsType<Blahcket>()) MakeExplo();
		}
	}
}
