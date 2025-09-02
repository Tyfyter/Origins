using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoMod.Cil;
using Origins.Core;
using Origins.Items.Other.Dyes;
using Origins.Items.Weapons.Magic;
using Origins.Projectiles;
using PegasusLib;
using PegasusLib.Graphics;
using PegasusLib.Networking;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Light;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseThreeIdleState : AIState {
		#region stats
		public static float HealthMultiplier => 2000f / 6600f;
		public static float IdleTime => 60 - DifficultyMult * 10;
		#endregion stats
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => boss.IsInPhase3.Mul(3)));
		}
		public override void SetStaticDefaults() {
			aiStates.Add(ModContent.GetInstance<MagicMissilesState>());
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			GeometryUtils.AngularSmoothing(ref npc.rotation, npc.AngleTo(npc.GetTargetData().Center) - MathHelper.PiOver2, 0.3f);
			boss.Hover(0.2f);
			npc.TargetClosest();
			Vector2 hoverTarget = npc.GetTargetData().Center - Vector2.UnitY * 16 * 15;
			npc.velocity += (hoverTarget - npc.Center).Normalized(out float distance) * 0.5f;
			if (distance > 16 * 250) {
				npc.Center = hoverTarget;
			}
			npc.velocity *= 0.97f;
			if (++npc.ai[0] > (60 - ContentExtensions.DifficultyDamageMultiplier * 10) && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.previousStates.Contains)) Array.Fill(boss.previousStates, Index);
				SelectAIState(boss, aiStates);
			}
			if (npc.HasValidTarget) npc.DiscourageDespawn(60 * 5);
			else npc.EncourageDespawn(60);
		}
		public override void TrackState(int[] previousStates) { }
		public static List<AIState> aiStates = [];
	}
	public class ShimmerLandminesState : AIState {
		#region stats
		public static int StarDamage => (int) (32 + 6 * DifficultyMult);
		public static int AreaYOffset => 16 * 10;
		public static int AreaWidth => 60;
		public static int AreaHeight => 80;
		/// <summary>
		/// I'm not actually 100% sure what unit this is in, but it gets more dense the lower it is
		/// </summary>
		public static float Density => 16 * (30 - DifficultyMult * 2);
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		readonly Stack<Vector2> positions = new();
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			GeometryUtils.AngularSmoothing(ref npc.rotation, npc.AngleTo(npc.GetTargetData().Center) - MathHelper.PiOver2, 0.3f);
			npc.velocity *= 0.97f;
			boss.Hover();
			Vector2 targetPos = npc.GetTargetData().Center;
			npc.velocity += npc.DirectionTo(targetPos - Vector2.UnitY * 16 * 15) * 0.5f;
			for (int i = 0; i < 2; i++) {
				SoundEngine.PlaySound(SoundID.Item88, npc.Center);
				SoundEngine.PlaySound(SoundID.Item91.WithPitchRange(1.65f, 1.8f).WithVolume(0.75f), npc.Center);
				if (positions.TryPop(out Vector2 position)) {
					npc.SpawnProjectile(null,
						position,
						Vector2.Zero,
						Shimmer_Landmines.ID,
						StarDamage,
						1
					);
				} else {
					SetAIState(boss, StateIndex<AutomaticIdleState>());
				}
			}
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			float lowestHeight = 0;
			int buffID = ModContent.BuffType<Weak_Shimmer_Debuff>();
			foreach (Player player in Main.ActivePlayers) {
				if (lowestHeight < player.BottomLeft.Y && player.HasBuff(buffID)) {
					lowestHeight = player.BottomLeft.Y;
				}
			}
			if (lowestHeight != 0) {
				Vector2 basePos = npc.GetTargetData().Center;
				basePos.Y = lowestHeight + AreaYOffset;
				Rectangle area = OriginExtensions.BoxOf(basePos - new Vector2(AreaWidth * 16, 0), basePos + new Vector2(AreaWidth * 16, AreaHeight * 16));
				List<Vector2> newPositions = Main.rand.PoissonDiskSampling(area, Density);
				positions.Clear();
				while (newPositions.Count > 0) {
					int rand = Main.rand.Next(newPositions.Count);
					positions.Push(newPositions[rand]);
					newPositions.RemoveAt(rand);
				}
			}
		}
		public override double GetWeight(Shimmer_Construct boss, int[] previousStates) => (!previousStates.Contains(Index)).ToInt();
		public class Shimmer_Landmines : ModProjectile {
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;
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
				Projectile.timeLeft = 60 * 20;
				Projectile.aiStyle = 0;
				Projectile.width = 24;
				Projectile.height = 24;
				Projectile.tileCollide = false;
			}
			public override void AI() {
				if (Projectile.localAI[0] == 0) {
					Projectile.localAI[0] = 1;
					Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
					Vector2 pos = Projectile.position;
					foreach (NPC npc in Main.ActiveNPCs) {
						if (npc.ModNPC is Shimmer_Construct) {
							pos = npc.Center - Projectile.Size * 0.5f;
							break;
						}
					}
					float rotation = (pos - Projectile.position).ToRotation();
					for (int i = 0; i < Projectile.oldPos.Length; i++) {
						Projectile.oldPos[i] = Vector2.Lerp(Projectile.position, pos, i / (float)Projectile.oldPos.Length);
						Projectile.oldRot[i] = rotation;
					}
				}
				if (Projectile.timeLeft < 15) {
					Projectile.Opacity = Projectile.timeLeft / 15f;
				} else if (!NPC.npcsFoundForCheckActive[ModContent.NPCType<Shimmer_Construct>()]) {
					Projectile.timeLeft = 15;
				}
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				int direction = Math.Sign(target.Center.X - Projectile.Center.X);
				if (direction == 0) direction = Main.rand.NextBool().ToDirectionInt();
				modifiers.HitDirectionOverride = direction;
				modifiers.KnockbackImmunityEffectiveness *= 0.8f;
				modifiers.Knockback.Base += 6;
			}
			public override bool PreDraw(ref Color lightColor) {
				Shimmerstar_Staff_P.DrawShimmerstar(Projectile);
				return false;
			}
		}
	}
	public class DroneDashState : AIState {
		#region stats
		public static float DashSpeed => 6 + DifficultyMult;
		public static float SpawnCount => DifficultyMult * 2 - 2;
		/// <summary>
		/// measured in tiles traveled
		/// </summary>
		public static float SpawnRate => 16 * (11 - ContentExtensions.DifficultyDamageMultiplier * 2);
		public static int NoSpawnThreshold => 2;
		#endregion stats
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.velocity = npc.ai[1].ToRotationVector2() * npc.ai[3] * (npc.ai[2] > 0 ? 1.15f : 1.3f);
			npc.rotation = npc.velocity.ToRotation();
			if ((++npc.ai[0]) * npc.ai[3] > SpawnRate) {
				npc.ai[0] = 0;
				if (npc.ai[2] == 0) {
					SetAIState(boss, StateIndex<AutomaticIdleState>());
				}
				if (npc.ai[2] > 0) {
					npc.ai[2]--;
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						NPC.NewNPCDirect(
							npc.GetSource_FromAI(),
							npc.Center,
							ModContent.NPCType<Shimmer_Drone>()
						).velocity = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2() * Main.rand.NextFloat(4, 8);
					}
				} else {
					npc.ai[2]++;
				}
				npc.ai[2] = (int)npc.ai[2];
			}
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = DashSpeed;
			npc.ai[1] = (npc.GetTargetData().Center - npc.Center).ToRotation();
			npc.ai[2] = SpawnCount;

			int droneType = ModContent.NPCType<Shimmer_Drone>();
			int drones = 0;
			int threshold = NoSpawnThreshold;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other.type == droneType && ++drones >= threshold) {
					npc.ai[2] = -npc.ai[2];
					break;
				}
			}
		}
	}
	public class ShotgunState : AIState {
		#region stats
#pragma warning disable IDE0060 // Remove unused parameter
		public static int ShotDamage => (int) (29 + 8 * DifficultyMult);
		public static float ShotCount => 3 + DifficultyMult;
		public static float Spread => 0.5f;
		public static float ShotSpeed => 25;
		public static float MinShotSpeedFactor => 0.7f;
		public static float TurnRate => 0.03f;
		public static int Startup => 60;
		public static int EndLag => 30;
		public static float IndicatorAlpha(int time) => (time <= 5 && time >= 2) ? 0.08f : 0.02f;
		public static float IndicatorLength(int time) => 80;
		public static Color IndicatorColor(int time) => Color.White;
#pragma warning restore IDE0060 // Remove unused parameter
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			boss.Hover();
			NPC npc = boss.NPC;
			GeometryUtils.AngularSmoothing(ref npc.ai[2], (npc.GetTargetData().Center - npc.Center).ToRotation(), TurnRate);
			npc.rotation = npc.ai[2];
			npc.velocity *= 0.93f;
			if (++npc.ai[0] > EndLag) SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[2] = (npc.GetTargetData().Center - npc.Center).ToRotation();
			SoundEngine.PlaySound(SoundID.Item121.WithPitchRange(0.15f, 0.4f), npc.Center);
			npc.SpawnProjectile(null,
				npc.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Shotgun_Indicator>(),
				ShotDamage,
				1,
				Startup,
				Main.rand.Next(1000),
				npc.whoAmI
			);
		}
		public class Shotgun_Indicator : ModProjectile {
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HallowBossRainbowStreak;

			readonly UnifiedRandom rand = new();
			NPC Owner => Main.npc.GetIfInRange((int)Projectile.ai[2]);
			public override void SetDefaults() {
				Projectile.tileCollide = false;
				Projectile.hostile = true;
			}
			public override bool ShouldUpdatePosition() => false;
			IEnumerable<Vector2> GetShots() {
				rand.SetSeed((int)Projectile.ai[1]);
				Vector2 dir = Owner.rotation.ToRotationVector2() * ShotSpeed;
				for (int i = rand.RandomRound(ShotCount); i > 0; i--) {
					yield return dir.RotatedBy(rand.NextFloat(Spread) * (i % 2 == 0).ToDirectionInt()) * rand.NextFloat(MinShotSpeedFactor, 1f);
				}
			}
			public override void AI() {
				if (Owner is not NPC owner || !owner.active) {
					Projectile.Kill();
					return;
				}
				Projectile.Center = owner.Center;
				if (--Projectile.ai[0] <= 0) {
					foreach (Vector2 shot in GetShots()) {
						Projectile.SpawnProjectile(null,
							Projectile.Center,
							shot,
							Shimmer_Construct_Bullet.ID,
							Projectile.damage,
							Projectile.knockBack
						);
					}
					Projectile.Kill();
				}
				owner.ai[0] = 0;
			}
			public override bool PreDraw(ref Color lightColor) {
				float alpha = IndicatorAlpha((int)Projectile.ai[0]);
				foreach (Vector2 shot in GetShots()) {
					float rotation = shot.ToRotation();
					DrawPath(
						[Projectile.Center, Projectile.Center + shot * IndicatorLength((int)Projectile.ai[0])],
						[rotation, rotation],
						8,
						alpha,
						0.5f,
						IndicatorColor((int)Projectile.ai[0])
					);
				}
				return false;
			}
			private static VertexStrip vertexStrip = new();
			public static void DrawPath(Vector2[] positions, float[] rotations, float width, float alpha, float progress, Color color) {
				MiscShaderData shader = GameShaders.Misc["Origins:DefiledIndicator"];
				shader.UseImage1(TextureAssets.Extra[193]);
				shader.UseColor(Color.Black);
				shader.UseSecondaryColor(Color.Green);
				shader.UseShaderSpecificData(new Vector4(alpha, progress, 0, 0));
				shader.Apply();
				vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (p) => color * alpha, (p) => width, -Main.screenPosition, true);
				vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}
		}
	}
	public class SpawnTurretsState : AIState {
		#region stats
		public static float ShotRate => 110 - DifficultyMult * 10f;
		public static int ShotDamage => (int)(15 + 9 * DifficultyMult);
		public static float ShotVelocity => 6;
		public static int HealthAmount => (int)(150 + 90 * DifficultyMult);
		public static float MoveSpeedX => 0.4f;
		public static float MoveSpeedY => 0.6f;
		//will move towards the player if x distance is greater than this
		public static float MoveInXRange => 16 * 40;
		//will move away from the player if x distance is less than this
		public static float MoveOutXRange => 16 * 7;
		//will slide vertically if y distance to player is greater than this
		public static float SlideYRange => 16 * 20;
		#endregion stats
		public override bool Ranged => true;
		static List<int> Types { get; } = [];
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			GeometryUtils.AngularSmoothing(ref npc.rotation, npc.AngleTo(npc.GetTargetData().Center) - MathHelper.PiOver2, 0.3f);
			SoundEngine.PlaySound(SoundID.Item28, npc.Center);
			npc.SpawnNPC(null,
				(int)npc.Center.X,
				(int)npc.Center.Y,
				Main.rand.Next(Types),
				ai0: npc.whoAmI
			);
			SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override double GetWeight(Shimmer_Construct boss, int[] previousStates) {
			int count = 0;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.ModNPC is Shimmer_Construct_Turret_Chunk && ++count >= 2) return 0;
			}
			return Math.Max(base.GetWeight(boss, previousStates) - 0.5f, 0);
		}
		public abstract class Shimmer_Construct_Turret_Chunk : Shimmer_Construct_Health_Chunk {
			public override string Texture => base.Texture.Replace("_Turret", "");
			public override void SetStaticDefaults() {
				NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
				Types.Add(Type);
			}
			public override void AI() {
				if (Main.npc.GetIfInRange((int)NPC.ai[0]) is not NPC owner || !owner.active) {
					NPC.active = false;
					return;
				}
				if (NPC.lifeMax == 1) {
					NPC.lifeMax = HealthAmount;
					NPC.life = NPC.lifeMax;
				}

				Vector2 friction = new(0.95f, 0.95f);
				if (NPC.ai[3] <= 0) {
					Player ownerTarget = Main.player.GetIfInRange(owner.target);

					if (ownerTarget is not null) ownerTarget.aggro -= 800;
					NPC.TargetClosest();
					if (ownerTarget is not null) ownerTarget.aggro += 800;
					int fallDir = 1;
					if (NPC.HasValidTarget) {
						Vector2 diff = NPC.GetTargetData().Center - NPC.Center;
						Vector2 direction = diff.SafeNormalize(Vector2.UnitY);
						if (++NPC.ai[2] > ShotRate) {
							NPC.ai[2] -= ShotRate;
							SoundEngine.PlaySound(SoundID.Item12.WithVolume(0.5f).WithPitchRange(0.25f, 0.4f), NPC.Center);
							NPC.SpawnProjectile(null,
								NPC.Center,
								direction * ShotVelocity,
								Shimmer_Construct_Bullet.ID,
								ShotDamage,
								1
							);
						}
						int moveDir = Math.Sign(diff.X);
						if (Math.Abs(diff.X) < MoveInXRange) {
							if (Math.Abs(diff.X) > MoveOutXRange) {
								moveDir = 0;
							} else {
								moveDir *= -1;
							}
						}
						NPC.velocity.X += moveDir * MoveSpeedX;
						if (diff.Y < 0) {
							fallDir = -1;
						}
						if (Math.Abs(diff.Y) < SlideYRange) friction.Y = 1;
					}
					NPC.velocity.Y += (MathF.Sin(NPC.ai[1]++ / 60) + 1) * MoveSpeedY * fallDir;
				} else {
					NPC.velocity += NPC.DirectionTo(owner.Center) * 2;
					if (NPC.Hitbox.Intersects(owner.Hitbox)) DoStrike();
				}
				NPC.velocity *= friction;
			}
			public override Color? GetAlpha(Color drawColor) => Color.White * NPC.Opacity;
		}
		public class Shimmer_Turret_Chunk1 : Shimmer_Construct_Turret_Chunk { }
		public class Shimmer_Turret_Chunk2 : Shimmer_Construct_Turret_Chunk { }
		public class Shimmer_Turret_Chunk3 : Shimmer_Construct_Turret_Chunk { }
	}
	public class ReverseRainState : AIState {
		#region stats
#pragma warning disable IDE0060 // Remove unused parameter
		public static int ShotDamage => (int)(29 + 4 * DifficultyMult);
		public static int SparkCount => (int)(4 + DifficultyMult * 2);
		public static float ShotSpeed => 16;
		public static float SparkSpeed => Main.rand.NextFloat(8, 12);
		public static float TurnRate => 0.2f;
		public static int Startup => 60;
		public static int EndLag => 30;
#pragma warning restore IDE0060 // Remove unused parameter
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.velocity *= 0.97f;
			boss.Hover();
			Vector2 diff = npc.GetTargetData().Center - npc.Center;
			GeometryUtils.AngularSmoothing(ref npc.rotation, diff.ToRotation(), TurnRate);
			npc.velocity += npc.DirectionTo(npc.GetTargetData().Center - Vector2.UnitY * 16 * 15) * 0.5f;
			npc.velocity *= 0.97f;
			if (++npc.ai[0] > Startup && npc.ai[1].TrySet(1)) {
				SoundEngine.PlaySound(SoundID.Item163, npc.Center);
				npc.SpawnProjectile(null,
					npc.Center,
					(diff + Vector2.UnitY * 16 * 20).SafeNormalize(default) * ShotSpeed,
					ModContent.ProjectileType<SC_Firework>(),
					ShotDamage,
					1,
					-(diff.Length() / ShotSpeed),
					Main.rand.NextFloat(0.5f, 2f)
				);
			}
			if (npc.ai[0] > Startup + EndLag) SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public class SC_Firework : ModProjectile {
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HallowBossRainbowStreak;
			public override void SetStaticDefaults() {
				ProjectileID.Sets.TrailingMode[Type] = 3;
				ProjectileID.Sets.TrailCacheLength[Type] = 30;
			}
			public override void SetDefaults() {
				Projectile.width = 24;
				Projectile.height = 24;
				Projectile.tileCollide = false;
				Projectile.hostile = true;
				Projectile.scale = 1.5f;
			}
			public override void AI() {
				Projectile.velocity = Projectile.velocity.RotatedBy(Math.Clamp(Math.Cos(Projectile.ai[1] * ++Projectile.ai[0] * 0.5f), -0.1f, 0.1f) * Projectile.ai[1]);
				if (!Projectile.IsLocallyOwned()) return;
				if (Main.rand.Next(60, 90) < Projectile.ai[0]) {
					int type = ModContent.ProjectileType<SC_Firework_Spark>();
					for (int i = SparkCount; i > 0; i--) {
						Projectile.SpawnProjectile(Projectile.GetSource_Death(),
							Projectile.Center,
							Main.rand.NextVector2CircularEdge(1, 1) * SparkSpeed,
							type,
							ShotDamage,
							1,
							ai1: Main.rand.NextFloat(0.5f, 1.5f)
						);
					}
					Projectile.Kill();
				}
			}
			public override void OnKill(int timeLeft) {
				SoundEngine.PlaySound(SoundID.Item176, Projectile.Center);
				SoundEngine.PlaySound(SoundID.Zombie83.WithPitch(-2f), Projectile.Center);
				SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolume(0.8f), Projectile.Center);
			}
			public override bool PreDraw(ref Color lightColor) {
				Shimmerstar_Staff_P.DrawShimmerstar(Projectile);
				return false;
			}
		}
		public class SC_Firework_Spark : ModProjectile {
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HallowBossRainbowStreak;
			public override void SetStaticDefaults() {
				ProjectileID.Sets.TrailingMode[Type] = 3;
				ProjectileID.Sets.TrailCacheLength[Type] = 30;
			}
			public override void SetDefaults() {
				Projectile.width = 24;
				Projectile.height = 24;
				Projectile.tileCollide = false;
				Projectile.hostile = true;
			}
			public override void AI() {
				Projectile.velocity *= 0.98f;
				Projectile.velocity.Y -= 0.2f;
				Projectile.velocity = Projectile.velocity.RotatedBy(Math.Clamp(Math.Cos(Projectile.ai[1] * ++Projectile.ai[0] * 0.5f), -0.1f, 0.1f) * Projectile.ai[1]);
				if (!Projectile.IsLocallyOwned()) return;
				if (Main.rand.Next(240, 360) < Projectile.ai[0]) {
					Projectile.Kill();
				}
			}
			public override bool PreDraw(ref Color lightColor) {
				Shimmerstar_Staff_P.DrawShimmerstar(Projectile);
				return false;
			}
		}
	}
	public class Weak_Shimmer_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Shimmer;
		public static int ID { get; private set; }
		public override void Load() {
			On_Player.ShimmerCollision += (On_Player.orig_ShimmerCollision orig, Player self, bool fallThrough, bool ignorePlats, bool noCollision) => {
				if (self.OriginPlayer().weakShimmer) {
					self.position += self.velocity * new Vector2(0.75f, self.controlDown ? 0.75f : (self.controlUp ? 0.3f : 0.5f));
				} else {
					orig(self, fallThrough, ignorePlats, noCollision);
				}
			};
			On_Player.TryLandingOnDetonator += (orig, self) => {
				if (self.OriginPlayer().weakShimmer) {
					Collision.up = false;
					Collision.down = false;
				} else {
					orig(self);
				}
			};
			try {
				IL_Projectile.HandleMovement += IL_Projectile_HandleMovement;
			} catch (Exception ex) {
				if (Origins.LogLoadingILError(nameof(IL_Projectile_HandleMovement), ex)) throw;
			}
			On_Projectile.Update += [DebuggerStepThrough](orig, self, i) => {
				isUpdatingShimmeryThing = (self.TryGetGlobalProjectile(out OriginGlobalProj proj) && proj.weakShimmer);
				try {
					orig(self, i);
				} catch {
					isUpdatingShimmeryThing = false;
					throw;
				}
				isUpdatingShimmeryThing = false;
			};
			On_Collision.CanHitLine += (orig, Position1, Width1, Height1, Position2, Width2, Height2) => isUpdatingShimmeryThing || orig(Position1, Width1, Height1, Position2, Width2, Height2);
			On_Collision.CanHitWithCheck += (orig, Position1, Width1, Height1, Position2, Width2, Height2, check) => isUpdatingShimmeryThing || orig(Position1, Width1, Height1, Position2, Width2, Height2, check);
			On_Collision.CanHit_Entity_Entity += (orig, a, b) => isUpdatingShimmeryThing || orig(a, b);
			On_Collision.CanHit_Entity_NPCAimedTarget += (orig, a, b) => isUpdatingShimmeryThing || orig(a, b);
			On_Collision.CanHit_Point_int_int_Point_int_int += (orig, a, aw, ah, b, bw, bh) => isUpdatingShimmeryThing || orig(a, aw, ah, b, bw, bh);
			On_Collision.CanHit_Vector2_int_int_Vector2_int_int += (orig, a, aw, ah, b, bw, bh) => isUpdatingShimmeryThing || orig(a, aw, ah, b, bw, bh);
			MonoModHooks.Add(typeof(CollisionExt).GetMethod(nameof(CollisionExt.Raymarch), [typeof(Vector2), typeof(Vector2), typeof(Predicate<Tile>), typeof(float)]), (Func<Vector2, Vector2, Predicate<Tile>, float, float> orig, Vector2 position, Vector2 direction, Predicate<Tile> extraCheck, float maxLength) => {
				if (isUpdatingShimmeryThing) return maxLength;
				return orig(position, direction, extraCheck, maxLength);
			});
			MonoModHooks.Add(typeof(CollisionExt).GetMethod(nameof(CollisionExt.Raymarch), [typeof(Vector2), typeof(Vector2), typeof(float)]), (Func<Vector2, Vector2, float, float> orig, Vector2 position, Vector2 direction, float maxLength) => {
				if (isUpdatingShimmeryThing) return maxLength;
				return orig(position, direction, maxLength);
			});
			On_Collision.TileCollision += (orig, Position, Velocity, Width, Height, fallThrough, fall2, gravDir) => isUpdatingShimmeryThing ? Velocity : orig(Position, Velocity, Width, Height, fallThrough, fall2, gravDir);
			On_Collision.AdvancedTileCollision += (orig, forcedIgnoredTiles, Position, Velocity, Width, Height, fallThrough, fall2, gravDir) => isUpdatingShimmeryThing ? Velocity : orig(forcedIgnoredTiles, Position, Velocity, Width, Height, fallThrough, fall2, gravDir);
			On_Collision.SlopeCollision += (orig, Position, Velocity, Width, Height, fallThrough, gravDir) => isUpdatingShimmeryThing ? new Vector4(Position, Velocity.X, Velocity.Y) : orig(Position, Velocity, Width, Height, fallThrough, gravDir);
			On_Collision.SolidCollision_Vector2_int_int += (orig, Position, Width, Height) => !isUpdatingShimmeryThing && orig(Position, Width, Height);
			On_Collision.WetCollision += (orig, Position, Width, Height) => {
				Collision.honey = false;
				Collision.shimmer = false;
				return !isUpdatingShimmeryThing && orig(Position, Width, Height);
			};
			On_Collision.LavaCollision += (orig, Position, Width, Height) => !isUpdatingShimmeryThing && orig(Position, Width, Height);
			try {
				IL_PlayerDrawLayers.DrawPlayer_27_HeldItem += DrawPlayer_27_HeldItem;
			} catch (Exception ex) {
				if (Origins.LogLoadingILError(nameof(DrawPlayer_27_HeldItem), ex)) throw;
			}
			On_PlayerDrawLayers.DrawHeldProj += (On_PlayerDrawLayers.orig_DrawHeldProj orig, PlayerDrawSet drawinfo, Projectile proj) => {
				isDrawingShimmeryThing = drawinfo.drawPlayer?.OriginPlayer()?.weakShimmer ?? false;
				orig(drawinfo, proj);
				isDrawingShimmeryThing = false;
			};
			On_LegacyLighting.GetColor += (On_LegacyLighting.orig_GetColor orig, LegacyLighting self, int x, int y) => {
				if (isDrawingShimmeryThing) return Vector3.One;
				return orig(self, x, y);
			};
			On_LightingEngine.GetColor += (On_LightingEngine.orig_GetColor orig, LightingEngine self, int x, int y) => {
				if (isDrawingShimmeryThing) return Vector3.One;
				return orig(self, x, y);
			};
		}

		static bool isDrawingShimmeryThing = false;
		internal static bool isUpdatingShimmeryThing = false;

		static void IL_Projectile_HandleMovement(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchLdfld<Projectile>(nameof(Projectile.tileCollide))
			);
			c.EmitLdarg0();
			c.EmitDelegate((bool tileCollide, Projectile projectile) => tileCollide && !(projectile.TryGetGlobalProjectile(out OriginGlobalProj proj) && proj.weakShimmer));
		}
		static void DrawPlayer_27_HeldItem(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before,
				i => i.MatchStfld<PlayerDrawSet>(nameof(PlayerDrawSet.itemColor))
			);
			c.EmitLdarg0();
			c.EmitDelegate((Color color, ref PlayerDrawSet drawInfo) => drawInfo.drawPlayer.OriginPlayer().weakShimmer ? drawInfo.colorArmorBody : color);
		}

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.mount?.Active == true) {
				Mount mount = player.mount;
				player.ClearBuff(mount._data.buff);

				mount._mountSpecificData = null;

				if (mount.Cart) {
					player.ClearBuff(mount._data.extraBuff);
					player.cartFlip = false;
					player.lastBoost = Vector2.Zero;
				}

				player.fullRotation = 0f;
				player.fullRotationOrigin = Vector2.Zero;

				mount.Reset();
				player.position.Y += player.height;
				player.height = 42;
				player.position.Y -= player.height;
				if (player.whoAmI == Main.myPlayer)
					NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
			}
			if (player.grapCount > 0) player.RemoveAllGrapplingHooks();
			player.buffImmune[BuffID.Shimmer] = true;
			player.shimmering = true;
			player.OriginPlayer().weakShimmer = true;
			player.fallStart = (int)(player.position.Y / 16f);
			if (player.buffTime[buffIndex] > 2) {
				player.timeShimmering = 0;
			} else {
				bool isBlocked = false;
				for (int i = (int)(player.position.X / 16f); i <= (player.position.X + player.width) / 16; i++) {
					for (int j = (int)(player.position.Y / 16f); j <= (player.position.Y + player.height) / 16; j++) {
						if (WorldGen.SolidTile3(i, j))
							isBlocked = true;
					}
				}

				if (isBlocked) {
					player.buffTime[buffIndex]++;
				} else {
					new Remove_Shimmer_Action(player).Perform();
				}
			}
			if (player.whoAmI == Main.myPlayer && !SoundEngine.TryGetActiveSound(ambienceSlot, out _)) {
				bool UpdateCallback(ActiveSound sound) {
					MathUtils.LinearSmoothing(ref sound.Volume, OriginPlayer.LocalOriginPlayer.weakShimmer.ToInt(), 1f / (60 * 5));
					unchecked {
						startLoopUntil = Origins.gameFrameCount + 10;
					}
					return sound.Volume > 0;
				}
				if (Origins.gameFrameCount < startLoopUntil) {
					ambienceSlot = SoundEngine.PlaySound(Origins.Sounds.ShimmerConstructAmbienceLoop, updateCallback: UpdateCallback);
					if (playOutro) SoundEngine.PlaySound(Origins.Sounds.ShimmerConstructAmbienceOutro);
					playOutro = true;
				} else {
					ambienceSlot = SoundEngine.PlaySound(Origins.Sounds.ShimmerConstructAmbienceIntro, updateCallback: UpdateCallback);
					if (SoundEngine.TryGetActiveSound(ambienceSlot, out ActiveSound sound)) sound.Volume = 0;
					playOutro = false;
				}
			}
		}
		uint startLoopUntil = 0;
		bool playOutro = false;
		SlotId ambienceSlot;
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().lazyCloakShimmer = true;
			bool isBlocked = false;
			for (int i = (int)(npc.position.X / 16f); i <= (npc.position.X + npc.width) / 16; i++) {
				for (int j = (int)(npc.position.Y / 16f); j <= (npc.position.Y + npc.height) / 16; j++) {
					if (WorldGen.SolidTile3(i, j))
						isBlocked = true;
				}
			}

			if (isBlocked) {
				npc.buffTime[buffIndex]++;
			} else {
				npc.DelBuff(buffIndex--);
			}
		}
	}
	public class Cheap_SC_Phase_Three_Underlay() : Overlay(EffectPriority.High, RenderLayers.Walls) {
		bool active = false;
		public override void Activate(Vector2 position, params object[] args) => active = true;
		public override void Deactivate(params object[] args) => active = false;
		public override void Draw(SpriteBatch spriteBatch) {
			float brightness = 0.8f * Opacity;
			float alpha = 1f * Opacity;
			spriteBatch.Draw(
				SC_Phase_Three_Overlay.CurrentBG,
				new Rectangle(0, 0, Main.ScreenSize.X, Main.ScreenSize.Y),
				null,
				new(brightness, brightness, brightness, alpha),
				0,
				Vector2.Zero,
				Main.GameViewMatrix.Effects,
			0);
			SC_Phase_Three_Underlay.alwaysLightAllTiles = true;
		}
		public override bool IsVisible() => !Lighting.NotRetro;
		public override void Update(GameTime gameTime) {
			if (Lighting.NotRetro) return;
			SC_Phase_Three_Overlay overlay = ModContent.GetInstance<SC_Phase_Three_Overlay>();
			if (!overlay.IsVisible()) overlay.Update(gameTime);
			Mode = active ? (Opacity >= 1 ? OverlayMode.Active : OverlayMode.FadeIn) : OverlayMode.FadeOut;
			SC_Phase_Three_Underlay.alwaysLightAllTiles = false;
		}
	}
	public class SC_Phase_Three_Underlay() : SC_Phase_Three_BG_Layer(RenderLayers.Walls) {
		public static List<DrawData> DrawDatas => instance.drawDatas;
		public static HashSet<object> DrawnMaskSources => instance.drawnMaskSources;

		public static SC_Phase_Three_Underlay instance;
		public override void Load() => instance = this;
		public override void Draw(SpriteBatch spriteBatch) {
			alwaysLightAllTiles = false;
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			if (spriteBatch is null) return;
			SC_Scene_Effect effect = ModContent.GetInstance<SC_Scene_Effect>();
			effect.AddArea();
			effect.DoMonolith();
			//if (drawDatas.Count > 0) alwaysLightAllTiles = true;
			base.Draw(spriteBatch);
		}
		public static bool alwaysLightAllTiles = false;
		public static void AddMinLightArea(Vector2 pos, float range) {
			range += 16;
			minLightAreas.Add((pos, range));
		}
		internal static List<(Vector2 pos, float range)> minLightAreas = [];
		public static bool ForcedLit(int i, int j) {
			for (int k = 0; k < minLightAreas.Count; k++) {
				if (minLightAreas[k].pos.WithinRange(new Vector2(i * 16 + 8, j * 16 + 8), minLightAreas[k].range)) return true;
			}
			return false;
		}
	}
	public class SC_Phase_Three_Midlay() : SC_Phase_Three_BG_Layer(RenderLayers.TilesAndNPCs) {
		public static List<DrawData> DrawDatas => instance.drawDatas;
		public static HashSet<object> DrawnMaskSources => instance.drawnMaskSources;

		public static SC_Phase_Three_Midlay instance;
		public override void Load() => instance = this;
		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
		}
	}
	public abstract class SC_Phase_Three_BG_Layer(RenderLayers layer) : Overlay(EffectPriority.High, layer), ILoadable {
		readonly ArmorShaderData simpleMaskShader = new(ModContent.Request<Effect>("Origins/Effects/ShimmerConstruct"), "SimpleMask");
		public override void Activate(Vector2 position, params object[] args) => Mode = OverlayMode.Active;
		public override void Deactivate(params object[] args) {
			Mode = OverlayMode.FadeOut;
			Opacity = 0;
		}
		public List<DrawData> drawDatas = [];
		public HashSet<object> drawnMaskSources = [];
		public virtual float Brightness => 0.8f;
		public virtual float Alpha => 1f;
		public override void Draw(SpriteBatch spriteBatch) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			if (spriteBatch is null) return;
			layersRenderedThisFrame[index] = true;
			SpriteBatchState state = spriteBatch.GetState() with { rasterizerState = RasterizerState.CullNone };

			RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
			try {
				spriteBatch.End();
				Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
				spriteBatch.Begin(state);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);
				for (int i = 0; i < drawDatas.Count; i++) {
					drawDatas[i].Draw(spriteBatch);
				}
			} finally {
				spriteBatch.End();
				spriteBatch.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);
				spriteBatch.Begin(state);
			}
			drawDatas.Clear();
			drawnMaskSources.Clear();

			Origins.shaderOroboros.Capture(spriteBatch);
			Main.spriteBatch.Restart(Main.spriteBatch.GetState(), transformMatrix: Main.GameViewMatrix.EffectMatrix);
			spriteBatch.Draw(
				SC_Phase_Three_Overlay.CurrentBG,
				new Rectangle(0, 0, Main.ScreenSize.X, Main.ScreenSize.Y),
				null,
				new(Brightness, Brightness, Brightness, Alpha),
				0,
				Vector2.Zero,
				Main.GameViewMatrix.Effects,
			0);
			Main.graphics.GraphicsDevice.Textures[1] = renderTarget;
			Origins.shaderOroboros.Stack(simpleMaskShader);
			Origins.shaderOroboros.Release();
		}
		public override bool IsVisible() => Lighting.NotRetro && ModContent.GetInstance<SC_Phase_Three_Overlay>().IsVisible();
		public override void Update(GameTime gameTime) { }
		static readonly List<SC_Phase_Three_BG_Layer> layers = [];
		static readonly List<bool> layersRenderedThisFrame = [];
		internal static void ClearRenderedLayerTracker() {
			for (int i = 0; i < layers.Count; i++) {
				layersRenderedThisFrame[i] = false;
			}
		}
		public static IEnumerable<SC_Phase_Three_BG_Layer> GetActiveLayers() {
			for (int i = 0; i < layers.Count; i++) {
				if (layersRenderedThisFrame[i]) yield return layers[i];
			}
		}
		int index;
		public void Load(Mod mod) {
			Load();
			index = layers.Count;
			layers.Add(this);
			layersRenderedThisFrame.Add(false);
		}
		public new abstract void Load();
		public void Unload() {
			if (renderTarget is not null) {
				Main.QueueMainThreadAction(renderTarget.Dispose);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			SetupRenderTargets();
		}
		protected void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
	public class SC_Phase_Three_Overlay() : Overlay(EffectPriority.High, RenderLayers.ForegroundWater), ILoadable {
		readonly Asset<Texture2D> texture = ModContent.Request<Texture2D>("Origins/Textures/Shimmer_Construct_BG");
		readonly ArmorShaderData invertAnimateShader = new(ModContent.Request<Effect>("Origins/Effects/ShimmerConstruct"), "InvertAnimate");
		readonly ArmorShaderData maskShader = new(ModContent.Request<Effect>("Origins/Effects/ShimmerConstruct"), "Mask");
		internal bool active = false;
		float opacity;
		static readonly BlendState realAlphaSourceBlend = new() {
			ColorSourceBlend = Blend.One,
			AlphaSourceBlend = Blend.One,
			ColorDestinationBlend = Blend.InverseSourceAlpha,
			AlphaDestinationBlend = Blend.InverseSourceAlpha
		};
		public override void Draw(SpriteBatch spriteBatch) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			if (spriteBatch is null) return;
			SC_Phase_Three_BG_Layer[] layers = SC_Phase_Three_BG_Layer.GetActiveLayers().ToArray();
			if (layers.Length <= 0) return;
			SpriteBatchState state = spriteBatch.GetState();
			Texture2D maskTexture;
			if (layers.Length <= 1) {
				maskTexture = layers[0].renderTarget;
			} else {
				Origins.shaderOroboros.Capture(spriteBatch);
				spriteBatch.Restart(state, blendState: realAlphaSourceBlend, sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap, transformMatrix: Matrix.Identity);
				for (int i = 0; i < layers.Length; i++) {
					if (layers[i].renderTarget is null) continue;
					spriteBatch.Draw(layers[i].renderTarget, Vector2.Zero, Color.White);
				}
				Origins.shaderOroboros.DrawContents(renderTarget);
				Origins.shaderOroboros.Reset(default);
				maskTexture = renderTarget;
			}
			Origins.shaderOroboros.Capture(spriteBatch);
			spriteBatch.Restart(state, blendState: realAlphaSourceBlend, sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.AnisotropicWrap);
			Vector2 size = new(texture.Width() * (int)MathF.Ceiling(Main.screenWidth / (float)texture.Width()), texture.Height() * (int)MathF.Ceiling(Main.screenHeight / (float)texture.Height()));
			const int scale = 4;
			Rectangle frame = new(0, 0, (int)size.X, (int)size.Y);
			spriteBatch.Draw(
				texture.Value,
				-new Vector2(Main.screenPosition.X % (size.X * scale * 0.5f), Main.screenPosition.Y % (size.Y * scale * 0.5f)),
				frame,
				Color.White,
				0,
				Vector2.Zero,
				scale,
				SpriteEffects.None,
			0);
			ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(Shimmer_Dye.ShaderID, null);
			invertAnimateShader.Shader.Parameters["uFullColor"].SetValue(new Vector4(opacity));
			Origins.shaderOroboros.Stack(invertAnimateShader);
			Origins.shaderOroboros.Stack(shader);
			const float brightness = 0.1f;
			const float alpha = 0.1f;
			maskShader.Shader.Parameters["uFullColor"].SetValue(new Vector4(brightness, brightness, brightness, alpha));
			Main.graphics.GraphicsDevice.Textures[1] = maskTexture;
			Origins.shaderOroboros.Stack(maskShader);
			Origins.shaderOroboros.Release();

			SC_Phase_Three_BG_Layer.ClearRenderedLayerTracker();

			for (int i = 0; i < renderTargetsToDispose.Count; i++) {
				renderTargetsToDispose[i].Dispose();
			}
			renderTargetsToDispose.Clear();
		}
		internal static readonly List<RenderTarget2D> renderTargetsToDispose = [];
		public static void SendRenderTargetForDisposal(ref RenderTarget2D renderTarget) {
			renderTargetsToDispose.Add(renderTarget);
			renderTarget = null;
		}
		private double SurfaceFrameCounter;
		private int SurfaceFrame = 6;
		private int pingpongCounter = 1;
		private Asset<Texture2D>[] sc_BGs;
		private const int bgsAmount = 30;
		public static Texture2D CurrentBG => instance.sc_BGs[instance.SurfaceFrame].Value;
		public override void Update(GameTime gameTime) {
			SurfaceFrameCounter += Math.Round(gameTime.ElapsedGameTime.Divide(TimeSpan.FromSeconds(1 / 60d)));
			const double frames_per_frame = 2;
			while (SurfaceFrameCounter >= frames_per_frame) {
				// remove the first 5 frame since it makes me want to throw up 
				if (SurfaceFrame == 5 || SurfaceFrame + 1 > bgsAmount - 1)
					pingpongCounter *= -1;
				SurfaceFrame += pingpongCounter;
				SurfaceFrameCounter -= frames_per_frame;
			}
		}

		public override void Activate(Vector2 position, params object[] args) {
			Mode = OverlayMode.Active;
			if (active.TrySet(true)) opacity = Main.rand.NextFloat(0.6f, 0.85f);
		}
		public override void Deactivate(params object[] args) {
			active = false;
			Mode = OverlayMode.FadeOut;
			Opacity = 0;
		}
		public override bool IsVisible() => active;
		public static SC_Phase_Three_Overlay instance;
		public void Load(Mod mod) {
			instance = this;
			sc_BGs = new Asset<Texture2D>[bgsAmount];
			for (int i = 1; i < bgsAmount; i++) {
				sc_BGs[i] = ModContent.Request<Texture2D>("Origins/Textures/Backgrounds/SC_BG/SC_BG" + i);
			}
		}
		public void Unload() {
			for (int i = 0; i < renderTargetsToDispose.Count; i++) {
				renderTargetsToDispose[i].Dispose();
			}
			renderTargetsToDispose.Clear();
			if (renderTarget is not null) {
				Main.QueueMainThreadAction(renderTarget.Dispose);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
	public record class Remove_Shimmer_Action(Player Player) : SyncedAction {
		public override bool ServerOnly => true;
		public Remove_Shimmer_Action() : this(default(Player)) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Player = Main.player[reader.ReadByte()]
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)Player.whoAmI);
		}
		protected override void Perform() {
			Player.ClearBuff(Weak_Shimmer_Debuff.ID);
		}
	}
}
