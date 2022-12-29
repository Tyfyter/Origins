using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace Origins.Questing {
	public class Blue_Bovine_Quest : Quest {
		public override bool HasDialogue(NPC npc) {
			if (npc.type != NPCID.Merchant) return false;
			return !Started;
		}
		public override string GetDialogue() {
			return "Moo";
		}
		public override void OnDialogue() {
			Main.npcChatText = "oh, you actually clicked on it?";
		}
	}
}
