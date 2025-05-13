using Humanizer;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Summoner;
using PegasusLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class Shimmer_Construct : ModNPC {
		protected readonly static List<AIState> aiStates = [];
		public override string Texture => "Terraria/Images/NPC_" + NPCID.EyeofCthulhu;
		public readonly int[] previousStates = new int[6];
		public bool IsInPhase3 => Main.expertMode && NPC.life * 10 < NPC.lifeMax;
		public override void Load() {
			On_NPC.DoDeathEvents += static (On_NPC.orig_DoDeathEvents orig, NPC self, Player closestPlayer) => {
				orig(self, closestPlayer);
				if (self.ModNPC is Shimmer_Construct) {
					if (oldItems is not null) {
						Rectangle npcRect = self.Hitbox;
						const int active_range = 5000;
						Rectangle playerRect = new(0, 0, active_range * 2, active_range * 2);
						List<Player> players = [];
						foreach (Player player in Main.ActivePlayers) {
							if (self.playerInteraction[player.whoAmI] || npcRect.Intersects(playerRect.Recentered(player.Center))) {
								player.AddBuff(Weak_Shimmer_Debuff.ID, 5, true);
								players.Add(player);
							}
						}
						DoDeathTeleport(self.Center, oldItems);
						oldItems = null;
					}
				}
			};
		}
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
					if (NPC.playerInteraction[player.whoAmI] || npcRect.Intersects(playerRect.Recentered(player.Center))) {
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
		public override bool PreKill() {
			if (IsInPhase3) {
				oldItems = new(Main.item.Length);
				for (int i = 0; i < Main.item.Length; i++) {
					if (Main.item[i].active) oldItems[i] = true;
				}
			}
			return base.PreKill();
		}
		static BitArray oldItems;
		public override void BossLoot(ref string name, ref int potionType) {
			potionType = ItemID.RestorationPotion;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new OneFromRulesRule(1,
				ItemDropRule.Common(ModContent.ItemType<Cool_Sword>()),
				//ItemDropRule.Common(ModContent.ItemType<>()),
				ItemDropRule.Common(ModContent.ItemType<Shimmerstar_Staff>()),
				ItemDropRule.Common(ModContent.ItemType<Aether_Opal>())
				//ItemDropRule.Common(ModContent.ItemType<Cool_Sword>())
			));
		}
		internal static void DoDeathTeleport(Vector2 offset, BitArray oldItems) {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			StringBuilder stringBuilder = new();
			stringBuilder.Append($"Shimmer Construct downed, ");
			if (OriginSystem.Instance.shimmerPosition is Vector2 baseShimmerPosition) {
				stringBuilder.Append($"baseShimmerPosition: {baseShimmerPosition}, ");
				Point averageShimmerSurfacePosition = Point.Zero;
				int shimmerCount = 0;
				Point pos = baseShimmerPosition.ToPoint();
				for (int i = -100; i <= 100; i++) {
					for (int j = -100; j <= 100; j++) {
						if (Framing.GetTileSafely(pos.X + i, pos.Y + j).LiquidType == LiquidID.Shimmer) {
							Tile above = Framing.GetTileSafely(pos.X + i, pos.Y + j - 1);
							if (above.LiquidAmount > 0 && above.LiquidType == LiquidID.Shimmer) continue;
							averageShimmerSurfacePosition.X += pos.X + i;
							averageShimmerSurfacePosition.Y += pos.Y + j;
							shimmerCount++;
						}
					}
				}
				if (shimmerCount != 0) {
					averageShimmerSurfacePosition.X /= shimmerCount;
					averageShimmerSurfacePosition.Y /= shimmerCount;
					if (Framing.GetTileSafely(averageShimmerSurfacePosition).LiquidType == LiquidID.Shimmer) {
						for (; Framing.GetTileSafely(averageShimmerSurfacePosition).LiquidType == LiquidID.Shimmer; averageShimmerSurfacePosition.Y--) ;
					} else {
						for (; Framing.GetTileSafely(averageShimmerSurfacePosition).LiquidType == LiquidID.Shimmer; averageShimmerSurfacePosition.Y++) ;
					}
					offset -= averageShimmerSurfacePosition.ToWorldCoordinates() - (Vector2.UnitY * 16 * 8);
					List<Player> players = [];
					foreach (Player player in Main.ActivePlayers) {
						if (player.HasBuff(Weak_Shimmer_Debuff.ID)) {
							players.Add(player);
						}
					}
					pos = averageShimmerSurfacePosition;
					List<Point> positions = [];
					static bool CheckTeleport(int x, int y) {
						for (int i = x - 1; i <= x + 1; i++) {
							for (int j = y - 3; j < y; j++) {
								if (Framing.GetTileSafely(i, j).HasFullSolidTile()) return false;
							}
						}
						return true;
					}
					for (int i = 0; i <= 100; i++) {
						for (int j = -10; j <= 10; j++) {
							if (Framing.GetTileSafely(pos.X + i, pos.Y + j).HasSolidTile()) {
								Tile above = Framing.GetTileSafely(pos.X + i, pos.Y + j - 1);
								if (above.LiquidAmount <= 0 || above.LiquidType != LiquidID.Shimmer) {
									if (CheckTeleport(pos.X + i, pos.Y + j)) {
										positions.Add(new(pos.X + i, pos.Y + j - 3));
									}
								}
							}
							if (Framing.GetTileSafely(pos.X - i, pos.Y + j).HasSolidTile()) {
								Tile above = Framing.GetTileSafely(pos.X + i, pos.Y + j - 1);
								if (above.LiquidAmount <= 0 || above.LiquidType != LiquidID.Shimmer) {
									if (CheckTeleport(pos.X + i, pos.Y + j)) {
										positions.Add(new(pos.X + i + 2, pos.Y + j - 3));
									}
								}
							}
							if (positions.Count >= players.Count * 4 + 8) {
								break;
							}
						}
					}
					stringBuilder.Append($"player positions: [{string.Join(", ", positions)}], ");
					if (positions.Count > 0) {
						List<(Player player, Vector2 position)> teleports = [];
						if (positions.Count < players.Count) {
							for (int i = 0; i < players.Count; i++) {
								teleports.Add((players[i], Main.rand.Next(positions).ToWorldCoordinates(0, -8)));
							}
						} else {
							for (int i = 0; i < players.Count; i++) {
								pos = Main.rand.Next(positions);
								positions.Remove(pos);
								teleports.Add((players[i], pos.ToWorldCoordinates(0, -8)));
							}
						}
						stringBuilder.Append($"teleports: [{string.Join(", ", teleports.Select(static ((Player player, Vector2 position) tele) => $"{tele.player.name}: {tele.position}"))}], ");
						for (int i = 0; i < teleports.Count; i++) {
							(Player player, Vector2 position) = teleports[i];
							ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
								PositionInWorld = player.Bottom
							});
							player.Teleport(position, 12);
							ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
								PositionInWorld = player.Bottom
							});
						}
						if (Main.netMode != NetmodeID.SinglePlayer) {
							ModPacket packet = Origins.instance.GetPacket();
							packet.Write(Origins.NetMessageType.mass_teleport);
							packet.Write((ushort)teleports.Count);
							for (int i = 0; i < teleports.Count; i++) {
								(Player player, Vector2 position) = teleports[i];
								packet.Write((ushort)player.whoAmI);
								packet.WritePackedVector2(position);
							}
							packet.Send();
						}
					}
				} else {
					stringBuilder.Append($"baseShimmerPosition: null, ");
					offset = Vector2.Zero;
				}
			} else {
				offset = Vector2.Zero;
			}
			stringBuilder.Append($"item offset: {offset}, Shimmering items: [");
			for (int i = 0; i < Main.item.Length; i++) {
				Item item = Main.item[i];
				if (item.active && !oldItems[i]) {
					item.shimmered = true;
					item.shimmerTime = 1;
					item.position -= offset;
					stringBuilder.Append($"{item.Name}, ");
					if (Main.netMode != NetmodeID.SinglePlayer) {
						NetMessage.SendData(MessageID.SyncItemsWithShimmer, -1, -1, null, i, 1f);
					}
				}
			}
			stringBuilder.Append($"]");
			Origins.instance.Logger.Info(stringBuilder.ToString());
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedShimmerConstruct = true;
			NetMessage.SendData(MessageID.WorldData);
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
