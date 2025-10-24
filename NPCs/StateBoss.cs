using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Items.Other.Testing;
using Origins.Items.Tools.Wiring;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.Utilities;

namespace Origins.NPCs {
	/// <typeparam name="TSelf">Should be the class which is extending this</typeparam>
	public interface IStateBoss<TSelf> where TSelf : ModNPC, IStateBoss<TSelf> {
		public static abstract AIList<TSelf> AIStates { get; }
		public abstract int[] PreviousStates { get; }
		public static virtual AutomaticIdleState<TSelf> AutomaticIdleState { get; set; }
	}
	public static class StateBossExtensions {
		public static void SetupStates<TBoss>(this TBoss self) where TBoss : ModNPC, IStateBoss<TBoss> => TBoss.AIStates.SetupStates();
		public static void SetupStates<TBoss>(this IEnumerable<AIState<TBoss>> states) where TBoss : ModNPC, IStateBoss<TBoss> {
			foreach (AIState<TBoss> state in states) state.SetStaticDefaults();
		}
		public static void StartIdle<TBoss>(this TBoss boss) where TBoss : ModNPC, IStateBoss<TBoss> {
			boss.SetAIState(TBoss.AutomaticIdleState.Index);
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
			bool canBeDisabled = Boss_Controller_Item<TBoss>.disabled is not null;
			if (canBeDisabled) Boss_Controller_Item<TBoss>.activeInLastRoll.Clear();
			for (int i = 0; i < from.Length; i++) {
				foreach (AIState<TBoss> state in from[i]) {
					if (canBeDisabled) {
						Boss_Controller_Item<TBoss>.activeInLastRoll.Add(state);
						if (Boss_Controller_Item<TBoss>.disabled.Contains(state)) continue;
					}
					states.Add(state.Index, state.GetWeight(boss, boss.PreviousStates));
				}
			}
			SetAIState(boss, states);
		}
		public static AIState<TBoss> GetState<TBoss>(this TBoss boss) where TBoss : ModNPC, IStateBoss<TBoss> => TBoss.AIStates[boss.NPC.aiAction];
		public static void AddBossControllerItem<TBoss>(this TBoss boss, int vanillaItemTexture) where TBoss : ModNPC, IStateBoss<TBoss> {
			boss.AddBossControllerItem(() => $"Terraria/Images/Item_{vanillaItemTexture}");
		}
		public static void AddBossControllerItem<TBoss>(this TBoss boss, Type type) where TBoss : ModNPC, IStateBoss<TBoss> {
			Func<ModItem> itemGetter = typeof(ModContent).GetMethod("GetInstance").MakeGenericMethod(type).CreateDelegate<Func<ModItem>>();
			boss.AddBossControllerItem(() => itemGetter().Texture);
		}
		public static void AddBossControllerItem<TBoss>(this TBoss boss, string texture = null) where TBoss : ModNPC, IStateBoss<TBoss> {
			boss.AddBossControllerItem(() => texture ?? boss.BossHeadTexture);
		}
		public static void AddBossControllerItem<TBoss>(this TBoss boss, Func<string> texture) where TBoss : ModNPC, IStateBoss<TBoss> {
			boss.Mod.AddContent((ModItem)typeof(Boss_Controller_Item<>).MakeGenericType(typeof(TBoss)).GetConstructors()[0].Invoke([boss.Name, texture]));
		}
	}
	public static class StateBossMethods<TBoss> where TBoss : ModNPC, IStateBoss<TBoss> {
		public static int StateIndex<TState>() where TState : AIState<TBoss> => ModContent.GetInstance<TState>().Index;
	}
	public class AIList<TBoss> : List<AIState<TBoss>> where TBoss : ModNPC, IStateBoss<TBoss> { }
	public abstract class AIState<TBoss> : ILoadable, IFlowerMenuItem<BossControllerPetalData> where TBoss : ModNPC, IStateBoss<TBoss> {
		/// <summary>
		/// Used by boss controller debugging items
		/// </summary>
		public AutoLoadingAsset<Texture2D> iconTexture;
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

