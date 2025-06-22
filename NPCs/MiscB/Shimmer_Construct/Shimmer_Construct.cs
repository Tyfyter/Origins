using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics.Primitives;
using Origins.Items.Accessories;
using Origins.Items.Armor.Aetherite;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Other.LootBags;
using Origins.Items.Pets;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Summoner;
using Origins.LootConditions;
using Origins.Music;
using Origins.Tiles.BossDrops;
using PegasusLib;
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
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	[AutoloadBossHead]
	public class Shimmer_Construct : ModNPC {
		protected readonly static List<AIState> aiStates = [];
		public readonly int[] previousStates = new int[6];
		public bool IsInPhase2 => isInPhase2;// NPC.life * 2 < NPC.lifeMax;
		public bool IsInPhase3 => isInPhase3;// Main.expertMode && NPC.life * 10 <= NPC.lifeMax;
		internal static IItemDropRule normalDropRule;
		public override void Load() {
			On_NPC.DoDeathEvents += static (On_NPC.orig_DoDeathEvents orig, NPC self, Player closestPlayer) => {
				orig(self, closestPlayer);
				if (self.ModNPC is Shimmer_Construct construct && !construct.noShimmer) {
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
						DoDeathTeleport(construct.averageShimmerSurfacePosition, oldItems);
						oldItems = null;
					}
				}
			};
			On_NPC.Transform += (orig, self, newType) => {
				orig(self, newType);
				if (self.ModNPC is Shimmer_Construct && Main.netMode != NetmodeID.MultiplayerClient) {
					self.ModNPC.OnSpawn(null);
				}
			};
		}
		public override void Unload() {
			normalDropRule = null;
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 7;
			NPCID.Sets.ShimmerTransformToNPC[NPCID.EyeofCthulhu] = Type;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() { // Influences how the NPC looks in the Bestiary
				Position = new Vector2(25, -30),
				Rotation = 0.7f,
				Frame = 6
			};
			for (int i = 0; i < aiStates.Count; i++) aiStates[i].SetStaticDefaults();
		}
		public override void SetDefaults() {
			NPC.width = 100;
			NPC.height = 110;
			NPC.lifeMax = 6600;
			NPC.damage = 27;
			NPC.defense = 14;
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.DD2_CrystalCartImpact;
			NPC.BossBar = GetInstance<Boss_Bar_SC>();
			NPC.aiAction = StateIndex<PhaseOneIdleState>();
			NPC.knockBackResist = 0;
			Array.Fill(previousStates, NPC.aiAction);
		}
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			// I think this is the "normal" amount:
			//NPC.lifeMax = (int)(NPC.lifeMax * balance * bossAdjustment)
		}
		public override void OnSpawn(IEntitySource source) {
			int count = Main.rand.RandomRound(2 + ContentExtensions.DifficultyDamageMultiplier);
			int[] chunks = [
				NPCType<Shimmer_Chunk1>(), NPCType<Shimmer_Chunk1>(),
				NPCType<Shimmer_Chunk2>(), NPCType<Shimmer_Chunk2>(),
				NPCType<Shimmer_Chunk3>(), NPCType<Shimmer_Chunk3>(),
			];
			int n = chunks.Length;
			while (n > 1) {
				n--;
				int k = Main.rand.Next(n + 1);
				(chunks[n], chunks[k]) = (chunks[k], chunks[n]);
			}
			for (int i = 0; i < count; i++) {
				NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, chunks[i], ai0: NPC.whoAmI, ai1: i, ai2: count);
			}
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
			if (aiStates[NPC.aiAction] is not DashState or MagicMissilesState or SpawnDronesStateState or SpawnCloudsState or DroneDashState or ShotgunState) GeometryUtils.AngularSmoothing(ref NPC.rotation, NPC.AngleTo(NPC.GetTargetData().Center) - MathHelper.PiOver2, 0.3f);
			if (deathAnimationTime <= 0 || deathAnimationTime >= DeathAnimationTime) {
				aiStates[NPC.aiAction].DoAIState(this);
				if (!isInPhase2 && (NPC.npcsFoundForCheckActive[NPCType<Shimmer_Chunk1>()] || NPC.npcsFoundForCheckActive[NPCType<Shimmer_Chunk2>()] || NPC.npcsFoundForCheckActive[NPCType<Shimmer_Chunk3>()])) {
					NPC.dontTakeDamage = true;
				} else {
					NPC.dontTakeDamage = false;
				}
			} else {
				NPC.velocity = Vector2.Zero;
				NPC.dontTakeDamage = true;
				if (++deathAnimationTime >= DeathAnimationTime) {
					if (Main.expertMode) {
						NPC.lifeMax = (int)(NPC.lifeMax * PhaseThreeIdleState.HealthMultiplier);
						NPC.life = NPC.lifeMax;
						isInPhase3 = true;
						NPC.netUpdate = true;
					} else {
						NPC.life = 0;
						HitEffect(default);
						NPC.checkDead();
					}
				}
			}
			if (!IsInPhase3 && NPC.GetLifePercent() < 0.5f) isInPhase2 = true;
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
		bool isInPhase2 = false;
		bool isInPhase3 = false;
		static int DeathAnimationTime => Main.expertMode ? 521 : 440;
		public override bool CheckDead() {
			if (deathAnimationTime < DeathAnimationTime) {
				NPC.life = 1;
				return false;
			}
			return true;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				if (deathAnimationTime <= 0) {
					SoundEngine.PlaySound(SoundID.DD2_DefeatScene, NPC.Center);
					deathAnimationTime = 1;
				} else {
					if (Main.netMode != NetmodeID.Server) {
						for (int i = 0; i < chunks.Length; i++) {
							Chunk chk = chunks[i];
							Gore gore = Main.gore[OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), chk.VisualPostion, chk.velocity, chk.ID)];
							gore.position -= new Vector2(gore.Width * 0.5f, gore.Height * 0.5f);
							gore.rotation = chunks[i].rotation;
						}
					}
				}
			}
			if (!IsInPhase3) {
				Rectangle goreArea = NPC.Hitbox.Scaled(0.85f).Recentered(NPC.Center);
				SpawnGore((NPC.Size / 2) - new Vector2(Main.rand.NextFloat(goreArea.Width), Main.rand.NextFloat(goreArea.Height)), Main.rand.Next(1,3).ToString());
			}
		}
		private void SpawnGore(Vector2 position, string type = "2") {
			string kind = type;
			position.X *= NPC.direction;
			if (string.IsNullOrEmpty(kind)) kind = "2";
			Gore.NewGore(
				NPC.GetSource_Death(),
				NPC.Center + position.RotatedBy(NPC.rotation),
				NPC.velocity.RotatedBy(NPC.rotation),
				Mod.GetGoreSlot($"Gores/NPCs/Shimmer_Thing{kind}")
			);
		}
		private int frame = 0;
		public override void FindFrame(int frameHeight) {
			float stage = Math.Min((1 - NPC.GetLifePercent()) * 2, 1) * (Main.npcFrameCount[Type] - 1);
			NPC.frame.Y = frameHeight * (int)stage;
			Vector2[] positions = [
				new(-1, 82),
				new(13, 68),
				new(-13, 71),
				new(-25, 55),
				new(7, 42),
				new(24, 47),
				new(-14, 41),
				new(12, 55),
				new(-11, 55),
				new(0, 65),
				new(-30, 44),
				new(22, 36)
			];
			string[] shard = new string[positions.Length];
			if (!IsInPhase3 && frame < (int)stage) {
				RangeRandom rangeRandom = new(Main.rand, 0, shard.Length);
				int smallShards = (int)stage >= 6 ? Main.rand.Next(2, 5) : Main.rand.Next(6, shard.Length - 1);
				for (int i = 0; i < smallShards; i++) {
					int index = rangeRandom.Get();
					shard[index] = "2";
					rangeRandom.Multiply(index, index + 1, 0);
				}
				for (int i = 0; i < shard.Length; i++) {
					shard[i] ??= $"_Lorg{Main.rand.Next(1,4)}";
				}
				for (int i = 0; i < positions.Length; i++) SpawnGore(positions[i], shard[i]);
			}
			frame = (int)stage;
		}
		Chunk[] chunks = [];
		struct Chunk(int type, Vector2 position) {
			public float rotation;
			public Vector2 position = position;
			public Vector2 velocity = Main.rand.NextVector2Circular(1, 1) * (Main.rand.NextFloat(0.5f, 1f) * 12);
			public Vector2 offset = Vector2.Zero;
			public readonly Vector2 VisualPostion => position + offset;
			public int ID = type;
			public void Update(Shimmer_Construct construct) {
				bool collision = true;
				if (construct.deathAnimationTime < 240) {
					velocity.Y += 0.4f;
				} else if (construct.deathAnimationTime < 440) {
					float prog = float.Pow((construct.deathAnimationTime - 240) / 220f, 2);
					offset = Main.rand.NextVector2Circular(1, 1) * Main.rand.NextFloat(0.5f, 1f) * 12 * prog - Vector2.UnitY * prog * 32;
					velocity *= 0.9f;
				} else if (construct.deathAnimationTime == 440) {
					velocity = new(0, 32 + (ID % 10) * 8);
					offset = VisualPostion - construct.NPC.Center;
				} else {
					float offsetLength = offset.Length();
					if (offsetLength > 0) {
						offset -= offset * (Math.Min(16, offsetLength) / offsetLength);
						offset *= 0.97f;
					}
					float speed = 0.15f + (ID % 10) * 0.01f;
					velocity = velocity.RotatedBy(speed);
					position = construct.NPC.Center + velocity;
					rotation += speed;
					return;
				}
				rotation += velocity.X * 0.1f;
				if (collision) {
					int size = (int)(Math.Min(TextureAssets.Gore[ID].Width(), TextureAssets.Gore[ID].Height()) * 0.9f);
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
				Point lightPos = VisualPostion.ToTileCoordinates();
				Main.instance.LoadGore(ID);
				Main.spriteBatch.Draw(
					TextureAssets.Gore[ID].Value,
					VisualPostion - Main.screenPosition,
					null,
					construct.isInPhase3 ? Color.White : Lighting.GetColor(lightPos),
					rotation,
					TextureAssets.Gore[ID].Size() * 0.5f,
					1,
					SpriteEffects.None,
				0f);
				return false;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 position = NPC.Center;
			if (IsInPhase3 || (deathAnimationTime > 100)) {
				default(ShimmerConstructSDF).Draw(position - screenPos, NPC.rotation);
			}
			if (deathAnimationTime > 0) {
				const float shattertime = 100;
				if (deathAnimationTime >= shattertime) {
					if (chunks.Length <= 0) {
						chunks = [
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece1"), position + new Vector2(38 * NPC.direction, 27).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece2"), position + new Vector2(-26 * NPC.direction, 31).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece3"), position + new Vector2(48 * NPC.direction, -12).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece4"), position + new Vector2(14 * NPC.direction, 14).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece5"), position + new Vector2(-23 * NPC.direction, -57).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece6"), position + new Vector2(22 * NPC.direction, -57).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece7"), position + new Vector2(16 * NPC.direction, -34).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece8"), position + new Vector2(22 * NPC.direction, -20).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece9"), position + new Vector2(-14 * NPC.direction, -11).RotatedBy(NPC.rotation)),
							new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Construct_Piece10"), position + new Vector2(-49 * NPC.direction, -22).RotatedBy(NPC.rotation)),
						];
					}
					for (int i = 0; i < chunks.Length; i++) chunks[i].Draw(this);
					return false;
				}
				position += Main.rand.NextVector2Circular(1, 1) * (Main.rand.NextFloat(0.5f, 1f) * MathF.Pow(deathAnimationTime / shattertime, 1.5f) * 12);
			}
			Main.EntitySpriteDraw(
				TextureAssets.Npc[Type].Value,
				position - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				NPC.Size / 1.5f,
				NPC.scale,
				SpriteEffects.None
			);
			return false;
		}
		public override bool PreKill() {
			if (IsInPhase3) {
				oldItems = new(Main.item.Length);
				for (int i = 0; i < Main.item.Length; i++) {
					if (Main.item[i].active) oldItems[i] = true;
				}
				DoPreDeathTeleport();
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
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ItemType<Jawbreaker>(), 4));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ItemType<Wishing_Glass>(), 4));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(RelicTileBase.ItemType<Shimmer_Construct_Relic>()));
		}
		Point averageShimmerSurfacePosition = Point.Zero;
		bool noShimmer = false;
		internal void DoPreDeathTeleport() {
			noShimmer = true;
			if (OriginSystem.Instance.shimmerPosition is Vector2 baseShimmerPosition) {
				averageShimmerSurfacePosition = Point.Zero;
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
					NPC.Center = averageShimmerSurfacePosition.ToWorldCoordinates() - (Vector2.UnitY * 16 * 8);
					noShimmer = false;
				}
			}
		}
		internal static void DoDeathTeleport(Point averageShimmerSurfacePosition, BitArray oldItems) {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			StringBuilder stringBuilder = new();
			stringBuilder.Append($"Shimmer Construct downed, ");
			if (OriginSystem.Instance.shimmerPosition is Vector2 baseShimmerPosition) {
				stringBuilder.Append($"baseShimmerPosition: {baseShimmerPosition}, ");
				List<Player> players = [];
				foreach (Player player in Main.ActivePlayers) {
					if (player.HasBuff(Weak_Shimmer_Debuff.ID)) {
						players.Add(player);
					}
				}
				Point pos = averageShimmerSurfacePosition;
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
					SoundEngine.PlaySound(SoundID.Item6.WithVolume(0.5f));
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
			}
			stringBuilder.Append($"Shimmering items: [");
			for (int i = 0; i < Main.item.Length; i++) {
				Item item = Main.item[i];
				if (item.active && !oldItems[i]) {
					item.shimmered = true;
					item.shimmerTime = 1;
					item.position += Main.rand.NextVector2Circular(64, 64);
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
			protected static float DifficultyMult => ContentExtensions.DifficultyDamageMultiplier;
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
				int type = ProjectileType<Aetherite_Aura_P>();
				foreach (Projectile projectile in Main.ActiveProjectiles) {
					if (projectile.type == type || projectile.type == Jawbreaker.projectileID) {
						phase3Active = true;
						break;
					}
				}
			}
			player.ManageSpecialBiomeVisuals("Origins:ShimmerConstructPhase3", phase3Active, sourcePos);
		}
	}
	public class Boss_Bar_SC : ModBossBar {
		int bossHeadIndex = -1;
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			if (bossHeadIndex == -1) return null;
			return TextureAssets.NpcHeadBoss[bossHeadIndex];
		}
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			bossHeadIndex = npc.GetBossHeadTextureIndex();
			BossBarLoader.DrawFancyBar_TML(spriteBatch, drawParams);
			return false;
		}
	}
	class Eye_Shimmer_Collision : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type is NPCID.EyeofCthulhu or NPCID.ServantofCthulhu;
		public override void AI(NPC npc) {
			Collision.WetCollision(npc.position, npc.width, npc.height);
			if (Collision.shimmer) {
				switch (npc.type) {
					case NPCID.ServantofCthulhu: goto case -1;
					case NPCID.EyeofCthulhu: {
						if (NPC.CountNPCS(NPCType<Shimmer_Construct>()) <= 0) goto case -1;
						break;
					}
					case -1: {
						npc.buffImmune[BuffID.Shimmer] = false;
						npc.AddBuff(BuffID.Shimmer, 100, true); // Pass true to quiet as clients execute this as well.
						break;
					}
				}
			}
		}
	}
	public struct ShimmerConstructSDF {
		private static VertexRectangle rect = new VertexRectangle();
		public void Draw(Vector2 position, float rotation) {
			MiscShaderData shader = GameShaders.Misc["Origins:ShimmerConstructSDF"];
			shader.UseColor(Color.CornflowerBlue);
			shader.UseSecondaryColor(Color.MediumPurple);
			shader.UseImage1(TextureAssets.Extra[193]);
			//shader.UseImage2(ModContent.Request<Texture2D>("Origins/Textures/SC_Mask"));
			shader.Apply();
			rect.Draw(position, Color.White, new Vector2(256, 256), rotation, position);
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
	}
}
