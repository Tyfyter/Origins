using CalamityMod.NPCs.TownNPCs;
using Origins.Graphics;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Magic;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class Shimmer_Construct : ModNPC {
		protected readonly static List<AIState> aiStates = [];
		public override string Texture => "Terraria/Images/NPC_" + NPCID.EyeofCthulhu;
		public readonly int[] previousStates = new int[6];
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.ShimmerTransformToNPC[NPCID.EyeofCthulhu] = Type;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
			for (int i = 0; i < aiStates.Count; i++) aiStates[i].SetStaticDefaults();
		}
		public override void SetDefaults() {
			NPC.width = 100;
			NPC.height = 110;
			NPC.lifeMax = 100;
			NPC.damage = 1;
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.aiAction = StateIndex<PhaseOneIdleState>();
			Array.Fill(previousStates, NPC.aiAction);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}
		public override void AI() {
			if (NPC.shimmerTransparency > 0) {
				NPC.shimmerTransparency -= 0.005f;
				if (NPC.shimmerTransparency < 0) NPC.shimmerTransparency = 0;
			}
			aiStates[NPC.aiAction].DoAIState(this);
		}
		public override void BossLoot(ref string name, ref int potionType) {
			potionType = ItemID.RestorationPotion;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)NPC.aiAction);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadByte();
		}
		public static void SetAIState(Shimmer_Construct boss, int state) {
			NPC npc = boss.NPC;
			aiStates[npc.aiAction].TrackState(boss.previousStates);
			npc.aiAction = state;
			npc.ai[0] = 0;
			aiStates[npc.aiAction].StartAIState(boss);
			npc.netUpdate = true;
		}
		public static int StateIndex<TState>() where TState : AIState => ModContent.GetInstance<TState>().Index;
		public static void SelectAIState(Shimmer_Construct boss, params List<AIState>[] from) {
			WeightedRandom<int> states = new(Main.rand);
			for (int j = 0; j < from.Length; j++) {
				for (int i = 0; i < from[j].Count; i++) {
					states.Add(from[j][i].Index, from[j][i].GetWeight(boss, boss.previousStates));
				}
			}
			SetAIState(boss, states);
		}
		public abstract class AIState : ILoadable {
			public int Index { get; private set; }
			public void Load(Mod mod) {
				Index = aiStates.Count;
				aiStates.Add(this);
				Load();
			}
			public virtual void Load() { }
			public virtual void SetStaticDefaults() { }
			public abstract void DoAIState(Shimmer_Construct boss);
			public virtual void StartAIState(Shimmer_Construct boss) { }
			public virtual double GetWeight(Shimmer_Construct boss, int[] previousStates) {
				int index = Array.IndexOf(previousStates, Index);
				if (index == -1) index = previousStates.Length;
				return index / (float)previousStates.Length + (ContentExtensions.DifficultyDamageMultiplier - 0.5f) * 0.1f;
			}
			public virtual void TrackState(int[] previousStates) => previousStates.Roll(Index);
			public void Unload() { }
		}
	}
	class Eye_Shimmer_Collision : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.EyeofCthulhu;
		public override void AI(NPC npc) {
			Collision.WetCollision(npc.position, npc.width, npc.height);
			if (Collision.shimmer) {
				npc.AddBuff(BuffID.Shimmer, 100, true); // Pass true to quiet as clients execute this as well.
			}
		}
	}
}
