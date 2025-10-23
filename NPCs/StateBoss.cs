using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs {
	/// <typeparam name="TSelf">Should be the class which is extending this</typeparam>
	public interface IStateBoss<TSelf> where TSelf : ModNPC, IStateBoss<TSelf> {
		public static abstract AIList<TSelf> AIStates { get; }
		public abstract int[] PreviousStates { get; }
		public static int StateIndex<TState>() where TState : AIState<TSelf> => ModContent.GetInstance<TState>().Index;
	}
	public static class StateBossExtensions {
		public static void SetupStates<TBoss>(this TBoss self) where TBoss : ModNPC, IStateBoss<TBoss> => TBoss.AIStates.SetupStates();
		public static void SetupStates<TBoss>(this IEnumerable<AIState<TBoss>> states) where TBoss : ModNPC, IStateBoss<TBoss> {
			foreach (AIState<TBoss> state in states) state.SetStaticDefaults();
		}
		public static void SetAIState<TBoss>(this TBoss boss, int state) where TBoss : ModNPC, IStateBoss<TBoss> {
			NPC npc = boss.NPC;
			TBoss.AIStates[npc.aiAction].TrackState(boss.PreviousStates);
			npc.aiAction = state;
			npc.ai[0] = 0;
			TBoss.AIStates[npc.aiAction].StartAIState(boss);
			npc.netUpdate = true;
		}
		public static void SelectAIState<TBoss>(this TBoss boss, params IEnumerable<AIState<TBoss>>[] from) where TBoss : ModNPC, IStateBoss<TBoss> {
			WeightedRandom<int> states = new(Main.rand);
			for (int i = 0; i < from.Length; i++) {
				foreach (AIState<TBoss> state in from[i]) {
					states.Add(state.Index, state.GetWeight(boss, boss.PreviousStates));
				}
			}
			SetAIState(boss, states);
		}
	}
	public class AIList<TBoss> : List<AIState<TBoss>> where TBoss : ModNPC, IStateBoss<TBoss> { }
	public abstract class AIState<TBoss> : ILoadable where TBoss : ModNPC, IStateBoss<TBoss> {
		public int Index { get; private set; }
		public void Load(Mod mod) {
			Index = TBoss.AIStates.Count;
			TBoss.AIStates.Add(this);
			Load();
		}
		public virtual void Load() { }
		public virtual void SetStaticDefaults() { }
		public abstract void DoAIState(TBoss boss);
		public virtual void StartAIState(TBoss boss) { }
		public virtual double GetWeight(TBoss boss, int[] previousStates) {
			int index = Array.IndexOf(previousStates, Index);
			if (index == -1) index = previousStates.Length;
			float disincentivization = 1f;
			return (index / (float)previousStates.Length + (ContentExtensions.DifficultyDamageMultiplier - 0.5f) * 0.1f) * disincentivization;
		}
		public virtual void TrackState(int[] previousStates) => previousStates.Roll(Index);
		public virtual void Unload() { }
		public virtual bool Ranged => false;
		protected static float DifficultyMult => ContentExtensions.DifficultyDamageMultiplier;
		public static int StateIndex<TState>() where TState : AIState<TBoss> => ModContent.GetInstance<TState>().Index;
	}
	public abstract class AutomaticIdleState<TBoss> : AIState<TBoss> where TBoss : ModNPC, IStateBoss<TBoss> {
		public delegate int StatePriority(TBoss boss);
		public static List<(AIState<TBoss> state, StatePriority condition)> aiStates = [];
		public override void StartAIState(TBoss boss) {
			NPC npc = boss.NPC;
			npc.ai[0] = 0;
			npc.ai[1] = 0;
			npc.ai[2] = 0;
			npc.ai[3] = 0;
			int bestPriority = int.MinValue;
			for (int i = 0; i < aiStates.Count; i++) {
				(AIState<TBoss> state, StatePriority condition) = aiStates[i];
				int priority = condition(boss);
				if (priority > bestPriority) {
					npc.aiAction = state.Index;
					bestPriority = priority;
				}
			}
		}
		public override void DoAIState(TBoss boss) {
			StartAIState(boss);
		}
		public override void TrackState(int[] previousStates) { }
	}
}
