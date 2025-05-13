using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class Shimmer_Construct : ModNPC {
		protected readonly static List<AIState> aiStates = [];
		public override string Texture => "Terraria/Images/NPC_" + NPCID.EyeofCthulhu;
		public readonly int[] previousStates = new int[6];
		public bool IsInPhase3 => Main.expertMode && NPC.life * 10 < NPC.lifeMax;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.ShimmerTransformToNPC[NPCID.EyeofCthulhu] = Type;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
			for (int i = 0; i < aiStates.Count; i++) aiStates[i].SetStaticDefaults();
		}
		public override void SetDefaults() {
			NPC.width = 100;
			NPC.height = 110;
			NPC.lifeMax = 6600;
			NPC.damage = 27;
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
			if (IsInPhase3) {
				Rectangle npcRect = NPC.Hitbox;
				const int active_range = 5000;
				Rectangle playerRect = new(0, 0, active_range * 2, active_range * 2);
				Vector2 min = NPC.TopLeft;
				Vector2 max = NPC.BottomRight;
				List<Player> players = [];
				foreach (Player player in Main.ActivePlayers) {
					if (npcRect.Intersects(playerRect.Recentered(player.Center))) {
						min = Vector2.Min(min, player.TopLeft);
						max = Vector2.Max(max, player.BottomRight);
						player.AddBuff(Weak_Shimmer_Debuff.ID, 5, true);
						players.Add(player);
					}
				}
				if (max.Y >= Main.bottomWorld - 640f - 64f) {
					Vector2 top = (min.Y - (640f + 16f)) * Vector2.UnitY;
					ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
						PositionInWorld = NPC.Bottom
					});
					NPC.Teleport(NPC.position - top, 12);
					ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
						PositionInWorld = NPC.Bottom
					});
					for (int i = 0; i < players.Count; i++) {
						ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
							PositionInWorld = players[i].Bottom
						});
						players[i].Teleport(players[i].position - top, 12);
						ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
							PositionInWorld = players[i].Bottom
						});
					}
				}
			}
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
		public override void OnKill() {
			Boss_Tracker.Instance.downedShimmerConstruct = true;
			NetMessage.SendData(MessageID.WorldData);
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
		public class AutomaticIdleState : AIState {
			public delegate int StatePriority(Shimmer_Construct boss);
			public static List<(AIState state, StatePriority condition)> aiStates = [];
			public override void StartAIState(Shimmer_Construct boss) {
				NPC npc = boss.NPC;
				npc.ai[0] = 0;
				npc.ai[1] = 0;
				npc.ai[2] = 0;
				npc.ai[3] = 0;
				int bestPriority = int.MinValue;
				for (int i = 0; i < aiStates.Count; i++) {
					(AIState state, StatePriority condition) = aiStates[i];
					int priority = condition(boss);
					if (priority > bestPriority) {
						npc.aiAction = state.Index;
						bestPriority = priority;
					}
				}
			}
			public override void DoAIState(Shimmer_Construct boss) {
				StartAIState(boss);
			}
			public override void TrackState(int[] previousStates) { }
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
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type is NPCID.EyeofCthulhu or NPCID.ServantofCthulhu;
		public override void AI(NPC npc) {
			Collision.WetCollision(npc.position, npc.width, npc.height);
			if (Collision.shimmer) {
				npc.buffImmune[BuffID.Shimmer] = false;
				npc.AddBuff(BuffID.Shimmer, 100, true); // Pass true to quiet as clients execute this as well.
			}
		}
	}
}
