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

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class PhaseThreeIdleState : AIState {
		public static List<AIState> aiStates = [];
		public override void Load() {
			AutomaticIdleState.aiStates.Add((this, boss => boss.IsInPhase3.Mul(3)));
		}
		public override void SetStaticDefaults() {

		}
		public override void DoAIState(Shimmer_Construct boss) {
			NPC npc = boss.NPC;
			npc.TargetClosest();
			npc.velocity *= 0.97f;
			if (++npc.ai[0] > (60 - ContentExtensions.DifficultyDamageMultiplier * 10) && Main.netMode != NetmodeID.MultiplayerClient) {
				if (aiStates.Select(state => state.Index).All(boss.previousStates.Contains)) Array.Fill(boss.previousStates, Index);
				SelectAIState(boss, aiStates);
			}
		}
		public override void TrackState(int[] previousStates) { }
	}
	public class Weak_Shimmer_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Shimmer;
		public static int ID { get; private set; }
		public override void Load() {
			On_Player.ShimmerCollision += (On_Player.orig_ShimmerCollision orig, Player self, bool fallThrough, bool ignorePlats, bool noCollision) => {
				if (self.OriginPlayer().weakShimmer) {
					self.position += self.velocity * new Vector2(0.75f, 0.75f);
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
