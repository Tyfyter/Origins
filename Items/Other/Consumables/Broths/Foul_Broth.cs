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
	public class Foul_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(9, 141, 59),
				new Color(74, 137, 100),
				new Color(187, 154, 23)
			];
		}
		public override void ModifyMinionHit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 2f;
		}
		public override void PreUpdateMinion(Projectile minion) {
			minion.GetGlobalProjectile<MinionGlobalProjectile>().tempBonusUpdates -= 0.5f;
		}
	}
}
