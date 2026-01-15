using Microsoft.Xna.Framework.Graphics;
using Origins.Dusts;
using Origins.Graphics.Primitives;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;
using static Origins.OriginExtensions;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen.Boss {
	public class PhaseOneIdleState : AIState {
		#region stats
		public static float IdleTime => 60;
		#endregion stats
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, _ => 1));
		}
		public override void DoAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			TargetSearchResults searchResults = SearchForTarget(npc, TargetSearchFlag.Players);
			if (searchResults.FoundTarget) {
				npc.target = searchResults.NearestTargetIndex;
				npc.targetRect = searchResults.NearestTargetHitbox;
				if (npc.ShouldFaceTarget(ref searchResults)) npc.FaceTarget();
			}

			//npc.velocity *= 0.97f;
			if (++npc.ai[0] > IdleTime && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.PreviousStates.Contains)) Array.Fill(boss.PreviousStates, Index);
				boss.SelectAIState(aiStates);
			}
			if (npc.HasValidTarget) npc.DiscourageDespawn(60 * 5);
			else npc.EncourageDespawn(60);
		}
		public override void TrackState(int[] previousStates) { }
		public static List<AIState> aiStates = [];
	}
	public class Fire_Guns_State : AIState {
		#region stats
		public static float ShotRate => 10 - DifficultyMult;
		public static int ShotDamage => (int)(18 * DifficultyMult);
		public static float ShotVelocity => 12;
		public static int Duration => 45;
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			Vector2 direction = npc.rotation.ToRotationVector2();
			int shotsToHaveFired = (int)((++npc.ai[0]) / npc.ai[3]);
			if (shotsToHaveFired > npc.ai[1]) {
				//SoundEngine.PlaySound(SoundID.Item12.WithVolume(0.5f).WithPitchRange(0.25f, 0.4f), npc.Center);
				npc.ai[1]++;
				Vector2 perp = direction.RotatedBy(MathHelper.PiOver2);
				npc.SpawnProjectile(null,
					boss.GunPos + direction * 8 + perp * (6 * ((npc.ai[1] % 2) == 0).ToDirectionInt() + 2),
					direction * ShotVelocity,
					ProjectileID.BulletDeadeye,
					ShotDamage,
					1
				);
			}
			if (npc.ai[0] > Duration) boss.StartIdle();
		}
		public override void StartAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = ShotRate;
		}
		public override double GetWeight(Trenchmaker boss, int[] previousStates) => boss.GunType == 0 ? base.GetWeight(boss, previousStates) : 0;
	}
	public class Fire_Cannons_State : AIState {
		#region stats
		public static float ShotRate => 20 - DifficultyMult * 2;
		public static int ShotDamage => (int)(25 * DifficultyMult);
		public static float ShotVelocity => 12;
		public static int Duration => 40;
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			Vector2 direction = npc.rotation.ToRotationVector2();
			int shotsToHaveFired = (int)((++npc.ai[0]) / npc.ai[3]);
			if (shotsToHaveFired > npc.ai[1]) {
				SoundEngine.PlaySound(Origins.Sounds.HeavyCannon.WithPitch(-0.5f), boss.GunPos);
				npc.ai[1]++;
				npc.SpawnProjectile(null,
					boss.GunPos + direction * 6,
					direction * ShotVelocity,
					ModContent.ProjectileType<Trenchmaker_Cannon_P>(),
					ShotDamage,
					1
				);
			}
			if (npc.ai[0] > Duration) boss.StartIdle();
		}
		public override void StartAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = ShotRate;
		}
		public override double GetWeight(Trenchmaker boss, int[] previousStates) {
			if (boss.GunType != 1) return 0;
			if (boss.NPC.targetRect.Center().WithinRange(boss.NPC.Bottom + Vector2.UnitY * 16 * 4, 16 * 10)) return 0;
			return base.GetWeight(boss, previousStates);
		}
		public class Trenchmaker_Cannon_P : ModProjectile {
			public override string Texture => "Terraria/Images/Item_1";
			public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Canister_Outer";
			public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Canister_Inner";
			public static int ID { get; private set; }
			public override void SetStaticDefaults() {
				ProjectileID.Sets.TrailingMode[Type] = 2;
				ProjectileID.Sets.TrailCacheLength[Type] = 45;
				ProjectileID.Sets.DrawScreenCheckFluff[Type] = ProjectileID.Sets.TrailCacheLength[Type] * 10 + 64;
				ID = Type;
			}
			public override void SetDefaults() {
				OriginsSets.Projectiles.HomingEffectivenessMultiplier[Type] = 0.0125f;
				Projectile.aiStyle = 0;
				Projectile.extraUpdates = 2;
				Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
				Projectile.width = 28;
				Projectile.height = 28;
				Projectile.hostile = true;
				Projectile.penetrate = 1;
				Projectile.timeLeft = 900;
				Projectile.scale = 0.85f;
				Projectile.appliesImmunityTimeOnSingleHits = true;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.localNPCHitCooldown = 6;
			}
			public override void OnSpawn(IEntitySource source) {
				if (Projectile.TryGetGlobalProjectile(out ExplosiveGlobalProjectile global)) global.modifierBlastRadius *= 2;
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				if (Projectile.velocity.X == 0f) {
					Projectile.velocity.X = -oldVelocity.X;
				}
				if (Projectile.velocity.Y == 0f) {
					Projectile.velocity.Y = -oldVelocity.Y;
				}
				Projectile.timeLeft = 1;
				return true;
			}
			public override void AI() {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				if (Projectile.soundDelay == 0) {
					Projectile.soundDelay = -1;
					Dust.NewDustPerfect(
						Projectile.Center + Projectile.velocity.Normalized(out _) * 120,
						ModContent.DustType<Rocket_Launch>(),
						Projectile.velocity,
						newColor: Color.DimGray
					);
				}
			}
			public override void OnKill(int timeLeft) {
				if (NetmodeActive.Server) return;
				if (Projectile.owner != Main.myPlayer) {
					if (!Projectile.hide) {
						Projectile.hide = true;
						try {
							Projectile.active = true;
							Projectile.timeLeft = timeLeft;
							Projectile.Update(Projectile.whoAmI);
						} finally {
							Projectile.active = false;
							Projectile.timeLeft = 0;
						}
					}
					return;
				}
				ExplosiveGlobalProjectile.DoExplosion(Projectile, 80, sound: SoundID.Item62, hostile: true);
				Vector2[] oldPos = [.. Projectile.oldPos];
				float[] oldRot = [.. Projectile.oldRot];
				for (int i = 0; i < oldPos.Length; i++) {
					if (oldPos[i] == default) {
						Array.Resize(ref oldPos, i);
						Array.Resize(ref oldRot, i);
						break;
					}
					oldPos[i] += Projectile.Size * 0.5f;
					oldRot[i] += MathHelper.PiOver2;
				}
				Dust.NewDustPerfect(
					Main.LocalPlayer.Center,
					ModContent.DustType<Vertex_Trail_Dust>(),
					Vector2.Zero
				).customData = new Vertex_Trail_Dust.TrailData(oldPos, oldRot, StripColors(Color.Goldenrod), StripWidth, 2);
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				modifiers.ScalingArmorPenetration += 0.75f;
				modifiers.Knockback *= 3;
			}
			public override void OnHitPlayer(Player target, Player.HurtInfo info) {
				Min(ref Projectile.timeLeft, 1);
			}
			public override bool PreDraw(ref Color lightColor) {
				Vector2 origin = outerTexture.Value.Size() * 0.5f;
				SpriteEffects spriteEffects = SpriteEffects.None;
				if (Projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
				Main.EntitySpriteDraw(
					innerTexture,
					Projectile.Center - Main.screenPosition,
					null,
					Color.Goldenrod,
					Projectile.rotation,
					origin,
					Projectile.scale,
					spriteEffects
				);
				Main.EntitySpriteDraw(
					outerTexture,
					Projectile.Center - Main.screenPosition,
					null,
					Color.DarkGray.MultiplyRGBA(lightColor),
					Projectile.rotation,
					origin,
					Projectile.scale,
					spriteEffects
				);

				MiscShaderData miscShaderData = GameShaders.Misc["RainbowRod"];
				Vector2[] oldPos = [.. Projectile.oldPos];
				float[] oldRot = [.. Projectile.oldRot];
				for (int i = 0; i < oldPos.Length; i++) {
					if (oldPos[i] == default) {
						Array.Resize(ref oldPos, i);
						Array.Resize(ref oldRot, i);
						break;
					}
					oldRot[i] += MathHelper.PiOver2;
				}
				miscShaderData.UseSaturation(-2.8f);
				miscShaderData.UseOpacity(4f);
				miscShaderData.Apply();
				_vertexStrip.PrepareStripWithProceduralPadding(oldPos, oldRot, StripColors(Color.Goldenrod), StripWidth, -Main.screenPosition + Projectile.Size / 2f);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				return false;
			}
			static VertexStrip.StripColorFunction StripColors(Color color) => progressOnStrip => {
				if (float.IsNaN(progressOnStrip)) return Color.Transparent;
				float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				return color * (1f - lerpValue * lerpValue);
			};
			static float StripWidth(float progressOnStrip) {
				float lerpValue = 1f - Utils.GetLerpValue(0f, 0.2f, progressOnStrip, clamped: true);
				return MathHelper.Lerp(0, 8, 1f - lerpValue * lerpValue);
			}
			private static readonly VertexStrip _vertexStrip = new();
		}
	}
	public class Firecracker_State : AIState {
		#region stats
		public static int ShotCount => (int)(5 + DifficultyMult);
		public static int ShotDamage => (int)(18 * DifficultyMult);
		public static float ShotVelocityMin => 8;
		public static float ShotVelocityStep => 2;
		public static int StateDuration => 90;
		/// <param name="kind">0: red, 1: tan</param>
		public static float FuseTime(int kind, float index) => 90 + index * 4;
		public static int RedExplosionDuration => 30;
		public static int RedExplosionSize => (int)(48 + 16 * DifficultyMult);
		public static float TanExplosionRange => 112 + 16 * DifficultyMult;
		public static float TanExplosionSpread => 0.5f;
		#endregion stats
		public override bool Ranged => true;
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Trenchmaker boss) {
			if (++boss.NPC.ai[0] > StateDuration) boss.StartIdle();
		}
		public override void StartAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			int length = ShotCount;
			int projType = ModContent.ProjectileType<Trenchmaker_Firecracker>();
			int mode = Main.rand.Next(0, 2);
			int lastFirecracker = -1;
			for (int i = 0; i < length; i++) {
				lastFirecracker = npc.SpawnProjectile(null,
					npc.Center,
					Vector2.UnitX * npc.direction * (ShotVelocityMin + ShotVelocityStep * i),
					projType,
					ShotDamage,
					1,
					mode,
					i,
					lastFirecracker
				)?.whoAmI ?? lastFirecracker;
			}
		}
		public class Trenchmaker_Firecracker : ModProjectile {
			protected static AutoLoadingAsset<Texture2D> fuseTexture = typeof(Trenchmaker_Firecracker).GetDefaultTMLName("_Fuse");
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 2;
			}
			public override void SetDefaults() {
				Projectile.width = 16;
				Projectile.height = 16;
				Projectile.aiStyle = -1;
				Projectile.hostile = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = 360;
			}
			public override bool? CanDamage() => false;
			public override void AI() {
				Projectile.frame = (int)Projectile.ai[0];
				Projectile.rotation = 0;
				Projectile.velocity.X *= 
					float.Pow(0.97f, 1f / (Projectile.ai[1] + 1))
					* float.Pow(0.999f, Projectile.localAI[0] * 0.25f);
				Projectile.velocity.Y += 0.2f;
				if (++Projectile.localAI[0] > FuseTime((int)Projectile.ai[0], Projectile.ai[1])) {
					Projectile.Kill();
				}
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				Projectile.velocity *= 0.5f;
				return false;
			}
			public override void OnKill(int timeLeft) {
				Projectile.SpawnProjectile(null,
					Projectile.Center,
					Vector2.UnitY * -TanExplosionRange,
					Projectile.ai[0] == 0 ? ModContent.ProjectileType<Trenchmaker_Firecracker_Explosion_1>() : ModContent.ProjectileType<Trenchmaker_Firecracker_Explosion_2>(),
					Projectile.damage,
					Projectile.knockBack
				);
			}
			public override void SendExtraAI(BinaryWriter writer) {
				writer.Write((float)Projectile.localAI[0]);
			}
			public override void ReceiveExtraAI(BinaryReader reader) {
				Projectile.localAI[0] = reader.ReadSingle();
			}
			Vector2? prevPosition;
			private static readonly VertexRectangle VertexRectangle = new();
			public override bool PreDraw(ref Color lightColor) {
				if (Projectile.GetRelatedProjectile(2) is Projectile prev && prev.type == Type && prev.ai[0] == Projectile.ai[0]) {
					prevPosition = prev.Center;
				} else {
					Projectile.ai[2] = -1;
					if (prevPosition.HasValue) {
						if (Projectile.localAI[1] == 0) {
							Projectile.localAI[1] = FuseTime((int)Projectile.ai[0], Projectile.ai[1]) - Projectile.localAI[0];
						} else {
							prevPosition = Vector2.Lerp(prevPosition.Value, Projectile.Center, 1f / Projectile.localAI[1]);
							Projectile.localAI[1]--;
						}
					}
				}
				if (prevPosition is Vector2 end) {
					Asset<Texture2D> texture = fuseTexture;
					MiscShaderData miscShaderData = GameShaders.Misc["Origins:Beam"];
					miscShaderData.UseImage0(texture);
					miscShaderData.UseShaderSpecificData(texture.UVFrame());
					miscShaderData.Shader.Parameters["uLoopData"].SetValue(new Vector2(
						Projectile.Center.Distance(end) / 38,
						0
					));
					Vector2 unit = (end - Projectile.Center).Normalized(out _) * 2;
					(unit.X, unit.Y) = (unit.Y, -unit.X);
					miscShaderData.Apply();
					VertexRectangle.DrawLit(Main.screenPosition, Projectile.Center - unit, end - unit, Projectile.Center + unit, end + unit);
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				}
				return base.PreDraw(ref lightColor);
			}
		}
		public class Trenchmaker_Firecracker_Explosion_1 : ModProjectile {
			public override string Texture => typeof(Trenchmaker_Firecracker).GetDefaultTMLName();
			public override void SetDefaults() {
				Projectile.width = 0;
				Projectile.height = 0;
				Projectile.tileCollide = false;
				Projectile.hostile = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = RedExplosionDuration;
				Projectile.hide = true;
			}
			public override bool ShouldUpdatePosition() => false;
			public override bool? CanDamage() => Projectile.width != 0;
			public override void AI() {
				if (Projectile.ai[2].CycleDown(7)) {
					ExplosiveGlobalProjectile.DoExplosion(Projectile, RedExplosionSize, false, SoundID.Item40.WithPitch(0.0f).WithVolume(1f), 10, 0, 0, DustID.IchorTorch, hostile: true);
					Projectile.position.X += Projectile.width / 2;
					Projectile.position.Y += Projectile.height / 2;
					Projectile.width = 0;
					Projectile.height = 0;
				}
			}
		}
		public class Trenchmaker_Firecracker_Explosion_2 : ModProjectile, IIsExplodingProjectile {
			public override string Texture => typeof(Trenchmaker_Firecracker).GetDefaultTMLName();
			bool didVisual = false;
			public override void SetDefaults() {
				Projectile.width = 0;
				Projectile.height = 0;
				Projectile.tileCollide = false;
				Projectile.hostile = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = 5;
				Projectile.hide = true;
			}
			public override bool ShouldUpdatePosition() => false;
			public override void AI() {
				if (Main.netMode == NetmodeID.Server || didVisual) return;
				didVisual = true;
				Vector2 direction = Projectile.velocity.SafeNormalize(-Vector2.UnitY);
				Vector2 spread = GetSpread(direction);
				for (int i = 0; i < 30; i++) {
					Vector2 dustVelocity = (direction * Main.rand.NextFloat(0.8f, 1) + spread * Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
					Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.Smoke,
						dustVelocity.X,
						dustVelocity.Y,
						100,
						default,
						1.5f
					).velocity *= 1.4f;
				}
				for (int i = 0; i < 20; i++) {
					Vector2 dustVelocity = (direction * Main.rand.NextFloat(0.8f, 1) + spread * Main.rand.NextFloat(-0.5f, 0.5f)) * 4;
					Dust dust = Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.Torch,
						0,
						0,
						100,
						default,
						3.5f
					);
					dust.noGravity = true;
					dust.velocity *= 7f;
					dust.velocity += dustVelocity;
					dust = Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.Torch,
						0,
						0,
						100,
						default,
						1.5f
					);
					dust.velocity *= 3f;
					dust.velocity += dustVelocity;
				}
				SoundEngine.PlaySound(in SoundID.Item62, Projectile.position);
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				Vector2 laserStartPoint = Projectile.position;
				float factor = 1 - Math.Min(Projectile.timeLeft / 5f, 0.9f);
				Vector2 spread = GetSpread(Projectile.velocity * factor);
				return new Triangle(laserStartPoint, laserStartPoint + Projectile.velocity * factor - spread, laserStartPoint + Projectile.velocity * factor + spread).Intersects(targetHitbox);
			}
			static Vector2 GetSpread(Vector2 velocity) => velocity.RotatedBy(MathHelper.PiOver2) * TanExplosionSpread;
			public bool IsExploding => true;
		}
	}
	public class Carpet_Bomb_State : AIState {
		public static int MaxCount => (int)(12 + DifficultyMult);
		public static int DropRate => (int)(25 - DifficultyMult);
		public static int ShotDamage => (int)(18 * DifficultyMult);
		public static float ChargeSoundVolume => 1;
		public static float ChargeSoundFadeTime => 30;
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
			iconTexture = typeof(Trenchmaker_Carpet_Bomb).GetDefaultTMLName();
		}
		public override void DoAIState(Trenchmaker boss) {
			NPC npc = boss.NPC;
			if (boss.legs[0].CurrentAnimation is Jump_Preparation_Animation or Jump_Squat_Animation) {
				if (npc.soundDelay == 0) {
					SoundEngine.PlaySound(Origins.Sounds.ThrusterChargeUp.WithVolume(ChargeSoundVolume), npc.Center, sound => {
						if (npc.ai[0] > 0) {
							sound.Volume -= ChargeSoundVolume / ChargeSoundFadeTime;
						}
						return sound.Volume > 0;
					});
				}
				npc.soundDelay = 30;
				return;
			}
			if (boss.legs[0].CurrentAnimation is Jump_Extend_Animation) {
				npc.ai[1] = 1;
				return;
			}
			if (boss.legs[0].CurrentAnimation is not Jump_Air_Animation) {
				if (npc.ai[1] > 0) {
					npc.frame.Y = 0;
					npc.frameCounter = 0;
					boss.StartIdle();
				}
				return;
			}
			npc.velocity.Y -= 0.33f;
			npc.velocity.X += npc.direction * 0.01f;
			npc.DoFrames(4, 1..7);
			if (++npc.ai[0] > (npc.ai[1] * DropRate) && npc.ai[1] < MaxCount + 1) {
				npc.ai[1]++;
				npc.SpawnProjectile(null,
					npc.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Trenchmaker_Carpet_Bomb>(),
					ShotDamage,
					1,
					Main.rand.Next(0, 2)
				);
			}
			if (npc.ai[1] >= MaxCount + 1) {
				npc.velocity.Y = -0.1f;
				//Min(ref npc.velocity.Y, 1);
			}
		}
		public override LegAnimation ForceAnimation(Trenchmaker npc, Leg leg, Leg otherLeg) => npc.NPC.ai[1] == 0 ? ModContent.GetInstance<Jump_Preparation_Animation>() : null;
		public class Trenchmaker_Carpet_Bomb : ModProjectile {
			public override void SetStaticDefaults() {
				ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
			}
			public override void SetDefaults() {
				Projectile.width = 34;
				Projectile.height = 34;
				Projectile.aiStyle = -1;
				Projectile.hostile = true;
				Projectile.penetrate = -1;
				Projectile.timeLeft = 360;
				Projectile.hide = true;
			}
			public override void AI() {
				Projectile.rotation += 0.1f;
				Projectile.velocity.Y += 0.2f;
			}
			public override void OnKill(int timeLeft) {
				ExplosiveGlobalProjectile.DoExplosion(Projectile, 128, sound: SoundID.Item14, hostile: true);
			}
			public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
				behindNPCs.Add(index);
			}
		}
	}
	public class Teabag_State : AIState {
		public override float WalkDist => 16 * 4;
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => (!boss.NPC.HasValidTarget).Mul(10000)));
		}
		public override void DoAIState(Trenchmaker boss) {
			boss.NPC.FaceTarget();
		}
	}
}
