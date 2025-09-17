using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Magic;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseOneIdleState : AIState {
		#region stats
		public static float IdleTime => 60;
		#endregion stats
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, _ => 1));
		}
		static bool AnyHealthChunks() {
			int[] chunks = [ModContent.NPCType<Shimmer_Chunk1>(), ModContent.NPCType<Shimmer_Chunk2>(), ModContent.NPCType<Shimmer_Chunk3>()];
			foreach (NPC npc in Main.ActiveNPCs) {
				if (chunks.Contains(npc.type)) return true;
			}
			return false;
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			GeometryUtils.AngularSmoothing(ref npc.rotation, npc.AngleTo(npc.GetTargetData().Center) - MathHelper.PiOver2, 0.3f);
			boss.Hover(0.2f);
			if (npc.life == npc.lifeMax && !AnyHealthChunks()) {
				int count = Main.rand.RandomRound(2 + ContentExtensions.DifficultyDamageMultiplier);
				int[] chunks = [
					ModContent.NPCType<Shimmer_Chunk1>(), ModContent.NPCType<Shimmer_Chunk1>(),
					ModContent.NPCType<Shimmer_Chunk2>(), ModContent.NPCType<Shimmer_Chunk2>(),
					ModContent.NPCType<Shimmer_Chunk3>(), ModContent.NPCType<Shimmer_Chunk3>(),
				];
				int n = chunks.Length;
				while (n > 1) {
					n--;
					int k = Main.rand.Next(n + 1);
					(chunks[n], chunks[k]) = (chunks[k], chunks[n]);
				}
				for (int i = 0; i < count; i++) {
					npc.SpawnNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, chunks[i], ai0: npc.whoAmI, ai1: i, ai2: count);
				}
			}
			npc.TargetClosest();
			npc.velocity *= 0.97f;
			if (++npc.ai[0] > IdleTime && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.previousStates.Contains)) Array.Fill(boss.previousStates, Index);
				SelectAIState(boss, aiStates);
			}
			if (npc.HasValidTarget) npc.DiscourageDespawn(60 * 5);
			else npc.EncourageDespawn(60);
		}
		public override void TrackState(int[] previousStates) { }
		public static List<AIState> aiStates = [];
	}
	public class DashState : AIState {
		#region stats
		public static float DashSpeed => 6 + DifficultyMult;
		public static float DashLength => 16 * 30;
		#endregion stats
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.velocity.X = npc.ai[1];
			npc.velocity.Y = npc.ai[2];
			npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;
			if ((++npc.ai[0]) * npc.ai[3] > DashLength) SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = DashSpeed;
			Vector2 direction = npc.DirectionTo(npc.GetTargetData().Center) * npc.ai[3];
			npc.ai[1] = direction.X;
			npc.ai[2] = direction.Y;
		}
	}
	public class CircleState : AIState {
		#region stats
		public static float ShotRate => 16 - DifficultyMult * 0.75f;
		public static int ShotDamage => (int) (15 + 9 * DifficultyMult);
		public static float ShotVelocity => 6;
		public static float MoveSpeed => 6.5f + ContentExtensions.DifficultyDamageMultiplier * 0.5f;
		public static int Duration => 120;
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			Vector2 diff = npc.GetTargetData().Center - npc.Center;
			Vector2 direction = diff.SafeNormalize(Vector2.UnitY);
			Vector2 targetDiff = direction.RotatedBy(npc.direction) * 16 * 30;
			npc.velocity = diff.DirectionFrom(targetDiff) * MoveSpeed;
			npc.rotation = direction.ToRotation() - MathHelper.PiOver2;
			int shotsToHaveFired = (int)((++npc.ai[0]) / npc.ai[3]);
			if (shotsToHaveFired > npc.ai[1]) {
				SoundEngine.PlaySound(SoundID.Item12.WithVolume(0.5f).WithPitchRange(0.25f, 0.4f), npc.Center);
				npc.ai[1]++;
				npc.SpawnProjectile(null,
					npc.Center,
					direction * ShotVelocity,
					Shimmer_Construct_Bullet.ID,
					ShotDamage,
					1
				);
			}
			if (npc.ai[0] > Duration) SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = ShotRate;
		}
	}
	public class Shimmer_Construct_Bullet : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HallowBossRainbowStreak;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			ProjectileID.Sets.NoLiquidDistortion[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() / 2f;
			int trailLength = ProjectileID.Sets.TrailCacheLength[Type];
			static Color GetColorAtPos(Vector2 position) {
				return new Color(LiquidRenderer.GetShimmerBaseColor(position.X / 16, position.Y / 16)).MultiplyRGBA(new(1, 0.9f, 0.9f, 0.5f));
			}
			Vector2 gfxOff = Vector2.UnitY * Projectile.gfxOffY;
			for (int i = trailLength; i > 0; i--) {
				Vector2 oldPos = Projectile.oldPos.GetIfInRange(i);
				if (oldPos == Vector2.Zero) continue;

				Vector2 position = oldPos + Projectile.Size / 2f + gfxOff;

				Color color = GetColorAtPos(position);
				color.A -= (byte)(color.A / 4);
				color *= 0.5f;
				color *= Utils.GetLerpValue(0f, 20f, Projectile.timeLeft, clamped: true);
				color *= ((trailLength - i) / (ProjectileID.Sets.TrailCacheLength[Type] * 1.5f));

				Main.EntitySpriteDraw(texture, position - Main.screenPosition, null, color, Projectile.rotation, origin, MathHelper.Lerp(Projectile.scale, 0.5f, i / (trailLength + 1)), default);
			}
			return false;
		}
	}
	public class SpawnCloudsState : AIState {
		#region stats
		public static int RainDamage => (int) (25 + 7 * DifficultyMult);
		public static float CloudXDistance => 53 - 8 * DifficultyMult;
		public static float CloudYDistance => 25;
		public static float CloudBallSpeed => 4.5f + DifficultyMult * 4.5f;
		#endregion stats
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
			if (Main.dedServ) return;
			On_Main.DrawProjectiles += (orig, self) => {
				orig(self);
				if (Main.dedServ) return;
				if (cachedRain.Count == 0 && cachedClouds.Count == 0) return;
				try {
					GraphicsUtils.drawingEffect = true;
					Origins.shaderOroboros.Capture();
					while (cachedRain.Count != 0) {
						self.DrawProj(cachedRain.Pop());
					}
					while (cachedClouds.Count != 0) {
						self.DrawProj(cachedClouds.Pop());
					}
					ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(Shimmer_Dye.ShaderID, null);
					Origins.shaderOroboros.Stack(shader);
					Origins.shaderOroboros.Release();
				} finally {
					GraphicsUtils.drawingEffect = false;
				}
			};
		}
		internal static Stack<int> cachedClouds = [];
		internal static Stack<int> cachedRain = [];
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			SoundEngine.PlaySound(SoundID.Item28, npc.Center);
			Vector2 targetPos = npc.GetTargetData().Center;
			float xDist = CloudXDistance;
			Vector2 unfurlPos = targetPos - new Vector2(-xDist, CloudYDistance) * 16;
			float speed = CloudBallSpeed;
			npc.SpawnProjectile(null,
				npc.Center,
				npc.DirectionTo(unfurlPos) * speed,
				ModContent.ProjectileType<Shimmer_Construct_Cloud_Ball>(),
				RainDamage,
				1,
				unfurlPos.X,
				unfurlPos.Y,
				-1
			);
			unfurlPos = targetPos - new Vector2(xDist, CloudYDistance) * 16;
			npc.SpawnProjectile(null,
				npc.Center,
				npc.DirectionTo(unfurlPos) * speed,
				ModContent.ProjectileType<Shimmer_Construct_Cloud_Ball>(),
				RainDamage,
				1,
				unfurlPos.X,
				unfurlPos.Y,
				1
			);
			SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override double GetWeight(Shimmer_Construct boss, int[] previousStates) {
			int cloudType = ModContent.ProjectileType<Shimmer_Construct_Cloud_P>();
			int ballType = ModContent.ProjectileType<Shimmer_Construct_Cloud_Ball>();
			int count = 0;
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if ((proj.type == cloudType || proj.type == ballType) && ++count >= 2) return 0;
			}
			return Math.Max(base.GetWeight(boss, previousStates) - 0.5f, 0);
		}

		public class Shimmer_Construct_Cloud_Ball : ModProjectile {
			public override string Texture => typeof(Shimmer_Cloud_Ball).GetDefaultTMLName();
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 4;
				OriginsSets.Projectiles.IsEnemyOwned[Type] = true;
			}
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.RainCloudMoving);
				Projectile.aiStyle = 0;
				Projectile.timeLeft = 18000;
				Projectile.tileCollide = false;
			}
			public Vector2 TargetPos {
				get => new(Projectile.ai[0], Projectile.ai[1]);
				set {
					Projectile.ai[0] = value.X;
					Projectile.ai[1] = value.Y;
				}
			}
			public override void AI() {
				if (TargetPos != default) {
					Vector2 combined = Projectile.velocity * (TargetPos - Projectile.Center);
					if (Projectile.owner == Main.myPlayer && combined.X <= 0 && combined.Y <= 0) {
						SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
						Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							TargetPos,
							default,
							ModContent.ProjectileType<Shimmer_Construct_Cloud_P>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner,
							Projectile.ai[2],
							ai2: NPC.FindFirstNPC(ModContent.NPCType<Shimmer_Construct>())
						);
						Projectile.Kill();
						OriginExtensions.FadeOutOldProjectilesAtLimit([ModContent.ProjectileType<Shimmer_Construct_Cloud_P>(), ModContent.ProjectileType<Shimmer_Construct_Cloud_Ball>()], 3, 52);
						return;
					}
				}

				Projectile.rotation += Projectile.velocity.X * 0.02f;
				Projectile.frameCounter++;
				if (Projectile.frameCounter > 4) {
					Projectile.frameCounter = 0;
					Projectile.frame++;
					if (Projectile.frame > 3)
						Projectile.frame = 0;
				}
			}
			public override bool PreDraw(ref Color lightColor) {
				if (!GraphicsUtils.drawingEffect) {
					cachedClouds.Push(Projectile.whoAmI);
					return false;
				}
				lightColor = Color.LightGray;
				return true;
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				TargetPos = Projectile.Center;
				Projectile.velocity = default;
				return false;
			}
		}
		public class Shimmer_Construct_Cloud_P : ModProjectile {
			public override string Texture => typeof(Shimmer_Cloud_P).GetDefaultTMLName();
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 6;
				OriginsSets.Projectiles.IsEnemyOwned[Type] = true;
			}
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.RainCloudRaining);
				Projectile.aiStyle = 0;
				Projectile.timeLeft = 60 * 200;
				Projectile.width = 28;
				Projectile.height = 28;
				Projectile.tileCollide = false;
			}
			public override void AI() {
				if (Main.getGoodWorld && Projectile.localAI[0] == 0) {
					Projectile.localAI[0] = 1;
					float bestRot = 0;
					int bestLength = 0;
					float baseRot = Main.rand.NextFloat(MathHelper.TwoPi);
					Rectangle hitbox = new(0, 0, 4, 4);
					Vector2 direction = default;
					if (Main.npc.GetIfInRange((int)Projectile.ai[2]) is NPC construct) {
						direction = Projectile.DirectionTo(construct.targetRect.Center());
					}
					for (int i = 0; i < 24; i++) {
						float rot = i * MathHelper.TwoPi / 24f + baseRot;
						Vector2 unit = Vector2.UnitY.RotatedBy(rot);
						if (Vector2.Dot(direction, unit) < -0.2f) continue;
						unit *= 5;
						Vector2 pos = Projectile.Center;
						bool tileCollide = false;
						int length = 0;
						while (length < 180) {
							pos += unit;
							if (hitbox.Recentered(pos).OverlapsAnyTiles()) {
								if (tileCollide) break;
							} else {
								tileCollide = true;
							}
							Tile tile = Framing.GetTileSafely(pos.ToTileCoordinates());
							if (tile.LiquidType == LiquidID.Shimmer && tile.LiquidAmount > 64) {
								unit.Y = -Math.Abs(unit.Y);
							}
							length++;
						}
						if (length > bestLength || (length == bestLength && Main.rand.NextBool(3))) {
							bestRot = rot;
							bestLength = length;
						}
					}
					Projectile.rotation = bestRot;
				}
				if (++Projectile.ai[1] % 10f < 1) {
					Vector2 unit = Vector2.UnitY.RotatedBy(Projectile.rotation);
					Vector2 perp = new(unit.Y, -unit.X);
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center + unit * 24 + perp * Main.rand.Next(-14, 15),
						unit * 5,
						Shimmer_Construct_Cloud_Rain.ID,
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner,
						Projectile.ai[0]
					);
					if (Projectile.timeLeft > 52) {
						if (Main.npc.GetIfInRange((int)Projectile.ai[2]) is NPC construct) {
							if (!construct.active || construct.type != ModContent.NPCType<Shimmer_Construct>()) {
								Projectile.timeLeft = 52;
							} else if (Projectile.rotation == 0) {
								if ((Projectile.Center.X - construct.targetRect.Center.X) * Projectile.ai[0] > 16 * 20) {
									Projectile.timeLeft = 52;
								}
							} else {
								if (Vector2.Dot(Projectile.DirectionTo(construct.targetRect.Center()), Vector2.UnitY.RotatedBy(Projectile.rotation)) < -0.2f) {
									Projectile.timeLeft = 52;
								}
							}
						}
					}
				}
				Projectile.frameCounter++;
				if (Projectile.frameCounter > 4) {
					Projectile.frameCounter = 0;
					Projectile.frame++;
					if (Projectile.frame > 5)
						Projectile.frame = 0;
				}
			}
			public override void SendExtraAI(BinaryWriter writer) {
				writer.Write(Projectile.rotation);
			}
			public override void ReceiveExtraAI(BinaryReader reader) {
				Projectile.rotation = reader.ReadSingle();
			}
			public override bool PreDraw(ref Color lightColor) {
				if (!GraphicsUtils.drawingEffect) {
					cachedClouds.Push(Projectile.whoAmI);
					return false;
				}
				lightColor = Color.LightGray;
				Rectangle frame = TextureAssets.Projectile[Type].Value.Frame(verticalFrames: 6, frameY: Projectile.frame);
				float timeFactor = Math.Min(Projectile.timeLeft / 52f, 1);
				Main.spriteBatch.Draw(
					TextureAssets.Projectile[Type].Value,
					Projectile.Center - Main.screenPosition,
					frame,
					lightColor * timeFactor,
					Projectile.rotation,
					frame.Size() * 0.5f,
					Projectile.scale,
					0,
				0);
				return false;
			}
		}
		public class Shimmer_Construct_Cloud_Rain : ModProjectile {
			public override string Texture => typeof(Shimmer_Cloud_Rain).GetDefaultTMLName();
			public static int ID { get; private set; }
			public override void SetStaticDefaults() {
				Origins.HomingEffectivenessMultiplier[Type] = 2;
				ID = Type;
			}
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.RainFriendly);
				Projectile.friendly = false;
				Projectile.hostile = true;
				Projectile.aiStyle = 0;
				Projectile.width = 4;
				Projectile.height = 4;
				Projectile.tileCollide = false;
				Projectile.hide = true;
				CooldownSlot = ImmunityCooldownID.WrongBugNet;
			}
			public override void AI() {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				Projectile.hide = false;
				if (Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height) && Collision.shimmer) {
					Projectile.velocity.Y -= 0.2f;
				}
				Rectangle hitbox = Projectile.Hitbox;
				hitbox.Inflate(6, 6);
				if (!Projectile.tileCollide && !hitbox.OverlapsAnyTiles()) {
					Projectile.tileCollide = true;
				}
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				modifiers.HitDirectionOverride = (int)Projectile.ai[0];
				modifiers.KnockbackImmunityEffectiveness *= 0.6f;
				modifiers.Knockback.Base += 6;
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				if (projHitbox.Intersects(targetHitbox)) return true;
				const float factor = 1.5f;
				Rectangle hitbox = projHitbox;
				for (int i = 0; i < 7; i++) {
					hitbox.X = projHitbox.X - (int)(Projectile.velocity.X * i * factor);
					hitbox.Y = projHitbox.Y - (int)(Projectile.velocity.Y * i * factor);
					if (hitbox.Intersects(targetHitbox)) return true;
				}
				return false;
			}
			public override bool PreDraw(ref Color lightColor) {
				if (!GraphicsUtils.drawingEffect) {
					cachedRain.Push(Projectile.whoAmI);
					return false;
				}
				lightColor = new Color(1f, 0, 0.08f, 0.3f);
				return true;
			}
		}
	}
	public abstract class Shimmer_Construct_Health_Chunk : ModNPC {
		public virtual void SafeSetStaticDefaults() { }
		public override void SetStaticDefaults() {
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			SafeSetStaticDefaults();
			ContentSamples.NpcBestiaryRarityStars[Type] = 3;
			Minions.Add(Type);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.width = 56;
			NPC.height = 56;
			NPC.lifeMax = 1;
			NPC.damage = 1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.HitSound = SoundID.DD2_CrystalCartImpact;
		}
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) { // hopefully undoes the scaling
			NPC.lifeMax = 1;
			NPC.damage = 0;
			NPC.value = 0;
			NPC.knockBackResist = 1;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
			foreach (IBestiaryInfoElement info in bestiaryEntry.Info) {
				if (info is not NPCStatsReportInfoElement stats) continue;
				stats.OnRefreshStats += Stats_OnRefreshStats;
			}
		}

		private void Stats_OnRefreshStats(NPCStatsReportInfoElement element) {
			BestiaryEntry constructEntry = BestiaryDatabaseNPCsPopulator.FindEntryByNPCID(ModContent.NPCType<Shimmer_Construct>());
			element.Damage = 0;
			element.LifeMax = ((constructEntry.Info.Find(inf => inf is NPCStatsReportInfoElement) as NPCStatsReportInfoElement)?.LifeMax ?? 1) / Main.rand.RandomRound(2 + ContentExtensions.DifficultyDamageMultiplier) / 2;
		}

		public override void AI() {
			NPC.damage = 0;
			if (Main.npc.GetIfInRange((int)NPC.ai[0]) is not NPC owner) {
				NPC.active = false;
				return;
			}
			if (NPC.lifeMax == 1) {
				NPC.lifeMax = (owner.lifeMax / (int)NPC.ai[2]) / 2;
				NPC.life = NPC.lifeMax;
			}
			float distance = 16 * 10 - NPC.ai[3];
			NPC.Center = owner.Center + GeometryUtils.Vec2FromPolar(distance, (MathHelper.TwoPi * NPC.ai[1] / NPC.ai[2]) + (++NPC.localAI[0]) * 0.03f);
			if (NPC.ai[3] <= 0) {
				NPC.localAI[1] *= 0.95f;
				NPC.localAI[2] *= 0.95f;
				NPC.localAI[1] += NPC.velocity.X * 0.05f;
				NPC.localAI[2] += NPC.velocity.Y * 0.05f;
				NPC.velocity *= 0.95f;
				NPC.position += new Vector2(NPC.localAI[1], NPC.localAI[2]) * 18f;
			} else {
				if (NPC.localAI[1] != 0 || NPC.localAI[2] != 0) {
					NPC.velocity += new Vector2(NPC.localAI[1], NPC.localAI[2]);
					NPC.localAI[1] = 0;
					NPC.localAI[2] = 0;
				}
				NPC.ai[3] += Math.Clamp(NPC.velocity.Length() * 1.5f + 1, 6, 16);
				if (distance < 0) DoStrike();
			}
		}
		public void DoStrike() {
			if (Main.npc.GetIfInRange((int)NPC.ai[0]) is NPC owner) {
				for (int i = 0; i < owner.playerInteraction.Length; i++) {
					owner.playerInteraction[i] |= NPC.playerInteraction[i];
				}
				if (!NetmodeActive.MultiplayerClient) owner.StrikeNPC(new() { Damage = NPC.lifeMax }, noPlayerInteraction: true);
			}
			NPC.active = false;
		}
		public override bool CheckDead() {
			NPC.ai[3] += 1;
			NPC.life = 1;
			NPC.dontTakeDamage = true;
			return false;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCID.Sets.NPCBestiaryDrawOffset[Type] with {
					PortraitPositionYOverride = 5,
					Position = new(0)
				};
				Main.instance.LoadItem(ItemID.Handgun);
				Texture2D texture = TextureAssets.Item[ItemID.Handgun].Value;
				Vector2 diff = NPC.Center - Main.LocalPlayer.MountedCenter;
				if (NPC.IsABestiaryIconDummy) diff = Vector2.UnitX;

				spriteBatch.Draw(
					texture,
					NPC.Center - screenPos,
					null,
					drawColor,
					diff.ToRotation() + (diff.X > 0 ? 0 : MathHelper.Pi),
					texture.Size() * 0.5f,
					1.5f,
					diff.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
					0
				);
				return false;
			}
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCID.Sets.NPCBestiaryDrawOffset[Type] with {
				PortraitPositionYOverride = null,
				Position = new(0, -5)
			};
			return true;
		}
	}
	public class Shimmer_Chunk1 : Shimmer_Construct_Health_Chunk {
		public override void SafeSetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCID.Sets.NPCBestiaryDrawOffset[Type] with {
				Hide = false
			};
		}
	}
	public class Shimmer_Chunk2 : Shimmer_Construct_Health_Chunk { }
	public class Shimmer_Chunk3 : Shimmer_Construct_Health_Chunk { }
}
