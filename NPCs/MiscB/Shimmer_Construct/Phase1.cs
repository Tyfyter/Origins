using Origins.Graphics;
using Origins.Items.Weapons.Magic;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using static Origins.NPCs.MiscB.Shimmer_Construct.Shimmer_Construct;
using PegasusLib;
using Terraria.Graphics.Shaders;
using Origins.Items.Other.Dyes;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Liquid;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseOneIdleState : AIState {
		public static List<AIState> aiStates = [];
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, _ => 1));
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.TargetClosest();
			npc.velocity *= 0.97f;
			if (++npc.ai[0] > 60 && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.previousStates.Contains)) Array.Fill(boss.previousStates, Index);
				SelectAIState(boss, aiStates);
			}
		}
		public override void TrackState(int[] previousStates) { }
	}
	public class DashState : AIState {
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.velocity.X = npc.ai[1];
			npc.velocity.Y = npc.ai[2];
			npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;
			if ((++npc.ai[0]) * npc.ai[3] > 16 * 30) SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = 6 + ContentExtensions.DifficultyDamageMultiplier;
			Vector2 direction = npc.DirectionTo(npc.GetTargetData().Center) * npc.ai[3];
			npc.ai[1] = direction.X;
			npc.ai[2] = direction.Y;
		}
	}
	public class CircleState : AIState {
		public override void Load() {
			PhaseOneIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			Vector2 diff = npc.GetTargetData().Center - npc.Center;
			Vector2 direction = diff.SafeNormalize(Vector2.UnitY);
			Vector2 targetDiff = direction.RotatedBy(npc.direction) * 16 * 30;
			npc.velocity = diff.DirectionFrom(targetDiff) * (6.5f + ContentExtensions.DifficultyDamageMultiplier * 0.5f);
			int shotsToHaveFired = (int)((++npc.ai[0]) / npc.ai[3]);
			if (shotsToHaveFired > npc.ai[1]) {
				npc.ai[1]++;
				npc.SpawnProjectile(null,
					npc.Center,
					direction * 6,
					Shimmer_Construct_Bullet.ID,
					1,
					1
				);
			}
			if (npc.ai[0] > 120) SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[3] = 16 - ContentExtensions.DifficultyDamageMultiplier * 0.75f;
		}
	}
	public class Shimmer_Construct_Bullet : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HallowBossRainbowStreak;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
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
			base.AI();
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
			Vector2 targetPos = npc.GetTargetData().Center;
			float difficultyMultiplier = ContentExtensions.DifficultyDamageMultiplier;
			float xDist = 53 - 8 * difficultyMultiplier;
			Vector2 unfurlPos = targetPos - new Vector2(-xDist, 25) * 16;
			float speed = 4.5f + difficultyMultiplier * 4.5f;
			npc.SpawnProjectile(null,
				npc.Center,
				npc.DirectionTo(unfurlPos) * speed,
				ModContent.ProjectileType<Shimmer_Construct_Cloud_Ball>(),
				1,
				1,
				unfurlPos.X,
				unfurlPos.Y,
				-1
			);
			unfurlPos = targetPos - new Vector2(xDist, 25) * 16;
			npc.SpawnProjectile(null,
				npc.Center,
				npc.DirectionTo(unfurlPos) * speed,
				ModContent.ProjectileType<Shimmer_Construct_Cloud_Ball>(),
				1,
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
				if (++Projectile.ai[1] % 10f < 1) {
					Vector2 unit = Vector2.UnitY;
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
							if (!construct.active || construct.type != ModContent.NPCType<Shimmer_Construct>() || (Projectile.Center.X - construct.targetRect.Center.X) * Projectile.ai[0] > 16 * 20) {
								Projectile.timeLeft = 52;
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
				CooldownSlot = ImmunityCooldownID.WrongBugNet;
			}
			public override void AI() {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
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
}
