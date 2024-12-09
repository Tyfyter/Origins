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
	public class Sour_Broth : BrothBase {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.DrinkParticleColors[Type] = [
				new Color(197, 63, 5),
				new Color(197, 107, 5),
				new Color(197, 190, 5)
			];
			ItemID.Sets.FoodParticleColors[Type] = [
				new Color(78, 26, 2)
			];
		}
		public override void ModifyMinionHit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (Main.rand.NextBool(Origins.ArtifactMinion[proj.type] ? 25 : 10, 100)) modifiers.SetCrit();
		}
	}
}
