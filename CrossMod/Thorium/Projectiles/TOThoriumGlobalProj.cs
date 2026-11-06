using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod.Projectiles;
using static Terraria.ModLoader.ModContent;

namespace Origins.CrossMod.Thorium.Projectiles {
	[ExtendsFromMod("ThoriumMod")]
	public class TOThoriumGlobalProj : GlobalProjectile {
		public static HashSet<int> projectileTypes = [];
		public override void SetStaticDefaults() {
			try {
				ThoriumProjectile();
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOThoriumGlobalProj), e)) throw;
			}
		}
		public override void SetDefaults(Projectile proj) {
			if (projectileTypes.Contains(proj.type)) proj.MakeExplosive();
		}
		[NoJIT]
		public static void ThoriumProjectile() {
			SetExplosiveData(0, 0,
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
			SetExplosiveData<JavelinPro>(16);
			SetExplosiveData<SeethingChargePro>(48);
			SetExplosiveData<BuffaloLauncherPro>(48, 1);
			SetExplosiveData<BuffaloLauncherPro2>(16);
			SetExplosiveData<LaunchJumperPro>(20);
			SetExplosiveData<PhantomArmCannonPro3>(48);
			SetExplosiveData<PhantomArmCannonPro5>(48);
			SetExplosiveData<SleekRocketPro>(48, 1);
			SetExplosiveData<TheMassacrePro2>(32);
			SetExplosiveData<TheMassacrePro3>(32);
			SetExplosiveData<DreadBomb1>(28);
			SetExplosiveData<IllumiteRocketPro>(48, 1);
			SetExplosiveData<IllumiteRocketClusterPro>(24, 1);
			SetExplosiveData<MicroRocketPro>(36, 1);
			SetExplosiveData<TerrariumBomberPro>(36, 1);
			SetExplosiveData<TorpedoPro>(28);
		}
		public static void SetExplosiveData(int type, int tripwireRange = 0, int tripwireStyle = 0) {
			projectileTypes.Add(type);
			if (tripwireRange > 0) ProjectileExtensions.SetupTripwireValues(type, tripwireRange, tripwireStyle);
		}
		public static void SetExplosiveData(int tripwireRange = 0, int tripwireStyle = 0, params int[] types) {
			for (int i = 0; i < types.Length; i++) {
				SetExplosiveData(types[i], tripwireRange, tripwireStyle);
			}
		}
		public static void SetExplosiveData<TExplosive>(int tripwireRange, int tripwireStyle = 0) where TExplosive : ModProjectile {
			SetExplosiveData(ProjectileType<TExplosive>(), tripwireRange, tripwireStyle);
		}
	}
}
