using Microsoft.Xna.Framework;
using Origins.NPCs.Defiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Projectiles.Enemies {
	public class Defiled_Enemy_Summon : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Defiled/Infusion_P";
		public override void SetDefaults() {
			Projectile.aiStyle = 0;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = Main.rand.Next(4, 6);
			Projectile.hide = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNPC) {
				Projectile.ai[1] = parentNPC.whoAmI;
			}
		}
		public override void Kill(int timeLeft) {
			IEntitySource source = Projectile.GetSource_FromThis();
			NPC amalgam = Main.npc[(int)Projectile.ai[1]];
			if (amalgam.active && amalgam.type == ModContent.NPCType<Defiled_Amalgamation>()) {
				source = amalgam.GetSource_FromAI();
			}
			Point point = Projectile.position.ToPoint();
			NPC.NewNPC(source, point.X, point.Y, ModContent.NPCType<Defiled_Cyclops>());
		}
	}
}
