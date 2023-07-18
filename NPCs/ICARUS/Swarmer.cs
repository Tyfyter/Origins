using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Origins.NPCs.ICARUS {
	public class Swarmer : ModNPC {
		byte frame = 0;
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Swarmer");
			Main.npcFrameCount[NPC.type] = 3;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Bunny);
			NPC.aiStyle = 55;
			NPC.lifeMax = 80;
			NPC.defense = 3;
			NPC.damage = 15;
			NPC.width = 18;
			NPC.height = 18;
			NPC.friendly = false;
			NPC.FaceTarget();
			NPC.spriteDirection = NPC.direction;
		}

		public override void FindFrame(int frameHeight) {
			NPC.frame = new Rectangle(0, 15 * (frame & 4) / 2, 18, 18);
		}
	}
}
