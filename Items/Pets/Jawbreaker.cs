using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Pets;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Jawbreaker : ModItem {
		internal static int projectileID = 0;
		internal static int buffID = 0;
		public override void SetDefaults() {
			Item.DefaultToVanitypet(projectileID, buffID);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Master;
			Item.master = true;
		}
		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(Item.buffType, 3600);
			}
		}
		/*public IEnumerable<(string, (Texture2D, int)[])> GetAnimatedSprites => [
			("", SpriteGenerator.GenerateAnimationSprite(ModContent.Request<Texture2D>(typeof(Juvenile_Amalgamation).GetDefaultTMLName(), AssetRequestMode.ImmediateLoad).Value, Main.projFrames[projectileID], 5)),
		];*/
	}
	public class Stellar_Spark : ModProjectile, IPreDrawSceneProjectile, ITriggerSCBackground {
		public override void SetStaticDefaults() {
			Jawbreaker.projectileID = Projectile.type;
			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
		}
		public override void SetDefaults() {
			Projectile.timeLeft = 5;
			Projectile.Size = new(16 * 3);
			Projectile.tileCollide = false;
			Projectile.friendly = false;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}
		public override void AI() {
			if (Main.rand.NextBool(650) && Projectile.ai[1] == 0) Projectile.ai[1] = 1;
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Jawbreaker.buffID);
			}
			if (player.HasBuff(Jawbreaker.buffID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top;
			idlePosition.X -= 48f * player.direction;

			// Teleport to player if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				// Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
				// and then set netUpdate to true
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}

			// If your minion is flying, you want to do this independently of any conditions
			float overlapVelocity = 0.04f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				// Fix overlap with other minions
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width) {
					if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
					else Projectile.velocity.X += overlapVelocity;

					if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
					else Projectile.velocity.Y += overlapVelocity;
				}
			}


			#endregion
			#region Movement

			float speed;
			float inertia;
			if (distanceToIdlePosition > 600f) {
				speed = 16f;
				inertia = 36f;
			} else {
				speed = 6f;
				inertia = 48f;
			}
			if (Projectile.ai[0] > 0) {
				speed /= 2;
				inertia /= 2;
			}
			if (distanceToIdlePosition > 12f) {
				// The immediate range around the player (when it passively floats about)

				// This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
				vectorToIdlePosition.Normalize();
				vectorToIdlePosition *= speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
			} else if (Projectile.velocity != Vector2.Zero) {
				Projectile.velocity *= 0.95f;
			}
			#endregion

			#region Animation and visuals
			// So it will lean slightly towards the direction it's moving
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				Projectile.frame++;
				if (Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}
			int velDir = Math.Sign(Projectile.velocity.X);
			if (velDir == 0) {
				Projectile.spriteDirection = player.direction;
			} else {
				Projectile.spriteDirection = velDir;
			}

			// Some visuals here
			if (Projectile.ai[1] == 1 && Projectile.ai[0] == 0) {
				if (MathUtils.LinearSmoothing(ref Projectile.ai[2], 1f, 1 / 60f)) Projectile.ai[0] = 1;
			} else if (Projectile.ai[1] == 1 && Projectile.ai[0]++ > 600) {
				if (MathUtils.LinearSmoothing(ref Projectile.ai[2], 0, 1.5f / 60f)) {
					Projectile.ai[1] = Projectile.ai[0] = 0;
				}
			}
			#endregion
		}
		public override void PostDraw(Color lightColor) {
			// for player select screen
			for (int i = 0; i < chunks.Length; i++) chunks[i].Update(this);
		}

		Chunk[] chunks = [];
		struct Chunk(int type, Vector2 position, int index) {
			public float rotation;
			public Vector2 position = position;
			public Vector2 velocity = new(0, 12 + index * 6);
			public int ID = type;
			public void Update(Stellar_Spark spark) {
				float speed = 0.075f + index * 0.005f;
				velocity = velocity.RotatedBy(speed);
				position = spark.Projectile.Center + velocity;
				rotation += speed * 0.5f;
			}
			public readonly bool Draw(Stellar_Spark spark) {
				Main.instance.LoadGore(ID);
				Main.EntitySpriteDraw(
					TextureAssets.Gore[ID].Value,
					position - Main.screenPosition,
					null,
					Color.White,
					rotation,
					TextureAssets.Gore[ID].Size() * 0.5f,
					1,
					SpriteEffects.None,
				0f);
				return false;
			}
		}
		public DrawData GetAuraDrawData(Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			return new(
				texture,
				(Projectile.Center - Main.screenPosition).Floor(),
				null,
				lightColor,
				0,
				texture.Size() * 0.5f,
				Projectile.scale * Projectile.ai[2],
				SpriteEffects.None
			);
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			if (drawAuraInBackground) {
				behindNPCsAndTiles.Add(index);
			} else {
				behindNPCs.Add(index);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 position = Projectile.Center;
			if (DrawOrder.LastDrawnOverlayLayer <= RenderLayers.Walls) DrawAuraOutline();

			if (!Projectile.isAPreviewDummy && DrawOrder.LastDrawnOverlayLayer <= RenderLayers.Walls) return false;
			default(ShimmerConstructSDF).Draw(position - Main.screenPosition, Projectile.rotation, new Vector2(256, 256) / 3f);
			if (chunks.Length <= 0) {
				chunks = [
					new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing2"), position + new Vector2(18 * Projectile.direction, 27).RotatedBy(Projectile.rotation), 1),
					new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing2"), position + new Vector2(-26 * Projectile.direction, 31).RotatedBy(Projectile.rotation), 2),
					new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing1"), position + new Vector2(48 * Projectile.direction, -12).RotatedBy(Projectile.rotation), 3),
					new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing_Medium2"), position + new Vector2(14 * Projectile.direction, 14).RotatedBy(Projectile.rotation), 4),
					new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing_Medium1"), position + new Vector2(-23 * Projectile.direction, -57).RotatedBy(Projectile.rotation), 5),
					new(Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing1"), position + new Vector2(22 * Projectile.direction, -57).RotatedBy(Projectile.rotation), 6)
				];
			}
			for (int i = 0; i < chunks.Length; i++) chunks[i].Draw(this);
			position += Main.rand.NextVector2Circular(1, 1) * (Main.rand.NextFloat(0.5f, 1f) * 12);
			return false;
		}

		private void DrawAuraOutline() {
			if (middleRenderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}

			SpriteBatchState state = Main.spriteBatch.GetState().FixedCulling();
			Main.spriteBatch.Restart(state, SpriteSortMode.Immediate, transformMatrix: Main.GameViewMatrix.ZoomMatrix);
			Origins.shaderOroboros.Capture();

			if (Projectile.ai[2] > 0) {
				DrawData data = GetAuraDrawData(Color.White);
				Vector2 basePos = data.position;
				int shader = Main.CurrentDrawnEntityShader;
				Main.CurrentDrawnEntityShader = 0;
				for (int i = 0; i < 4; i++) {
					data.position = basePos + (MathHelper.PiOver2 * i).ToRotationVector2() * 2 * (data.scale + Vector2.One);
					Main.EntitySpriteDraw(data);
				}
				Main.CurrentDrawnEntityShader = shader;
			}

			Main.graphics.GraphicsDevice.Textures[1] = middleRenderTarget;
			Accretion_Ribbon.EraseShader.Shader.Parameters["uImageSize1"]?.SetValue(new Vector2(middleRenderTarget.Width, middleRenderTarget.Height));
			Origins.shaderOroboros.Stack(Accretion_Ribbon.EraseShader);
			Origins.shaderOroboros.DrawContents(edgeRenderTarget, Color.White, Matrix.Identity);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = edgeRenderTarget.Size() * 0.5f;
			Main.spriteBatch.Restart(state, transformMatrix: Matrix.Identity);
			Main.EntitySpriteDraw(
				edgeRenderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One,
				Main.GameViewMatrix.Effects
			);
			Main.spriteBatch.Restart(state);
		}
		bool drawAuraInBackground = true;
		public void PreDrawScene() {
			if (middleRenderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			List<DrawData> DrawDatas = SC_Phase_Three_Underlay.DrawDatas;
			HashSet<object> DrawnMaskSources = SC_Phase_Three_Underlay.DrawnMaskSources;
			drawAuraInBackground = true;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.ModNPC is Shimmer_Construct shimmerConstruct && shimmerConstruct.IsInPhase3) {
					//DrawDatas = SC_Phase_Three_Midlay.DrawDatas;
					//DrawnMaskSources = SC_Phase_Three_Midlay.DrawnMaskSources;
					drawAuraInBackground = false;
					break;
				}
			}
			if (!DrawnMaskSources.Add(this)) return;
			Origins.shaderOroboros.Capture();
			Main.spriteBatch.Restart(Main.spriteBatch.GetState().FixedCulling());

			Main.EntitySpriteDraw(GetAuraDrawData(Color.White));

			Origins.shaderOroboros.DrawContents(middleRenderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = middleRenderTarget.Size() * 0.5f;
			DrawDatas.Add(new(
				middleRenderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None
			));
		}
		public override void OnKill(int timeLeft) {
			if (middleRenderTarget is not null) {
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref middleRenderTarget);
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref edgeRenderTarget);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D middleRenderTarget;
		internal RenderTarget2D edgeRenderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref middleRenderTarget);
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref edgeRenderTarget);
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (middleRenderTarget is not null && !middleRenderTarget.IsDisposed) return;
			middleRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			edgeRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
}

namespace Origins.Buffs {
	public class Stellar_Spark_Buff : ModBuff {
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
			Jawbreaker.buffID = Type;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;

			int projType = Jawbreaker.projectileID;

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				var entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}
	}
}