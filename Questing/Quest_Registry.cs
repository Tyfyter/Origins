using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Origins.Questing {
	public class Quest_Registry : ILoadable {
		public static Dictionary<string, Quest> Quests { get; internal set; }

		public void Load(Mod mod) {
			if (Quests is null) Quests = new Dictionary<string, Quest>();
		}

		public void Unload() {
			Quests = null;
		}
	}
}
