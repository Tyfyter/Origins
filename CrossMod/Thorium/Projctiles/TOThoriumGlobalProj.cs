using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Projectiles;

namespace Origins.CrossMod.Thorium.Projctiles {
	[ExtendsFromMod("ThoriumMod")]
	public class TOThoriumGlobalProj : GlobalProjectile {
		public override void SetDefaults(Projectile proj) {
			void MakeExplo() {
				if (proj.DamageType == DamageClass.Default) proj.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				else proj.DamageType = DamageClasses.ExplosiveVersion[proj.DamageType];
			}

			if (proj.IsType<JavelinPro>() || proj.IsType<JavelinClusterPro>()) MakeExplo();
			if (proj.IsType<SeethingChargePro>() || proj.IsType<CannonBoom>()) MakeExplo();
			if (proj.IsType<BuffaloLauncherPro>() || proj.IsType<BuffaloLauncherPro2>() || proj.IsType<BuffaloLauncherPro3>() || proj.IsType<BuffaloLauncherClusterPro>()) MakeExplo();
			if (proj.IsType<LaunchJumperPro>() || proj.IsType<LaunchJumperPro2>()) MakeExplo();
			if (proj.IsType<PhantomArmCannonPro>() || proj.IsType<PhantomArmCannonPro2>() || proj.IsType<PhantomArmCannonPro3>() || proj.IsType<PhantomArmCannonPro4>() || proj.IsType<PhantomArmCannonPro5>()) MakeExplo();
			if (proj.IsType<SleekRocketPro>()) MakeExplo();
			if (proj.IsType<TheMassacrePro>() || proj.IsType<TheMassacrePro2>() || proj.IsType<TheMassacrePro3>() || proj.IsType<TheMassacrePro4>()) MakeExplo();
			if (proj.IsType<DreadBomb1>()) MakeExplo();
			if (proj.IsType<IllumiteRocketPro>() || proj.IsType<IllumiteRocketClusterPro>()) MakeExplo();
			if (proj.IsType<MicroRocketPro>() || proj.IsType<MicroRocketClusterPro>()) MakeExplo();
			if (proj.IsType<TerrariumBomberPro>()) MakeExplo();
			if (proj.IsType<TorpedoPro>() || proj.IsType<TorpedoPro2>()) MakeExplo();
		}
	}
}