		static readonly AutoLoadingAsset<Texture2D>[] textures = [
			"Origins/NPCs/Boss_Controller_Active_0",
			"Origins/NPCs/Boss_Controller_Active_1",
			"Origins/NPCs/Boss_Controller_Inactive_0",
			"Origins/NPCs/Boss_Controller_Inactive_1",
			"Origins/NPCs/Boss_Controller_Idle_0",
			"Origins/NPCs/Boss_Controller_Idle_1"
		];
		void IFlowerMenuItem<BossControllerPetalData>.Draw(Vector2 position, bool hovered, BossControllerPetalData data) {
			Texture2D texture = TextureAssets.WireUi[hovered.ToInt() + data.HasFlag(BossControllerPetalData.Disabled).ToInt() * 8].Value;
			if (data.HasFlag(BossControllerPetalData.Active)) {
				texture = textures[hovered.ToInt()];
			} else if (data.HasFlag(BossControllerPetalData.CurrentIdle)) {
				texture = textures[hovered.ToInt() + 4];
			} else if (data.HasFlag(BossControllerPetalData.Inactive) && !data.HasFlag(BossControllerPetalData.Disabled)) {
				texture = textures[hovered.ToInt() + 2];
			}
			DrawIcon(texture, position, Color.White);
			if (iconTexture.Exists) DrawIcon(iconTexture, position, Color.White);
			if (hovered) {
				UICommon.TooltipMouseText(GetType().Name);
			}
			static void DrawIcon(Texture2D texture, Vector2 position, Color tint) {
				Main.spriteBatch.Draw(
					texture,
					position,
					null,
					tint,
					0f,
					texture.Size() * 0.5f,
					1,
					SpriteEffects.None,
				0f);
			}
		}
		bool IFlowerMenuItem<BossControllerPetalData>.IsHovered(Vector2 position) => Main.MouseScreen.WithinRange(position, 20);
	}
	public abstract class AutomaticIdleState<TBoss> : AIState<TBoss> where TBoss : ModNPC, IStateBoss<TBoss> {
		public delegate int StatePriority(TBoss boss);
		public static List<(AIState<TBoss> state, StatePriority condition)> aiStates = [];
		public override void Load() {
			TBoss.AutomaticIdleState = this;
		}
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
		public static bool IsIdleStateActive(TBoss boss, AIState<TBoss> idleState) {
			NPC npc = boss.NPC;
			int bestPriority = int.MinValue;
			int bestState = int.MinValue;
			for (int i = 0; i < aiStates.Count; i++) {
				(AIState<TBoss> state, StatePriority condition) = aiStates[i];
				int priority = condition(boss);
				if (priority > bestPriority) {
					bestState = state.Index;
					bestPriority = priority;
				}
			}
			return bestState == idleState.Index;
		}
	}
	public class Boss_Controller_Item<TBoss>(string name, Func<string> texture) : TestingItem where TBoss : ModNPC, IStateBoss<TBoss> {
		public static HashSet<AIState<TBoss>> disabled;
		public static HashSet<AIState<TBoss>> activeInLastRoll;
		public override string Name => $"{name}_Controller";
		public override string Texture => texture();
		protected override bool CloneNewInstances => true;
		public override void Load() {
			Mod.AddContent(new Boss_Controller_Kite(this));
			disabled = [];
			activeInLastRoll = [];
		}

		public class Boss_Controller_Kite(Boss_Controller_Item<TBoss> controller) : ItemModeFlowerMenu<AIState<TBoss>, BossControllerPetalData> {
			public override bool IsActive() => Main.LocalPlayer.HeldItem.type == controller.Type;
			AutoLoadingAsset<Texture2D> wireMiniIcons = "Origins/Items/Tools/Wiring/Mini_Wire_Icons";
			public override float DrawCenter() {
				NPC boss = null;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.ModNPC is TBoss) {
						boss = npc;
						break;
					}
				}
				if (boss is not null) {
					int headSlot = boss.GetBossHeadTextureIndex();
					if (headSlot != -1) {
						Main.BossNPCHeadRenderer.DrawWithOutlines(boss, headSlot, activationPosition, Color.White, 0, 1, SpriteEffects.None);
					}
				}
				return 40;
			}
			public override BossControllerPetalData GetData(AIState<TBoss> mode) {
				BossControllerPetalData data = 0;
				if (disabled.Contains(mode)) data |= BossControllerPetalData.Disabled;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.ModNPC is TBoss boss) {
						if (npc.aiAction == mode.Index) data |= BossControllerPetalData.Active;
						if (AutomaticIdleState<TBoss>.IsIdleStateActive(boss, mode)) data |= BossControllerPetalData.CurrentIdle;
						break;
					}
				}
				if (!activeInLastRoll.Contains(mode)) data |= BossControllerPetalData.Inactive;
				return data;
			}
			public override bool GetCursorAreaTexture(AIState<TBoss> mode, out Texture2D texture, out Rectangle? frame, out Color color) {
				texture = wireMiniIcons;
				frame = new Rectangle(12 * (1 + disabled.Contains(mode).ToInt()), 0, 10, 10);
				if (disabled.Contains(mode)) {
					color = Color.Firebrick;
				} else if(!activeInLastRoll.Contains(mode)) {
					color = Color.Gray;
				} else {
					color = Color.White;
				}
				return true;
			}
			public override void Click(AIState<TBoss> mode) {
				if (RightClicked) {
					if (!disabled.Add(mode)) disabled.Remove(mode);
					return;
				}
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.ModNPC is TBoss boss) {
						boss.SetAIState(mode.Index);
						break;
					}
				}
			}
			public override IEnumerable<AIState<TBoss>> GetModes() => TBoss.AIStates;
		}
	}
	[Flags]
	public enum BossControllerPetalData {
		Disabled = 1 << 0,
		Active = 1 << 1,
		Inactive = 1 << 2,
		CurrentIdle = 1 << 3,
	}
}
