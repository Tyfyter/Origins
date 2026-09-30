using System;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Projectiles;

namespace Origins.CrossMod.Thorium.Projectiles {
	[ExtendsFromMod("ThoriumMod")]
	public class TOThoriumGlobalProj : GlobalProjectile {
		public override void SetDefaults(Projectile proj) {
			try {
				ThoriumProjectile(proj);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOThoriumGlobalProj), e)) throw;
			}
		}
		[NoJIT]
		public static void ThoriumProjectile(Projectile proj) {

			if (proj.IsType<JavelinPro>() || proj.IsType<JavelinClusterPro>()) proj.MakeExplosive();
			if (proj.IsType<SeethingChargePro>() || proj.IsType<CannonBoom>()) proj.MakeExplosive();
			if (proj.IsType<BuffaloLauncherPro>() || proj.IsType<BuffaloLauncherPro2>() || proj.IsType<BuffaloLauncherPro3>() || proj.IsType<BuffaloLauncherClusterPro>()) proj.MakeExplosive();
			if (proj.IsType<LaunchJumperPro>() || proj.IsType<LaunchJumperPro2>()) proj.MakeExplosive();
			if (proj.IsType<PhantomArmCannonPro>() || proj.IsType<PhantomArmCannonPro2>() || proj.IsType<PhantomArmCannonPro3>() || proj.IsType<PhantomArmCannonPro4>() || proj.IsType<PhantomArmCannonPro5>()) proj.MakeExplosive();
			if (proj.IsType<SleekRocketPro>()) proj.MakeExplosive();
			if (proj.IsType<TheMassacrePro>() || proj.IsType<TheMassacrePro2>() || proj.IsType<TheMassacrePro3>() || proj.IsType<TheMassacrePro4>()) proj.MakeExplosive();
			if (proj.IsType<DreadBomb1>()) proj.MakeExplosive();
			if (proj.IsType<IllumiteRocketPro>() || proj.IsType<IllumiteRocketClusterPro>()) proj.MakeExplosive();
			if (proj.IsType<MicroRocketPro>() || proj.IsType<MicroRocketClusterPro>()) proj.MakeExplosive();
			if (proj.IsType<TerrariumBomberPro>()) proj.MakeExplosive();
			if (proj.IsType<TorpedoPro>() || proj.IsType<TorpedoPro2>()) proj.MakeExplosive();
		}
	}
}
