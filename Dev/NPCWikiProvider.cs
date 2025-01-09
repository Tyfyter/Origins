using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.NPCs;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Origins.Dev.WikiPageExporter;

namespace Origins.Dev {
	public class NPCWikiProvider : WikiProvider<ModNPC> {
		public override string PageName(ModNPC modNPC) => WikiPageExporter.GetWikiName(modNPC);
		public override (PageTemplate template, Dictionary<string, object> context) GetPage(ModNPC modNPC) {
			NPC npc = modNPC.NPC;
			Dictionary<string, object> context = new() {
				["Name"] = WikiPageExporter.GetWikiName(modNPC),
				["DisplayName"] = Lang.GetNPCNameValue(npc.type)
			};

			IEnumerable<(string name, LocalizedText text)> pageTexts;
			if (modNPC is ICustomWikiStat wikiStats) {
				context["PageTextMain"] = wikiStats.PageTextMain.Value;
				pageTexts = wikiStats.PageTexts;
			} else {
				string key = $"WikiGenerator.{modNPC.Mod?.Name}.{modNPC.LocalizationCategory}.{context["Name"]}.MainText";
				context["PageTextMain"] = Language.GetOrRegister(key).Value;
				pageTexts = WikiPageExporter.GetDefaultPageTexts(modNPC);
			}
			foreach (var text in pageTexts) {
				context[text.name] = text.text;
			}
			return (WikiTemplate, context);
		}
		public static JObject GetNPCStats(ModNPC modNPC) {
			NPC npc = modNPC.NPC;
			JObject data = [];
			ICustomWikiStat customStat = modNPC as ICustomWikiStat;
			data["Image"] = customStat?.CustomSpritePath ?? (WikiPageExporter.GetWikiName(modNPC));
			data["Name"] = npc.TypeName;
			JArray types = new("NPC");
			if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) types.Add("Boss");
			if (customStat is not null) foreach (string cat in customStat.Categories) types.Add(cat);
			data.Add("Types", types);

			bool getGoodWorld = Main.getGoodWorld;
			Main.getGoodWorld = false;
			int gameMode = Main.GameMode;
			JObject expertData = [];
			JObject masterData = [];
			NPC tempInstance = new();
			bool dontDoHardmodeScaling = NPCID.Sets.DontDoHardmodeScaling[npc.type];
			try {
				NPCID.Sets.DontDoHardmodeScaling[npc.type] = true;
				Main.GameMode = GameModeID.Normal;
				tempInstance.SetDefaults(npc.netID, new NPCSpawnParams {
					gameModeData = Main.GameModeInfo,
					playerCountForMultiplayerDifficultyOverride = 1
				});
				data.AppendStat("MaxLife", tempInstance.lifeMax, 0);
				data.AppendStat("Defense", tempInstance.defense, 0);
				data.AppendStat("KBResist", 1 - tempInstance.knockBackResist, 0);
				data.AppendJStat("Immunities", tempInstance.GetImmunities(), []);
				data.AppendStat("Coins", tempInstance.value, 0);
				data.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));

				Main.GameMode = GameModeID.Expert;
				_ = Main.expertMode;
				tempInstance.SetDefaults(npc.netID, new NPCSpawnParams {
					gameModeData = Main.GameModeInfo,
					playerCountForMultiplayerDifficultyOverride = 1
				});
				expertData.AppendAltStat(data, "MaxLife", tempInstance.lifeMax);
				expertData.AppendAltStat(data, "Defense", tempInstance.defense);
				expertData.AppendAltStat(data, "KBResist", 1 - tempInstance.knockBackResist);
				expertData.AppendAltStat(data, "Immunities", tempInstance.GetImmunities());
				expertData.AppendAltStat(data, "Coins", tempInstance.value);
				expertData.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));

				Main.GameMode = GameModeID.Master;
				tempInstance.SetDefaults(npc.netID, new NPCSpawnParams {
					gameModeData = Main.GameModeInfo,
					playerCountForMultiplayerDifficultyOverride = 1
				});
				masterData.AppendAltStat(data, "MaxLife", tempInstance.lifeMax);
				masterData.AppendAltStat(data, "Defense", tempInstance.defense);
				masterData.AppendAltStat(data, "KBResist", 1 - tempInstance.knockBackResist);
				masterData.AppendAltStat(data, "Immunities", tempInstance.GetImmunities());
				masterData.AppendAltStat(data, "Coins", tempInstance.value);
				masterData.Add("Drops", new JArray().FillWithLoot(Main.ItemDropsDB.GetRulesForNPCID(npc.type, false).GetDropRates()));
			} finally {
				Main.GameMode = gameMode;
				Main.getGoodWorld = getGoodWorld;
				NPCID.Sets.DontDoHardmodeScaling[npc.type] = dontDoHardmodeScaling;
			}
			for (int i = 0; i < BiomeNPCGlobals.assimilationProviders.Count; i++) {
				if (npc.TryGetGlobalNPC((GlobalNPC)BiomeNPCGlobals.assimilationProviders[i], out GlobalNPC gNPC) && gNPC is IAssimilationProvider assimilationProvider) {
					AssimilationAmount amount = assimilationProvider.GetAssimilationAmount(npc);
					if (amount != default) {
						string assName = assimilationProvider.AssimilationName;
						if (amount.Function is not null) {
							data.Add(assName, "variable");
							continue;
						}
						data.Add(assName, amount.ClassicAmount);
						if (amount.ExpertAmount.HasValue) expertData.Add(assName, amount.ExpertAmount.Value);
						if (amount.MasterAmount.HasValue) masterData.Add(assName, amount.MasterAmount.Value);
					}
				}
			}
			data.AppendJStat("Expert", expertData, []);
			data.AppendJStat("Master", masterData, []);

			customStat?.ModifyWikiStats(data);
			if (!data.ContainsKey("SpriteWidth")) data.AppendStat("SpriteWidth", modNPC is null ? npc.width : ModContent.Request<Texture2D>(modNPC.Texture).Width(), 0);
			if (!data.ContainsKey("InternalName")) data.AppendStat("InternalName", modNPC?.Name, null);
			return data;
		}
		public override IEnumerable<(string, JObject)> GetStats(ModNPC modNPC) {
			NPC npc = modNPC.NPC;
			if (npc.catchItem > 0) yield break;
			string segmentText = "";
			if (modNPC is WormBody) {
				segmentText = "_Body";
			} else if (modNPC is WormTail) {
				segmentText = "_Tail";
			}
			yield return (((modNPC as ICustomWikiStat)?.CustomStatPath ?? PageName(modNPC)) + segmentText, GetNPCStats(modNPC));
		}
		public override IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites(ModNPC modNPC) {
			if (modNPC is not IWikiNPC wikiNPC) yield break;
			yield return (WikiPageExporter.GetWikiName(modNPC), SpriteGenerator.GenerateAnimationSprite(modNPC.NPC, wikiNPC.DrawRect, wikiNPC.AnimationFrames));
		}
	}
}
