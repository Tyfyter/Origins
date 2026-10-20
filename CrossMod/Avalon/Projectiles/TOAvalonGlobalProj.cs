using Avalon.Items.Weapons.Blah.Blahncher;
using Avalon.ModSupport.MLL.Projectiles;
using Avalon.Projectiles.Magic.Other;
using Avalon.Projectiles.Magic.Wands;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Origins.CrossMod.Avalon.Projectiles {
	[ExtendsFromMod("Avalon")]
	public class TOAvalonGlobalProj : GlobalProjectile {
		public static HashSet<int> projectileTypes = [];
		public override void SetStaticDefaults() {
			if (ModContent.TryFind("Avalon/BoomlashSeed", out ModProjectile proj)) {
				projectileTypes.Add(proj.Type);
			} else {
				MonoModHooks.Modify(typeof(BoomlashProj).GetMethod(nameof(ModProjectile.OnKill)), iL => {
					ILCursor c = new(iL);
					c.GotoNext(MoveType.After, inst => inst.MatchCall<DamageClass>("get_Magic"));
					c.EmitDelegate(MakeExplo);
				});
			}
		}
		public override void SetDefaults(Projectile proj) {
			try {
				AvalonProjectile(proj);
			} catch (Exception e) {
				if (Origins.LogLoadingError("MissingCrossModItem", nameof(TOAvalonGlobalProj), e)) throw;
			}
		}
		[NoJIT]
		public static void AvalonProjectile(Projectile proj) {
			if (proj.IsType<Blahcket>()) proj.MakeExplosive();
			if (proj.IsType<BoomlashProj>()) proj.MakeExplosive();
			if (proj.IsType<MagicGrenadeProj>() || proj.IsType<MagicGrenadeBoom>()) proj.MakeExplosive();
			if (proj.IsType<AcidBombProj>() || proj.IsType<BloodBombProj>()) proj.MakeExplosive(DamageClasses.ThrownExplosive);
			if (proj.IsType<AcidRocketProj>() || proj.IsType<AcidGrenadeProj>() || proj.IsType<AcidMineProj>() || proj.IsType<AcidSnowmanRocketProj>()) proj.MakeExplosive();
			if (proj.IsType<BloodRocketProj>() || proj.IsType<BloodGrenadeProj>() || proj.IsType<BloodMineProj>() || proj.IsType<BloodSnowmanRocketProj>()) proj.MakeExplosive();
			if (projectileTypes.Contains(proj.type)) proj.MakeExplosive();
		}
		public static DamageClass MakeExplo(DamageClass damageClass) => DamageClasses.ExplosiveVersion[damageClass];
	}
}
