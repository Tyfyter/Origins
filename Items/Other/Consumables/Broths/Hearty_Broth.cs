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
	public class Hearty_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(140, 0, 0),
				new Color(165, 61, 0),
				new Color(184, 129, 0)
			];
		}
		public override void PreUpdateMinion(Projectile minion) {
			minion.GetGlobalProjectile<MinionGlobalProjectile>().tempBonusUpdates += minion.MaxUpdates * 0.1f;
			if (minion.TryGetGlobalProjectile(out ArtifactMinionGlobalProjectile artifact)) artifact.maxHealthModifier += 0.2f;
		}
	}
}
