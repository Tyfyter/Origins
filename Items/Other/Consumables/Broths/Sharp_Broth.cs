using Origins.Buffs;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables.Broths {
	public class Sharp_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new(233, 173, 117),
				new(207, 110, 21),
				new(151, 82, 18)
			];
		}
		public override int Duration => 4;
		public override void OnMinionHit(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			if (MinionGlobalProjectile.IsArtifact(proj)) {
				target.AddBuff(Electrified_Debuff.ID, Main.rand.Next(180, 241));
			} else {
				Static_Shock_Debuff.Inflict(target, Main.rand.Next(180, 241));
			}
		}
	}
}
