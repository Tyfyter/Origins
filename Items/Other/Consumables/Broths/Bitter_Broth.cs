using Origins.Buffs;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace Origins.Items.Other.Consumables.Broths {
	public class Bitter_Broth : BrothBase {
		public override LocalizedText BuffDescription => Language.GetText("Mods.Origins.Buffs.Bitter_Broth_Buff.Description");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				FromHexRGB(0x3D0071),
				FromHexRGB(0x2B253D),
				FromHexRGB(0x0C0C0C)
			];
		}
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			float multi = MinionGlobalProjectile.IsArtifact(proj) ? 5 : 3;
			if (Main.rand.NextBool(10)) target.AddBuff(Broken_Armor_Debuff.ID, (int)(60 * multi));
			target.AddBuff(Weak_Debuff.ID, (int)(60 * multi));
		}
	}
}