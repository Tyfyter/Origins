using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles {
	//separate global for organization, might also make non-artifact projectiles less laggy than the alternative
	public class HarpoonGlobalProjectile : GlobalProjectile {
		bool isRetracting = false;
		public override bool InstancePerEntity => true;
		protected override bool CloneNewInstances => false;
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.aiStyle == 13;
		}
		public override void SetDefaults(Projectile projectile) {
			isRetracting = false;
		}
		public override void AI(Projectile projectile) {
			if (projectile.ai[0] == 1) {
				if (!isRetracting) {
					if (projectile.aiStyle == 13 && Main.player[projectile.owner].GetModPlayer<OriginPlayer>().turboReel) {
						projectile.extraUpdates++;
					}
					isRetracting = true;
				}
			}
		}
	}
}
