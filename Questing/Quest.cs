using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Questing {
	public abstract class Quest : ModType {
		public virtual bool Started => false;
		public virtual bool Completed => false;
		public virtual bool HasDialogue(NPC npc) {
			return false;
		}
		public virtual string GetDialogue() {
			return "";
		}
		public virtual void OnDialogue() {

		}
		protected sealed override void Register() {
			ModTypeLookup<Quest>.Register(this);
			if (Quest_Registry.Quests is null) Quest_Registry.Quests = new Dictionary<string, Quest>();
			Quest_Registry.Quests.Add(FullName, this);
		}
	}
}
