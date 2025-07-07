using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;
using Terraria.ModLoader;
using MonoMod.Cil;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib.Graphics;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Origins.Items.Other.Dyes;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using Origins.Reflection;
using PegasusLib;
using Origins.Items.Weapons.Magic;
using Terraria.Utilities;
using Origins.Projectiles;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Audio;
using static Origins.NPCs.MiscB.Shimmer_Construct.SpawnTurretsState;

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
			aiStates.Add(ModContent.GetInstance<SpawnCloudsState>());
			aiStates.Add(ModContent.GetInstance<MagicMissilesState>());
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.TargetClosest();
			npc.velocity += npc.DirectionTo(npc.GetTargetData().Center - Vector2.UnitY * 16 * 15) * 0.5f;
			npc.velocity *= 0.97f;
			if (++npc.ai[0] > (60 - ContentExtensions.DifficultyDamageMultiplier * 10) && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.previousStates.Contains)) Array.Fill(boss.previousStates, Index);
				SelectAIState(boss, aiStates);
			}
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
		/// I'm not actually 100% sure what unit this is in
		/// </summary>
		public static float Density => 16 * (27 - DifficultyMult * 2);
		#endregion stats
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		Stack<Vector2> positions = new();
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			Vector2 targetPos = npc.GetTargetData().Center;
			npc.velocity += npc.DirectionTo(targetPos - Vector2.UnitY * 16 * 15) * 0.5f;
			for (int i = 0; i < 2; i++) {
				SoundEngine.PlaySound(SoundID.Item88);
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
				List<Vector2> newPositions = OriginExtensions.PoissonDiskSampling(Main.rand, area, Density);
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
				Projectile.damage = 48/* + (4 * difficultyMult)*/;
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
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
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
		static List<int> Types { get; } = [];
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			SoundEngine.PlaySound(SoundID.Item28);
			NPC npc = boss.NPC;
			npc.SpawnNPC(null,
				(int)npc.Center.X,
				(int)npc.Center.Y,
				Main.rand.Next(Types),
				ai0: npc.whoAmI
			);
			SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override double GetWeight(Shimmer_Construct boss, int[] previousStates) {
			int turretType = ModContent.NPCType<Shimmer_Construct_Turret_Chunk>();
			int count = 0;
			foreach (NPC proj in Main.ActiveNPCs) {
				if (proj.type == turretType && ++count >= 2) return 0;
			}
			return Math.Max(base.GetWeight(boss, previousStates) - 0.5f, 0);
		}
		public abstract class Shimmer_Construct_Turret_Chunk : Shimmer_Construct_Health_Chunk {
			public override string Texture => base.Texture.Replace("_Turret", "");
			public override void SetStaticDefaults() {
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
				orig(self);
				if (self.OriginPlayer().weakShimmer) {
					Collision.up = false;
					Collision.down = false;
				}
			};
			try {
				IL_Projectile.HandleMovement += IL_Projectile_HandleMovement;
			} catch (Exception ex) {
				if (Origins.LogLoadingILError(nameof(IL_Projectile_HandleMovement), ex)) throw;
			}
			On_Projectile.Update += (orig, self, i) => {
				isUpdatingShimmeryProj = (self.TryGetGlobalProjectile(out OriginGlobalProj proj) && proj.weakShimmer);
				try {
					orig(self, i);
				} catch {
					isUpdatingShimmeryProj = false;
					throw;
				}
				isUpdatingShimmeryProj = false;
			};
			On_Collision.CanHitLine += (orig, Position1, Width1, Height1, Position2, Width2, Height2) => isUpdatingShimmeryProj || orig(Position1, Width1, Height1, Position2, Width2, Height2);
			On_Collision.CanHitWithCheck += (orig, Position1, Width1, Height1, Position2, Width2, Height2, check) => isUpdatingShimmeryProj || orig(Position1, Width1, Height1, Position2, Width2, Height2, check);
			On_Collision.CanHit_Entity_Entity += (orig, a, b) => isUpdatingShimmeryProj || orig(a, b);
			On_Collision.CanHit_Entity_NPCAimedTarget += (orig, a, b) => isUpdatingShimmeryProj || orig(a, b);
			On_Collision.CanHit_Point_int_int_Point_int_int += (orig, a, aw, ah, b, bw, bh) => isUpdatingShimmeryProj || orig(a, aw, ah, b, bw, bh);
			On_Collision.CanHit_Vector2_int_int_Vector2_int_int += (orig, a, aw, ah, b, bw, bh) => isUpdatingShimmeryProj || orig(a, aw, ah, b, bw, bh);
			MonoModHooks.Add(typeof(CollisionExt).GetMethod(nameof(CollisionExt.Raymarch), [typeof(Vector2), typeof(Vector2), typeof(Predicate<Tile>), typeof(float)]), (Func<Vector2, Vector2, Predicate<Tile>, float, float> orig, Vector2 position, Vector2 direction, Predicate<Tile> extraCheck, float maxLength) => {
				if (isUpdatingShimmeryProj) return maxLength;
				return orig(position, direction, extraCheck, maxLength);
			});
			MonoModHooks.Add(typeof(CollisionExt).GetMethod(nameof(CollisionExt.Raymarch), [typeof(Vector2), typeof(Vector2), typeof(float)]), (Func<Vector2, Vector2, float, float> orig, Vector2 position, Vector2 direction, float maxLength) => {
				if (isUpdatingShimmeryProj) return maxLength;
				return orig(position, direction, maxLength);
			});
			On_Collision.TileCollision += (orig, Position, Velocity, Width, Height, fallThrough, fall2, gravDir) => isUpdatingShimmeryProj ? Velocity : orig(Position, Velocity, Width, Height, fallThrough, fall2, gravDir);
			On_Collision.AdvancedTileCollision += (orig, forcedIgnoredTiles, Position, Velocity, Width, Height, fallThrough, fall2, gravDir) => isUpdatingShimmeryProj ? Velocity : orig(forcedIgnoredTiles, Position, Velocity, Width, Height, fallThrough, fall2, gravDir);
			On_Collision.SlopeCollision += (orig, Position, Velocity, Width, Height, fallThrough, gravDir) => isUpdatingShimmeryProj ? new Vector4(Position, Velocity.X, Velocity.Y) : orig(Position, Velocity, Width, Height, fallThrough, gravDir);
		}

		static bool isUpdatingShimmeryProj = false;

		static void IL_Projectile_HandleMovement(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchLdfld<Projectile>(nameof(Projectile.tileCollide))
			);
			c.EmitLdarg0();
			c.EmitDelegate((bool tileCollide, Projectile projectile) => tileCollide && !(projectile.TryGetGlobalProjectile(out OriginGlobalProj proj) && proj.weakShimmer));
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
					player.DelBuff(buffIndex--);
				}
			}
		}
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
	public class SC_Phase_Three_Overlay() : Overlay(EffectPriority.High, RenderLayers.ForegroundWater), ILoadable {
		readonly Asset<Texture2D> texture = ModContent.Request<Texture2D>("Origins/Textures/Shimmer_Construct_BG");
		readonly ArmorShaderData invertAnimateShader = GameShaders.Armor.BindShader(ItemID.HallowedBar, new ArmorShaderData(ModContent.Request<Effect>("Origins/Effects/ShimmerConstruct"), "InvertAnimate"));
		readonly ArmorShaderData maskShader = GameShaders.Armor.BindShader(ItemID.AdamantiteMask, new ArmorShaderData(ModContent.Request<Effect>("Origins/Effects/ShimmerConstruct"), "Mask"));
		readonly ArmorShaderData simpleMaskShader = GameShaders.Armor.BindShader(ItemID.BeeMask, new ArmorShaderData(ModContent.Request<Effect>("Origins/Effects/ShimmerConstruct"), "SimpleMask"));
		readonly List<Player> players = new(255);
		bool active = false;
		float opacity;
		public static List<DrawData> drawDatas = [];
		public static HashSet<object> drawnMaskSources = [];
		static readonly BlendState realAlphaSourceBlend = new() {
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,
				ColorDestinationBlend = Blend.InverseSourceAlpha,
				AlphaDestinationBlend = Blend.InverseSourceAlpha
			};
		private int SurfaceFrameCounter;
		private int SurfaceFrame = 6;
		private int pingpongCounter = 1;
		private Asset<Texture2D>[] sc_BGs;
		private const int bgsAmount = 30;
		public override void Draw(SpriteBatch spriteBatch) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			if (spriteBatch is null) return;
			ModContent.GetInstance<SC_Scene_Effect>().AddArea();
			if (++SurfaceFrameCounter > 1) {
				// remove the first 5 frame since it makes me want to throw up 
				if (SurfaceFrame == 5 || SurfaceFrame + 1 > bgsAmount - 1)
					pingpongCounter *= -1;
				SurfaceFrame += pingpongCounter;
				SurfaceFrameCounter = 0;
			}
			SpriteBatchState state = spriteBatch.GetState();
			if (!Main.gamePaused) {
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				try {
					spriteBatch.Restart(state);
					Main.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
					Main.graphics.GraphicsDevice.Clear(Color.Transparent);
					for (int i = 0; i < drawDatas.Count; i++) {
						drawDatas[i].Draw(spriteBatch);
					}
				} finally {
					spriteBatch.End();
					spriteBatch.UseOldRenderTargets(oldRenderTargets);
					spriteBatch.Begin(state);
				}
				drawDatas.Clear();
				drawnMaskSources.Clear();
			}
			if (drawDatas.Count > 100) drawDatas.RemoveRange(0, drawDatas.Count - 99);
			Origins.shaderOroboros.Capture(spriteBatch);
			spriteBatch.Draw(sc_BGs[SurfaceFrame].Value, new Rectangle(0, 0, Main.ScreenSize.X, Main.ScreenSize.Y), new(1f, 1f, 1f, 1f));
			Main.graphics.GraphicsDevice.Textures[1] = renderTarget;
			Origins.shaderOroboros.Stack(simpleMaskShader);
			Origins.shaderOroboros.Release();
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
			maskShader.Shader.Parameters["uFullColor"].SetValue(new Vector4(new(0.15f), 0.7f));
			Main.graphics.GraphicsDevice.Textures[1] = renderTarget;
			Origins.shaderOroboros.Stack(maskShader);
			Origins.shaderOroboros.Release();

			static void DrawCachedProjectiles(List<int> projCache) {
				Main.CurrentDrawnEntity = null;
				Main.CurrentDrawnEntityShader = 0;
				for (int i = 0; i < projCache.Count; i++) {
					try {
						Main.instance.DrawProj(projCache[i]);
					} catch (Exception e) {
						TimeLogger.DrawException(e);
						Main.projectile[projCache[i]].active = false;
					}
				}
				Main.CurrentDrawnEntity = null;
				Main.CurrentDrawnEntityShader = 0;
			}
			DrawCachedProjectiles(DrawCacheProjsBehindProjectiles);
			DrawCachedProjectiles(DrawCacheProjsBehindNPCs);
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (shimmeryNormalProjs[proj.whoAmI]) Main.instance.DrawProj(proj.whoAmI);
			}
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.ModNPC is Shimmer_Construct or Shimmer_Drone or Shimmer_Construct_Turret_Chunk) {
					Main.instance.DrawNPCDirect(spriteBatch, npc, false, Main.screenPosition);
				}
			}
			spriteBatch.End();
			for (int i = 0; i < players.Count; i++) {
				Lighting.AddLight(players[i].Center, new Vector3(1));
			}
			Main.PlayerRenderer.DrawPlayers(Main.Camera, players);
			spriteBatch.Begin(state);
			DrawCachedProjectiles(DrawCacheProjsOverPlayers);
			spriteBatch.End();
			try {
				drawingDust = true;
				PegasusLib.Reflection.DelegateMethods._target.SetValue(MainReflection.DrawDust, Main.instance);
				MainReflection.DrawDust();
			} finally {
				drawingDust = false;
			}
			spriteBatch.Begin(state);
		}
		public override void Update(GameTime gameTime) { }
		public override void Activate(Vector2 position, params object[] args) {
			Mode = OverlayMode.Active;
			if (active.TrySet(true)) opacity = Main.rand.NextFloat(0.6f, 0.85f);
		}
		public override void Deactivate(params object[] args) {
			active = false;
			Mode = OverlayMode.Inactive;
		}
		public override bool IsVisible() => active;
		List<int> DrawCacheProjsBehindProjectiles = new(1000);
		List<int> DrawCacheProjsBehindNPCs = new(1000);
		List<int> DrawCacheProjsOverPlayers = new(1000);
		bool[] projOwners = new bool[Main.maxPlayers];
		bool[] shimmeryNormalProjs = new bool[Main.maxProjectiles];

		readonly FastFieldInfo<Main, List<Player>> _playersThatDrawAfterProjectiles = "_playersThatDrawAfterProjectiles";
		public void Load(Mod mod) {
			On_Main.CacheProjDraws += (orig, self) => {
				orig(self);
				Array.Clear(shimmeryNormalProjs);
				if (active) {
					static void PluckProjectiles(List<int> source, List<int> bucket) {
						bucket.Clear();
						for (int i = source.Count - 1; i >= 0; i--) {
							Projectile projectile = Main.projectile[source[i]];
							if (projectile.hostile || projectile.GetGlobalProjectile<OriginGlobalProj>().weakShimmer) {
								bucket.Add(source[i]);
								source.RemoveAt(i);
							}
						}
					}
					PluckProjectiles(Main.instance.DrawCacheProjsBehindProjectiles, DrawCacheProjsBehindProjectiles);
					PluckProjectiles(Main.instance.DrawCacheProjsBehindNPCs, DrawCacheProjsBehindNPCs);
					PluckProjectiles(Main.instance.DrawCacheProjsOverPlayers, DrawCacheProjsOverPlayers);
					foreach (Projectile projectile in Main.ActiveProjectiles) {
						if ((!projectile.hide || DrawCacheProjsBehindNPCs.Contains(projectile.whoAmI)) && (projectile.hostile || projectile.GetGlobalProjectile<OriginGlobalProj>().weakShimmer)) {
							shimmeryNormalProjs[projectile.whoAmI] = true;
						}
					}
				}
			};
			On_Main.RefreshPlayerDrawOrder += (orig, self) => {
				orig(self);
				if (active) {
					Array.Clear(projOwners);
					players.Clear();
					int buffID = ModContent.BuffType<Weak_Shimmer_Debuff>();
					List<Player> playersThatDrawAfterProjectiles = _playersThatDrawAfterProjectiles.GetValue(self);
					for (int i = playersThatDrawAfterProjectiles.Count - 1; i >= 0; i--) {
						Player player = playersThatDrawAfterProjectiles[i];
						if (player.HasBuff(buffID)) {
							players.Add(player);
							playersThatDrawAfterProjectiles.RemoveAt(i);
							projOwners[player.whoAmI] = true;
						}
					}
				}
			};
			try {
				IL_Main.DrawProjectiles += (il) => {
					ILCursor c = new(il);
					c.GotoNext(MoveType.Before, i => i.MatchLdfld<Projectile>(nameof(Projectile.hide)));
					c.EmitDup();
					c.Index++;
					c.EmitDelegate((Projectile proj, bool hide) => hide || shimmeryNormalProjs[proj.whoAmI]);
				};
			} catch (Exception ex) {
				if (Origins.LogLoadingILError($"{nameof(SC_Phase_Three_Overlay)}_DontNormalDrawShimmeryProjectiles", ex)) throw;
			}
			instance = this;
			sc_BGs = new Asset<Texture2D>[bgsAmount];
			for (int i = 1; i < bgsAmount; i++) {
				sc_BGs[i] = ModContent.Request<Texture2D>("Origins/Textures/Backgrounds/SC_BG/SC_BG" + i);
			}
		}
		bool drawingDust = false;
		public static bool HideAllDust => instance.active && !instance.drawingDust;
		static SC_Phase_Three_Overlay instance;
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
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
}
