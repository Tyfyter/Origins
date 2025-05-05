using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Dev;
using Origins.Items.Weapons.Summoner;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace Origins.NPCs.Dungeon {
	public class Etherealizer : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 6, 32, 56);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		//public override void Load() => this.AddBanner();
		protected override bool CloneNewInstances => true;
		static public int ID { get; private set; }
		AutoLoadingAsset<Texture2D> glowTexture;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			glowTexture = Texture + "_Glow";
			ID = Type;
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.Caster;
			NPC.lifeMax = 300;
			NPC.defense = 17;
			NPC.damage = 33;
			NPC.width = 20;
			NPC.height = 38;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath24.WithPitch(0.6f);
			NPC.value = Item.buyPrice(silver: 15);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!NPC.downedPlantBoss) return 0;
			if (!spawnInfo.HasRightDungeonWall(NPCExtensions.DungeonWallType.Brick)) return 0;
			return SpawnCondition.DungeonNormal.Chance * 0.0166f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {

		}
		public int ProjectileID {
			get => (int)NPC.ai[1] - 1;
			set => NPC.ai[1] = value + 1;
		}
		public override bool PreAI() {
			NPC.velocity.X *= 0.93f;
			if (NPC.velocity.X > -0.1 && NPC.velocity.X < 0.1) NPC.velocity.X = 0f;

			if (NPC.ai[0] == 0f) NPC.ai[0] = 500f;
			if (NPC.ai[2] != 0 && NPC.ai[3] != 0) {
				NPC.position += NPC.netOffset;

				SoundEngine.PlaySound(SoundID.Item8, NPC.position);
				for (int i = 0; i < 50; i++) {
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.DungeonSpirit, 0f, 0f, 100, default, 1.5f);
					dust.velocity *= 2f;
					dust.noGravity = true;
				}

				NPC.position = new Vector2(NPC.ai[2] * 16f - 8, NPC.ai[3] * 16f) - NPC.Size * new Vector2(0.5f, 1f);
				NPC.netOffset = Vector2.Zero;
				NPC.velocity = Vector2.Zero;
				SoundEngine.PlaySound(SoundID.Item8, NPC.position);
				for (int i = 0; i < 50; i++) {
					Dust dust = Dust.NewDustDirect(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.DungeonSpirit, 0f, 0f, 100, default, 1.5f);
					dust.velocity *= 2f;
					dust.noGravity = true;
				}
				NPC.ai[2] = 0;
				NPC.ai[3] = 0;
			}
			if (ProjectileID == -1) {
				NPC.TargetClosestUpgraded();
				NPC.ai[0] += 1f;
				if (NPC.ai[0] >= 120f && Main.netMode != NetmodeID.MultiplayerClient) {
					NPC.ai[0] = 1f;
					int targetTileX = (int)Main.player[NPC.target].Center.X / 16;
					int targetTileY = (int)Main.player[NPC.target].Center.Y / 16;
					Vector2 chosenTile = Vector2.Zero;
					if (NPC.AI_AttemptToFindTeleportSpot(ref chosenTile, targetTileX, targetTileY)) {
						NPC.ai[2] = chosenTile.X;
						NPC.ai[3] = chosenTile.Y;
					}

					NPC.netUpdate = true;
				} else if (NPC.ai[0] == 60f) {
					SoundEngine.PlaySound(SoundID.Item8, NPC.position);
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						Vector2 spawnPos = NPC.Top + new Vector2(NPC.direction * 8, 20);
						Vector2 diff = NPC.GetTargetData().Center - spawnPos;
						float difficultyMultiplier = Math.Min(ContentExtensions.DifficultyDamageMultiplier, 4);
						ProjectileID = Projectile.NewProjectile(
							NPC.GetSource_FromAI(),
							spawnPos,
							diff.SafeNormalize(default) * 2,
							Etherealizer_P.ID,
							(int)((40 - 5 * difficultyMultiplier) * difficultyMultiplier),
							0,
							ai0: NPC.whoAmI
						);
					}
				}
			} else {
				Projectile projectile = Main.projectile[ProjectileID];
				if (!projectile.active || projectile.type != Etherealizer_P.ID || projectile.ai[0] != NPC.whoAmI) {
					ProjectileID = -1;
				} else {
					NPC.DiscourageDespawn(60);
				}
			}


			NPC.position += NPC.netOffset;
			if (Main.rand.NextBool(2)) {
				Dust.NewDustDirect(NPC.position + Vector2.UnitY * 2, NPC.width, NPC.height, DustID.DungeonSpirit, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default).noGravity = true;
			}

			NPC.position -= NPC.netOffset;
			NPC.spriteDirection = NPC.direction;
			return false;
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}
		}
		public int Frame {
			get => NPC.frame.Y / 56;
			set => NPC.frame.Y = value * 56;
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.aiAction != 1 && NPC.velocity.Y == 0 && ++NPC.frameCounter > 7) {
				NPC.frameCounter = 0;
				Frame++;
			}
			int frameOffset = ProjectileID == -1 ? 0 : 3;
			if (Frame < frameOffset || Frame >= 3 + frameOffset) {
				Frame = frameOffset;
				NPC.frameCounter = 0;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life > 0) {
				for (int i = (int)(hit.Damage / (float)NPC.lifeMax * 50f); i-->0;) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, hit.HitDirection, -1f);
				}
			} else {
				for (int i = 0; i < 20; i++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, 2.5f * hit.HitDirection, -2.5f);
				}

				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 42, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
			}
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			const float strength = 0.75f;
			Glowing_Mod_NPC.DrawGlow(spriteBatch, screenPos, glowTexture, NPC, new Color(strength, strength, strength, 0.5f));
		}
	}
	public class Etherealizer_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicMissile;
		static public int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = ProjectileID.Sets.TrailingMode[ProjectileID.MagicMissile];
			ProjectileID.Sets.TrailCacheLength[Type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.MagicMissile];
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.hostile = true;
			Projectile.timeLeft = 600;
			Projectile.width = Projectile.height = 8;
		}
		public override void AI() {
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (!owner.active || owner.type != Etherealizer.ID || owner.ai[1] != Projectile.whoAmI + 1 || Projectile.velocity.LengthSquared() < 0.01f) {
				Projectile.Kill();
				return;
			}
			NPCAimedTarget target = owner.GetTargetData();
			void MoveTowardsTarget(Vector2 target, float inertia = 25) {
				Projectile.velocity = (((target - Projectile.Center).SafeNormalize(default) * 8 + Projectile.velocity * (inertia - 1)) / inertia).WithMaxLength(8);
			}
			if (CollisionExt.CanHitRay(Projectile.Center, target.Center)) {
				MoveTowardsTarget(target.Center);
			} else if (Projectile.ai[1] != 0) {
				Vector2 targetPos = new(Projectile.ai[1], Projectile.ai[2]);
				MoveTowardsTarget(targetPos);
				if (Projectile.DistanceSQ(targetPos) < 16 * 16) {
					Projectile.ai[1] = 0;
				}
			} else {
				Pathfind(target);
			}
			float scale = MathF.Pow(1 - Math.Min(Projectile.timeLeft / 300f, 1), 0.5f) * 0.5f + 0.5f;
			Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100, Scale: scale).noGravity = true;
		}
		public void Pathfind(NPCAimedTarget target) {
			int bestMatch = -1;
			float bestMatchDist = float.PositiveInfinity;
			Vector2 diff = target.Center - Projectile.Center;
			for (int i = 0; i < dirs.Length; i++) {
				float dist = dirs[i].ToVector2().DistanceSQ(diff);
				if (dist < bestMatchDist) {
					bestMatchDist = dist;
					bestMatch = i;
				}
			}
			diff.Normalize();
			Vector2 position = Projectile.position;
			position += diff * CollisionExt.Raymarch(position, diff, 16 * 50);
			Vector2 clockwiseTarget = default;
			Vector2 counterclockwiseTarget = default;
			if (Crawl(target, position.ToTileCoordinates(), bestMatch, false) is Point pos1) {
				clockwiseTarget = pos1.ToWorldCoordinates();
				/*Rectangle rect = new(0, 0, 16, 16);
				rect.X = pos1.X * 16;
				rect.Y = pos1.Y * 16;
				OriginExtensions.DrawDebugOutline(rect);*/
			}
			if (Crawl(target, position.ToTileCoordinates(), bestMatch, true) is Point pos2) {
				counterclockwiseTarget = pos2.ToWorldCoordinates();
			}
			if (clockwiseTarget == default && counterclockwiseTarget == default) return;
			if (target.Center.DistanceSQ(clockwiseTarget) > target.Center.DistanceSQ(counterclockwiseTarget)) {
				Projectile.ai[1] = counterclockwiseTarget.X;
				Projectile.ai[2] = counterclockwiseTarget.Y;
			} else {
				Projectile.ai[1] = clockwiseTarget.X;
				Projectile.ai[2] = clockwiseTarget.Y;
			}
		}
		static Point[] dirs = [
			new(0, -1),
			new(1, -1),
			new(1, 0),
			new(1, 1),
			new(0, 1),
			new(-1, 1),
			new(-1, 0),
			new(-1, -1)
		];
		public Point? Crawl(NPCAimedTarget target, Point pos, int dir, bool counterclockwise) {
			const float max_dist = 16 * 50;
			static bool IsValidPosition(Point pos) => !Framing.GetTileSafely(pos).HasFullSolidTile();
			static bool IsOnes(Point p) => p.X is -1 or 1 && p.Y is -1 or 1;
			List<Point> path = [pos];
			//Rectangle rect = new(0, 0, 16, 16);
			test:
			if (CollisionExt.CanHitRay(pos.ToWorldCoordinates(), target.Position)) {
				if (CollisionExt.CanHitRay(Projectile.Center, path[^1].ToWorldCoordinates())) return path[^1];
				if (path.Count <= 1) return null;
				Point? nextTarget = path[1];
				for (int i = 1; i < path.Count; i++) {
					/*rect.X = path[i].X * 16;
					rect.Y = path[i].Y * 16;
					OriginExtensions.DrawDebugOutline(rect);*/
					if (CollisionExt.CanHitRay(Projectile.Center, path[i].ToWorldCoordinates())) {
						nextTarget = path[i];
					} else {
						Point step = path[i] - nextTarget.Value;
						if (IsOnes(step)) {
							for (int j = 0; j < dirs.Length; j++) {
								if (step == dirs[j]) {
									nextTarget += dirs[(j + (counterclockwise ? -1 : 1) + dirs.Length) % dirs.Length];
									break;
								}
							}
						}
					}
				}
				return nextTarget;
			}
			//rect.X = pos.X * 16;
			//rect.Y = pos.Y * 16;
			//OriginExtensions.DrawDebugOutline(rect);
			if (counterclockwise) {
				for (int i = 0; i < dirs.Length; i++) {
					int nextDir = (dir - i + dirs.Length) % dirs.Length;
					if (IsValidPosition(pos + dirs[nextDir])) {
						pos += dirs[nextDir];
						dir = (nextDir + 2) % dirs.Length;
						/*rect.X = pos.X * 16;
						rect.Y = pos.Y * 16;
						OriginExtensions.DrawDebugOutline(rect);*/
						//Dust.NewDustPerfect(pos.ToWorldCoordinates(), 6, Vector2.Zero).noGravity = true;
						break;
					}
					/*Point miss = pos + dirs[nextDir];
					rect.X = miss.X * 16;
					rect.Y = miss.Y * 16;
					OriginExtensions.DrawDebugOutline(rect, dustType: DustID.DungeonWater);*/
				}
			} else {
				for (int i = 0; i < dirs.Length; i++) {
					int nextDir = (dir + i) % dirs.Length;
					if (IsValidPosition(pos + dirs[nextDir])) {
						pos += dirs[nextDir];
						dir = (nextDir - 2 + dirs.Length) % dirs.Length;
						//Dust.NewDustPerfect(pos.ToWorldCoordinates(), 6, Vector2.Zero).noGravity = true;
						/*rect.X = pos.X * 16;
						rect.Y = pos.Y * 16;
						OriginExtensions.DrawDebugOutline(rect);*/
						break;
					}
					/*Point miss = pos + dirs[nextDir];
					rect.X = miss.X * 16;
					rect.Y = miss.Y * 16;
					OriginExtensions.DrawDebugOutline(rect, dustType: DustID.DungeonWater);*/
				}
			}
			if (path.Contains(pos) || pos.ToWorldCoordinates().DistanceSQ(target.Center) > max_dist * max_dist) return null;
			path.Add(pos);
			goto test;
		}
		void ModifyDamage(ref StatModifier damage) {
			damage += 1 - Math.Min(Projectile.timeLeft / 300f, 1);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			ModifyDamage(ref modifiers.SourceDamage);
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			ModifyDamage(ref modifiers.SourceDamage);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Projectile.Kill();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.Kill();
		}
		public override void OnKill(int timeLeft) {
			NPC owner = Main.npc[(int)Projectile.ai[0]];
			if (owner.active && owner.type == Etherealizer.ID && owner.ai[1] == Projectile.whoAmI) {
				owner.ai[1] = -1;
			}
			float scale = MathF.Pow(1 - Math.Min(Projectile.timeLeft / 300f, 1), 0.5f) * 0.5f + 0.5f;
			for (int i = 0; i < 6; i++) {
				Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.DungeonSpirit,
					Projectile.velocity.X * 0.5f,
					Projectile.velocity.Y * 0.5f,
					100,
					Scale: scale
				).noGravity = true;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[1] = 0;
			return Projectile.velocity.LengthSquared() == 0;
		}
		public override bool PreDraw(ref Color lightColor) {
			default(MagicMissileDrawer).Draw(Projectile);
			return false;
		}
	}
}
