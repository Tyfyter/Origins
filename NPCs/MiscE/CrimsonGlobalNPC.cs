using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class CrimsonGlobalNPC : GlobalNPC, IAssimilationProvider {
		public string AssimilationName => "CrimsonAssimilation";
		public string AssimilationTexture => "Terraria/Images/UI/Bestiary/Icon_Tags_Shadow";
		public Rectangle AssimilationTextureFrame => new(30 * 12, 0, 30, 30);
		public static HashSet<int> NPCTypes { get; private set; } = [
			NPCID.BloodCrawler,
			NPCID.BloodCrawlerWall,
			NPCID.CrimsonBunny,
			NPCID.CrimsonGoldfish,
			NPCID.CrimsonPenguin,
			NPCID.FaceMonster,
			NPCID.Crimera,
			NPCID.BrainofCthulhu,
			NPCID.Creeper,

			NPCID.Herpling,
			NPCID.Crimslime,
			NPCID.BloodJelly,
			NPCID.BloodFeeder,

			NPCID.CrimsonAxe,
			NPCID.IchorSticker,
			NPCID.FloatyGross,
			NPCID.BigMimicCrimson,

			NPCID.BloodMummy,
			NPCID.DesertGhoulCrimson,

			NPCID.PigronCrimson,
		];
		public static Dictionary<int, AssimilationAmount> AssimilationAmounts { get; private set; } = new() {
			[NPCID.BloodCrawler] = 0.05f,
			[NPCID.BloodFeeder] = 0.06f,
			[NPCID.BloodJelly] = 0.08f,
			[NPCID.BrainofCthulhu] = 0.16f,
			[NPCID.Creeper] = 0.002f,
			[NPCID.Crimera] = 0.05f,
			[NPCID.Crimslime] = 0.06f,
			[NPCID.CrimsonGoldfish] = 0.05f,
			[NPCID.DesertGhoulCrimson] = 0.06f,
			[NPCID.FaceMonster] = 0.08f,
			[NPCID.FloatyGross] = 0.08f,
			[NPCID.Herpling] = 0.06f,
			[NPCID.IchorSticker] = 0.06f,
		};
		public override void Load() {
			BiomeNPCGlobals.assimilationProviders.Add(this);
		}
		public override void Unload() {
			NPCTypes = null;
			AssimilationAmounts = null;
		}
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) {
			return NPCTypes.Contains(entity.type);
		}
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers) {
			if (npc.ichor) {
				modifiers.Defense.Flat += 5;
			}
		}
		public override void ResetEffects(NPC npc) {
			int confusionIndex = npc.FindBuffIndex(BuffID.Confused);
			if (confusionIndex > -1 && Main.rand.NextBool()) {
				npc.buffTime[confusionIndex]--;
			}
		}
		public override void UpdateLifeRegen(NPC npc, ref int damage) {
			if (npc.poisoned) {
				npc.lifeRegen -= 5;
			}
			if (npc.venom) {
				npc.lifeRegen -= 5;
			}
            if (npc.ichor) {
                npc.lifeRegen += 25;
            }
            if (npc.onFire) {
				npc.lifeRegen -= 4;
				damage += 2;
			}
			if (npc.onFire3) {
				npc.lifeRegen -= 15;
				damage += 3;
			}
			if (npc.oiled && (npc.onFire || npc.onFire2 || npc.onFire3 || npc.shadowFlame)) {
				npc.lifeRegen -= 15;
			}
			if (npc.daybreak) {
				npc.lifeRegen -= 25 * 2;
			}
			if (npc.dryadBane) {
				const float baseDPS = 2;
				int totalDPS = (int)(baseDPS * BiomeNPCGlobals.CalcDryadDPSMult());
				npc.lifeRegen -= 2 * totalDPS;
				damage += totalDPS / 3;
			}
		}
		public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo) {
			AssimilationAmount amount = GetAssimilationAmount(npc);
			if (amount != default) {
				target.GetModPlayer<OriginPlayer>().CrimsonAssimilation += amount.GetValue(npc, target);
			}
		}
		public AssimilationAmount GetAssimilationAmount(NPC npc) {
			if (AssimilationAmounts.TryGetValue(npc.type, out AssimilationAmount amount)) {
				return amount;
			} else if (AssimilationAmounts.TryGetValue(0, out amount)) {
				return amount;
			}
			return default;
		}
	}
}
