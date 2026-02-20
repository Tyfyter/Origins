using Origins.Buffs;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Insubstantial_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				FromHexRGB(0x2A036B),
				FromHexRGB(0x5305D5),
				FromHexRGB(0xF5E3FF)
			];
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			bool artiMinion = MinionGlobalProjectile.IsArtifact(proj);
			if (Main.rand.NextBool(artiMinion ? 2 : 1, 10)) target.AddBuff(ModContent.BuffType<Lazy_Cloak_Buff>(), 20);
			if (artiMinion) target.AddBuff(BuffID.Confused, 180);
		}
	}
}