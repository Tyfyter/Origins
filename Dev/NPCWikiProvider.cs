using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.Buffs;
using Origins.Journal;
using Origins.NPCs;
using Origins.NPCs.Defiled;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
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
			JArray types = new(WikiCategories.NPC);
			if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) types.Add(WikiCategories.Boss);
			if (modNPC is IJournalEntrySource) types.Add(WikiCategories.Lore);
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
			Dictionary<int, AssimilationAmount> assimilations = [];
			if (BiomeNPCGlobals.assimilationDisplayOverrides.TryGetValue(npc.type, out Dictionary<int, AssimilationAmount> _assimilations)) {
				foreach (KeyValuePair<int, AssimilationAmount> item in _assimilations) {
					assimilations.TryAdd(item.Key, item.Value);
				}
			}
			if (BiomeNPCGlobals.NPCAssimilationAmounts.TryGetValue(npc.type, out _assimilations)) {
				foreach (KeyValuePair<int, AssimilationAmount> item in _assimilations) {
					assimilations.TryAdd(item.Key, item.Value);
				}
			}
			foreach (KeyValuePair<int, AssimilationAmount> item in assimilations) {
				if (item.Value != default) {
					string assName = AssimilationLoader.Debuffs[item.Key].DisplayName.Value.Replace(" ", "");
					if (item.Value.Function is not null) {
						data.Add(assName, "variable");
						continue;
					}
					data.Add(assName, item.Value.ClassicAmount);
					if (item.Value.ExpertAmount.HasValue) expertData.Add(assName, item.Value.ExpertAmount.Value);
					if (item.Value.MasterAmount.HasValue) masterData.Add(assName, item.Value.MasterAmount.Value);
				}
			}
			data.AppendJStat("Expert", expertData, []);
			data.AppendJStat("Master", masterData, []);

			JArray environments = WikiExtensions.GetEnvironment(ContentSamples.NpcsByNetId[modNPC.Type]);
			data.AppendJStat("Environment", environments, []);
			string quote = WikiExtensions.GetBestiaryText(npc);
			data.AppendStat("BestiaryQuotation", quote, "");

			customStat?.ModifyWikiStats(data);
			if (!data.ContainsKey("SpriteWidth")) data.AppendStat("SpriteWidth", modNPC is null ? npc.width : ModContent.Request<Texture2D>(modNPC.Texture).Width(), 0);
			if (!data.ContainsKey("InternalName")) data.AppendStat("InternalName", modNPC?.Name, null);
			return data;
		}
		public override IEnumerable<(string, JObject)> GetStats(ModNPC modNPC) {
			NPC npc = modNPC.NPC;
			string segmentText = "";
			if (npc.catchItem > 0) {
				segmentText = "_NPC";
			} else if (modNPC is WormBody or Defiled_Digger_Body) {
				segmentText = "_Body";
			} else if (modNPC is WormTail or Defiled_Digger_Tail) {
				segmentText = "_Tail";
			}
			yield return (((modNPC as ICustomWikiStat)?.CustomStatPath ?? PageName(modNPC)) + segmentText, GetNPCStats(modNPC));
		}
		public override IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites(ModNPC modNPC) {
			if (modNPC is not IWikiNPC wikiNPC) yield break;
			string savePath = (modNPC as ICustomWikiStat)?.CustomSpritePath ?? WikiPageExporter.GetWikiName(modNPC);
			if (wikiNPC.ImageExportType == NPCExportType.Bestiary) {
				yield return (savePath, SpriteGenerator.GenerateAnimationSprite(modNPC.NPC, wikiNPC.DrawRect, wikiNPC.AnimationFrames, wikiNPC.FrameDuration)[wikiNPC.FrameRange]);
			} else if (wikiNPC.ImageExportType == NPCExportType.SpriteSheet) {
				Main.instance.LoadNPC(modNPC.Type);
				var texture = TextureAssets.Npc[modNPC.Type];
				(Rectangle frame, int frames)[] frames = new (Rectangle frame, int frames)[wikiNPC.AnimationFrames];
				for (int i = 0; i < frames.Length; i++) {
					Rectangle newFrame = wikiNPC.DrawRect with { Y = wikiNPC.DrawRect.Y + wikiNPC.DrawRect.Height * i };
					frames[i] = (newFrame, wikiNPC.FrameDuration);
				}
				yield return (savePath, SpriteGenerator.GenerateAnimationSprite(texture.Value, frames));
			}
		}
	}
}
