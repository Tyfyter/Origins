using BetterDialogue.UI;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Origins.UI {
	public static class ForcedDialectCompatibility {
		internal static bool[] forcedCompatibleNPCs;
		public static void PostSetupContent() {
			forcedCompatibleNPCs = NPCID.Sets.Factory.CreateBoolSet();
			HashSet<int> forcedIncompatible = (OriginClientConfig.Instance.npcsNotToForceDialectOn ?? []).Where(def => !def.IsUnloaded).Select(def => def.Type).ToHashSet();
			for (int i = NPCID.Count; i < NPCLoader.NPCCount; i++) {
				if (forcedIncompatible.Contains(i)) continue;
				NPC npc = ContentSamples.NpcsByNetId[i];
				try {
					if ((NPCLoader.CanChat(npc) ?? false) && !NPCID.Sets.IsTownPet[i] && !BetterDialogue.BetterDialogue.SupportedNPCs.Contains(i)) {
						BetterDialogue.BetterDialogue.SupportedNPCs.Add(i);
						forcedCompatibleNPCs[i] = true;
					}
				} catch (Exception e) {
					Origins.instance.Logger.Error(e);
				}
			}
		}

	}
	public class ModLoaderChatButton1 : ChatButton {
		public override double Priority => 2.0;
		public override void OnClick(NPC npc, Player player) {
			if (!NPCLoader.PreChatButtonClicked(firstButton: true)) {
				return;
			}
			NPCLoader.OnChatButtonClicked(firstButton: true);
		}
		public override bool IsActive(NPC npc, Player player) => ForcedDialectCompatibility.forcedCompatibleNPCs[npc.type];
		public override string Text(NPC npc, Player player) {
			string focusText = Lang.inter[28].Value;
			string _ = default;
			NPCLoader.SetChatButtons(ref focusText, ref _);
			return focusText;
		}
	}
	public class ModLoaderChatButton2 : ChatButton {
		public override double Priority => 99.99;
		public override void OnClick(NPC npc, Player player) {
			if (!NPCLoader.PreChatButtonClicked(firstButton: false)) {
				return;
			}
			NPCLoader.OnChatButtonClicked(firstButton: false);
		}
		public override bool IsActive(NPC npc, Player player) {
			if (!ForcedDialectCompatibility.forcedCompatibleNPCs[npc.type]) return false;
			string focusText = "";
			string _ = default;
			NPCLoader.SetChatButtons(ref _, ref focusText);
			return !string.IsNullOrEmpty(focusText);
		}
		public override string Text(NPC npc, Player player) {
			string focusText = "";
			string _ = default;
			NPCLoader.SetChatButtons(ref _, ref focusText);
			return focusText;
		}
	}
}
