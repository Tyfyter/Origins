using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Armor.Aetherite;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Other.LootBags;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Summoner;
using Origins.LootConditions;
using Origins.Music;
using Origins.Tiles.BossDrops;
using Origins.Walls;
using ReLogic.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.Utilities.Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class Shimmer_Construct : ModNPC {
		protected readonly static List<AIState> aiStates = [];
		public override string Texture => "Terraria/Images/NPC_" + NPCID.EyeofCthulhu;
		public override string BossHeadTexture => "Terraria/Images/NPC_Head_Boss_0";
		public readonly int[] previousStates = new int[6];
		public bool IsInPhase3 => isInPhase3;// Main.expertMode && NPC.life * 10 <= NPC.lifeMax;
		internal static IItemDropRule normalDropRule;
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
		public override void Unload() {
			normalDropRule = null;
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
			NPC.HitSound = SoundID.DD2_CrystalCartImpact;
			NPC.BossBar = GetInstance<SC_BossBar>();
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
			NPC.dontTakeDamage = false;
			if (deathAnimationTime <= 0 || deathAnimationTime >= DeathAnimationTime) aiStates[NPC.aiAction].DoAIState(this);
			else {
				NPC.velocity = Vector2.Zero;
				NPC.dontTakeDamage = true;
				if (++deathAnimationTime >= DeathAnimationTime) {
					if (Main.expertMode) {
						NPC.lifeMax = (int)(2000 * ContentExtensions.DifficultyDamageMultiplier);
						NPC.life = NPC.lifeMax;
						isInPhase3 = true;
						NPC.netUpdate = true;
					} else {
						NPC.life = 0;
						NPC.checkDead();
					}
				}
			}
			for (int i = 0; i < chunks.Length; i++) chunks[i].Update(this);
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
		int deathAnimationTime = 0;
		bool isInPhase3 = false;
		static int DeathAnimationTime => 480;
		public override bool CheckDead() {
			if (deathAnimationTime < DeathAnimationTime) {
				NPC.life = 1;
				return false;
			}
			return true;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0 && deathAnimationTime <= 0) {
				SoundEngine.PlaySound(SoundID.DD2_DefeatScene, NPC.Center);
				if (deathAnimationTime <= 0) deathAnimationTime = 1;
			}
		}
		Chunk[] chunks = [];
		struct Chunk(int type, Vector2 position) {
			float rotation;
			Vector2 position = position;
			Vector2 velocity = Main.rand.NextVector2Circular(1, 1) * (Main.rand.NextFloat(0.5f, 1f) * 12);
			public void Update(Shimmer_Construct construct) {
				bool collision = true;
				if (construct.deathAnimationTime < 240) {
					velocity.Y += 0.4f;
				} else if (construct.deathAnimationTime < 300) {
					velocity *= 0.9f;
				} else if (construct.deathAnimationTime < 360) {
					velocity *= 0.9f;
					collision = false;
					Vector2 targetPos = construct.NPC.Center;
					velocity += position.DirectionTo(targetPos);
				} else if (construct.deathAnimationTime == 360) {
					velocity = new(0, 32 + type * 8);
					return;
				} else {
					float speed = 0.15f + type * 0.01f;
					velocity = velocity.RotatedBy(speed);
					position = construct.NPC.Center + velocity;
					rotation += speed;
					return;
				}
				rotation += velocity.X * 0.1f;
				if (collision) {
					int size = (int)(Math.Min(TextureAssets.Gore[type].Width(), TextureAssets.Gore[type].Height()) * 0.9f);
					Vector2 halfSize = new(size * 0.5f);
					Vector4 slopeCollision = Collision.SlopeCollision(position - halfSize, velocity, size, size);
					position = slopeCollision.XY() + halfSize;
					velocity = slopeCollision.ZW();
					velocity = Collision.TileCollision(position - halfSize, velocity, size, size);
					if (velocity.Y == 0f) {
						velocity.X *= 0.97f;
						if (velocity.X > -0.01 && velocity.X < 0.01) {
							velocity.X = 0f;
						}
					}
				}
				position += velocity;
			}
			public readonly bool Draw(Shimmer_Construct construct) {
				Point lightPos = position.ToTileCoordinates();
				Main.instance.LoadGore(type);
				Main.spriteBatch.Draw(
					TextureAssets.Gore[type].Value,
					position - Main.screenPosition,
					null,
					construct.isInPhase3 ? Color.White : Lighting.GetColor(lightPos),
					rotation,
					TextureAssets.Gore[type].Size() * 0.5f,
					1,
					SpriteEffects.None,
				0f);
				return false;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 position = NPC.Center;
			if (deathAnimationTime > 0) {
				const float shattertime = 100;
				if (deathAnimationTime >= shattertime) {
					if (chunks.Length <= 0) {
						chunks = [
							new(2, position),
							new(7, position),
							new(9, position),
							new(10, position)
						];
					}
					for (int i = 0; i < chunks.Length; i++) chunks[i].Draw(this);
					return false;
				}
				position += Main.rand.NextVector2Circular(1, 1) * (Main.rand.NextFloat(0.5f, 1f) * MathF.Pow(deathAnimationTime / shattertime, 1.5f) * 12);
			}
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				position - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				new(55, 107),
				NPC.scale,
				SpriteEffects.None,
			0);
			return false;
		}
		public override void BossHeadSlot(ref int index) {
			index = 1;
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
			normalDropRule = new LeadingSuccessRule();
			normalDropRule.OnSuccess(ItemDropRule.OneFromOptions(1,
				ItemType<Cool_Sword>(),
				//ItemType<>(),
				ItemType<Shimmerstar_Staff>(),
				ItemType<Aether_Opal>())
			);

			normalDropRule.OnSuccess(ItemDropRule.OneFromOptions(1,
				ItemType<Lazy_Cloak>(),
				ItemType<Resizing_Glove>())
			);

			normalDropRule.OnSuccess(ItemDropRule.Common(TrophyTileBase.ItemType<Shimmer_Construct_Trophy>(), 10));
			normalDropRule.OnSuccess(ItemDropRule.Common(ItemType<Shimmer_Construct_Mask>(), 10));

			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ItemType<Shimmer_Construct_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ItemType<Wishing_Glass>(), 4));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(RelicTileBase.ItemType<Shimmer_Construct_Relic>()));
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
							player.velocity = Vector2.Zero;
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
			stringBuilder.Append(']');
			//Origins.instance.Logger.Info(stringBuilder.ToString());
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedShimmerConstruct = true;
			NetMessage.SendData(MessageID.WorldData);
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)NPC.aiAction);
			writer.Write((bool)isInPhase3);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadByte();
			isInPhase3 = reader.ReadBoolean();
		}
		public static void SetAIState(Shimmer_Construct boss, int state) {
			NPC npc = boss.NPC;
			aiStates[npc.aiAction].TrackState(boss.previousStates);
			npc.aiAction = state;
			npc.ai[0] = 0;
			aiStates[npc.aiAction].StartAIState(boss);
			npc.netUpdate = true;
		}
		public static int StateIndex<TState>() where TState : AIState => GetInstance<TState>().Index;
		public static void SelectAIState(Shimmer_Construct boss, params List<AIState>[] from) {
			WeightedRandom<int> states = new(Main.rand);
			for (int j = 0; j < from.Length; j++) {
				for (int i = 0; i < from[j].Count; i++) {
					states.Add(from[j][i].Index, from[j][i].GetWeight(boss, boss.previousStates));
				}
			}
			SetAIState(boss, states);
		}
		public override Color? GetAlpha(Color drawColor) => Color.White * NPC.Opacity;
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
	public class SC_Scene_Effect : BossMusicSceneEffect<Shimmer_Construct> {
		public override int Music => Origins.Music.ShimmerConstruct;
		Asset<Texture2D> circle = Main.Assets.Request<Texture2D>("Images/Misc/StarDustSky/Planet");
		float scale = 0;
		public override void SpecialVisuals(Player player, bool isActive) {
			Vector2 sourcePos = default;
			bool phase3Active = false;
			if (isActive) {
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.ModNPC is Shimmer_Construct shimmerConstruct) {
						sourcePos = npc.Center;
						phase3Active = shimmerConstruct.IsInPhase3;
						break;
					}
				}
			}
			if (phase3Active) {
				if (float.IsFinite(scale)) {
					const float radius = 312f;
					scale += 0.05f;
					if (sourcePos.WithinRange(Main.screenPosition, radius * scale)
						&& sourcePos.WithinRange(Main.screenPosition + Main.ScreenSize.ToVector2() * Vector2.UnitX, radius * scale)
						&& sourcePos.WithinRange(Main.screenPosition + Main.ScreenSize.ToVector2() * Vector2.UnitY, radius * scale)
						&& sourcePos.WithinRange(Main.screenPosition + Main.ScreenSize.ToVector2(), radius * scale)
					) {
						scale = float.PositiveInfinity;
					}
				}
				if (float.IsFinite(scale)) {
					SC_Phase_Three_Overlay.drawDatas.Add(new(
						circle.Value,
						sourcePos - Main.screenPosition,
						null,
						Color.White
					) {
						origin = circle.Size() * 0.5f,
						scale = Vector2.One * scale
					});
				} else {
					SC_Phase_Three_Overlay.drawDatas.Add(new(
						TextureAssets.MagicPixel.Value,
						new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
						Color.White
					));
				}
			} else {
				scale = 0;
			}
			if (!phase3Active) {
				int type = ModContent.ProjectileType<Aetherite_Aura_P>();
				foreach (Projectile projectile in Main.ActiveProjectiles) {
					if (projectile.type == type) {
						phase3Active = true;
						break;
					}
				}
			}
			player.ManageSpecialBiomeVisuals("Origins:ShimmerConstructPhase3", phase3Active, sourcePos);
		}
	}
	public class SC_BossBar : ModBossBar {
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			return TextureAssets.Item[ItemID.ShimmerBlock]; // Corgi head icon
		}

		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			return true;
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
