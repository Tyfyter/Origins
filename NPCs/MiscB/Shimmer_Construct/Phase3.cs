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
using Origins.NPCs.Defiled.Boss;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseThreeIdleState : AIState {
		public static List<AIState> aiStates = [];
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => boss.IsInPhase3.Mul(3)));
		}
		public override void SetStaticDefaults() {
			aiStates.Add(ModContent.GetInstance<SpawnCloudsState>());
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
	}
	public class ShimmerLandminesState : AIState {
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		Stack<Vector2> positions = new();
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			Vector2 targetPos = npc.GetTargetData().Center;
			npc.velocity += npc.DirectionTo(targetPos - Vector2.UnitY * 16 * 15) * 0.5f;
			for (int i = 0; i < 2; i++) {
				if (positions.TryPop(out Vector2 position)) {
					npc.SpawnProjectile(null,
						position,
						Vector2.Zero,
						Shimmer_Landmines.ID,
						1,
						1
					);
				} else {
					SetAIState(boss, StateIndex<AutomaticIdleState>());
				}
			}
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[1] = 6 - ContentExtensions.DifficultyDamageMultiplier;
			npc.ai[2] = 5 + Main.rand.RandomRound(ContentExtensions.DifficultyDamageMultiplier - 1);
			float lowestHeight = 0;
			int buffID = ModContent.BuffType<Weak_Shimmer_Debuff>();
			foreach (Player player in Main.ActivePlayers) {
				if (lowestHeight < player.BottomLeft.Y && player.HasBuff(buffID)) {
					lowestHeight = player.BottomLeft.Y;
				}
			}
			if (lowestHeight == 0) {
				npc.ai[1] = 0;
				npc.ai[2] = 0;
			} else {
				Vector2 basePos = npc.GetTargetData().Center;
				basePos.Y = lowestHeight + 16 * 10;
				Rectangle area = OriginExtensions.BoxOf(basePos - new Vector2(60 * 16, 0), basePos + new Vector2(40 * 16, 80 * 16));
				List<Vector2> newPositions = OriginExtensions.PoissonDiskSampling(Main.rand, area, 16 * (27 - ContentExtensions.DifficultyDamageMultiplier * 2));
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
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.velocity = npc.ai[1].ToRotationVector2() * (6 + ContentExtensions.DifficultyDamageMultiplier) * (npc.ai[2] > 0 ? 1.15f : 1.3f);
			npc.rotation = npc.velocity.ToRotation();
			if ((++npc.ai[0]) * npc.ai[3] > 16 * (11 - ContentExtensions.DifficultyDamageMultiplier * 2)) {
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
			npc.ai[3] = 6 + ContentExtensions.DifficultyDamageMultiplier;
			npc.ai[1] = (npc.GetTargetData().Center - npc.Center).ToRotation();
			npc.ai[2] = ContentExtensions.DifficultyDamageMultiplier * 2 - 2;

			int droneType = ModContent.NPCType<Shimmer_Drone>();
			int drones = 0;
			const int threshold = 2;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other.type == droneType && ++drones >= threshold) {
					npc.ai[2] = -npc.ai[2];
					break;
				}
			}
		}
	}
	public class ShotgunState : AIState {
		public override void Load() {
			PhaseThreeIdleState.aiStates.Add(this);
		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			GeometryUtils.AngularSmoothing(ref npc.ai[2], (npc.GetTargetData().Center - npc.Center).ToRotation(), 0.01f);
			npc.rotation = npc.ai[2];
			npc.velocity *= 0.93f;
			if (++npc.ai[0] > 30) SetAIState(boss, StateIndex<AutomaticIdleState>());
		}
		public override void StartAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.ai[2] = (npc.GetTargetData().Center - npc.Center).ToRotation();
			npc.SpawnProjectile(null,
				npc.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Shotgun_Indicator>(),
				1,
				1,
				60,
				Main.rand.Next(1000),
				npc.whoAmI
			);
		}
		public class Shotgun_Indicator : ModProjectile {
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.HallowBossRainbowStreak;

			UnifiedRandom rand = new();
			NPC Owner => Main.npc.GetIfInRange((int)Projectile.ai[2]);
			public override void SetDefaults() {
				Projectile.tileCollide = false;
			}
			public override bool ShouldUpdatePosition() => false;
			IEnumerable<Vector2> GetShots() {
				rand.SetSeed((int)Projectile.ai[1]);
				Vector2 dir = Owner.rotation.ToRotationVector2() * 12;
				for (int i = 3 + rand.RandomRound(ContentExtensions.DifficultyDamageMultiplier); i > 0; i--) {
					yield return dir.RotatedBy(rand.NextFloat(0.5f) * (i % 2 == 0).ToDirectionInt()) * rand.NextFloat(0.7f, 1f);
				}
			}
			public override void AI() {
				if (Owner is not NPC owner) {
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
				float alpha = (Projectile.ai[0] <= 5 && Projectile.ai[0] >= 2) ? 0.08f : 0.02f;
				foreach (Vector2 shot in GetShots()) {
					float rotation = shot.ToRotation();
					Defiled_Spike_Indicator.Draw(
						[Projectile.Center, Projectile.Center + shot * 80],
						[rotation, rotation],
						8,
						alpha,
						0.5f
					);
				}
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
		}
		static void IL_Projectile_HandleMovement(ILContext il) {
			ILCursor c = new(il);
			c.GotoNext(MoveType.After,
				i => i.MatchLdfld<Projectile>(nameof(Projectile.tileCollide))
			);
			c.EmitLdarg0();
			c.EmitDelegate((bool tileCollide, Projectile projectile) => tileCollide && (!projectile.friendly || !projectile.TryGetOwner(out Player player) || !player.OriginPlayer().weakShimmer));
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
	}
	public class SC_Phase_Three_Overlay() : Overlay(EffectPriority.High, RenderLayers.ForegroundWater), ILoadable {
		readonly Asset<Texture2D> texture = ModContent.Request<Texture2D>("Origins/Textures/Shimmer_Construct_BG");
		readonly ArmorShaderData invertAnimateShader = GameShaders.Armor.BindShader(ItemID.HallowedBar, new ArmorShaderData(ModContent.Request<Effect>("Origins/Effects/ShimmerConstruct"), "InvertAnimate"));
		readonly ArmorShaderData maskShader = GameShaders.Armor.BindShader(ItemID.AdamantiteMask, new ArmorShaderData(ModContent.Request<Effect>("Origins/Effects/ShimmerConstruct"), "Mask"));
		readonly List<Player> players = [];
		bool active = false;
		float opacity;
		public static readonly List<DrawData> drawDatas = [];
		public override void Draw(SpriteBatch spriteBatch) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			SpriteBatchState state = spriteBatch.GetState();
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

			Origins.shaderOroboros.Capture(spriteBatch);
			spriteBatch.Restart(state, sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.AnisotropicWrap);
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
			maskShader.Shader.Parameters["uFullColor"].SetValue(new Vector4(new(0.3f), 0.3f));
			Main.graphics.GraphicsDevice.Textures[1] = renderTarget;
			Origins.shaderOroboros.Stack(maskShader);
			Origins.shaderOroboros.Release();
			players.Clear();
			int buffID = ModContent.BuffType<Weak_Shimmer_Debuff>();
			foreach (Player player in Main.ActivePlayers) {
				if (player.HasBuff(buffID)) {
					players.Add(player);
					Lighting.AddLight(player.Center, new Vector3(1));
				}
			}

			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (!proj.hide) Main.instance.DrawProj(proj.whoAmI);
			}
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.ModNPC is Shimmer_Construct or Shimmer_Drone) {
					Main.instance.DrawNPCDirect(spriteBatch, npc, false, Main.screenPosition);
				}
			}
			spriteBatch.End();
			Main.PlayerRenderer.DrawPlayers(Main.Camera, players);
			PegasusLib.Reflection.DelegateMethods._target.SetValue(MainReflection.DrawDust, Main.instance);
			MainReflection.DrawDust();
			spriteBatch.Begin(state);
		}
		public override void Update(GameTime gameTime) {
			if (!Main.gamePaused || drawDatas.Count > 25) drawDatas.Clear();
		}
		public override void Activate(Vector2 position, params object[] args) {
			Mode = OverlayMode.Active;
			if (active.TrySet(true)) opacity = Main.rand.NextFloat(0.6f, 0.85f);
		}
		public override void Deactivate(params object[] args) {
			active = false;
			Mode = OverlayMode.Inactive;
		}
		public override bool IsVisible() => active;
		public void Load(Mod mod) {
		}
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
