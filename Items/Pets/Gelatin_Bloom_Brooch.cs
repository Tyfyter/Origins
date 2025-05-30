using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Pets;
using PegasusLib;
using PegasusLib.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Pets {
	public class Gelatin_Bloom_Brooch : ModItem {
		AutoLoadingAsset<Texture2D> topTexture = typeof(Gelatin_Bloom_Brooch).GetDefaultTMLName() + "_Top";
		public override void SetDefaults() {
			Item.DefaultToVanitypet(ModContent.ProjectileType<Gelatin_Buddy>(), ModContent.BuffType<Gelatin_Aspect_Buff>());
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ModContent.RarityType<Eccentric_Rarity>();
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0) {
				player.AddBuff(Item.buffType, 3600);
			}
		}
		public static Color GetGolor(string name) {
			return Colors.GetIfInRange(GetNameIndex(name), Colors[^1]);
		}
		public static int GetNameIndex(string name) {
			switch (name) {
				case "Askoi":
				return 0;
				case "Calano":
				return 1;
				case "Aria":
				return 2;
				case "Saeheras":
				return 3;
				case "Blaire":
				return 4;

				case "Jennifer_Alt":
				return Colors.IndexInRange(Main.LocalPlayer.selectedItem) ? Main.LocalPlayer.selectedItem : -1;

				default:
				return -1;
			}
		}
		public static Color[] Colors { get; } = [
			new(184, 232, 67),
			new(255, 176, 53),
			new(76, 215, 222),
			new(229, 191, 255),
			new(225, 184, 58),
			Color.White
		];
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Color color = GetGolor(Main.LocalPlayer?.name);
			if (color == Color.White) return;
			spriteBatch.Draw(topTexture, position, frame, color, 0, origin, scale, SpriteEffects.None, 0);
		}
	}
	public class Eccentric_Rarity : ModRarity {
		static double timeForFade => Main.timeForVisualEffects / 60f;
		static float Easing(double time) {
			return (float)double.Lerp(Math.Pow(time, 3), 1 - Math.Pow(1 - time, 3), time);
		}
		public override Color RarityColor => Color.Lerp(
			Gelatin_Bloom_Brooch.Colors[(int)timeForFade % (Gelatin_Bloom_Brooch.Colors.Length - 1)],
			Gelatin_Bloom_Brooch.Colors[((int)timeForFade + 1) % (Gelatin_Bloom_Brooch.Colors.Length - 1)],
			Easing(timeForFade % 1)
		);
	}
	public class Gelatin_Buddy : ModProjectile {
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 16;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.LightPet[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.timeLeft = 5;
			Projectile.width = 24;
			Projectile.height = 22;
			Projectile.tileCollide = false;
			Projectile.friendly = false;
		}

		// Here you can decide if your minion breaks things like grass or pots
		public override bool? CanCutTiles() {
			return false;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(ModContent.BuffType<Gelatin_Aspect_Buff>());
			}
			if (player.HasBuff(ModContent.BuffType<Gelatin_Aspect_Buff>())) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Top;
			idlePosition.X += 40f * player.direction;

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
			if (distanceToIdlePosition > 160f) {
				speed = 16f;
				inertia = 18f;
			} else {
				speed = 6f;
				inertia = 24f;
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
			Projectile.rotation = Projectile.velocity.X * 0.1f;

			// This is a simple "loop through all frames from top to bottom" animation
			int frameSpeed = 5;
			if (++Projectile.frameCounter >= frameSpeed) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) Projectile.frame = 0;
			}
			if (Math.Abs(Projectile.velocity.X) < 0.1f) {
				Projectile.spriteDirection = player.direction;
			} else {
				Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
			}

			const int HalfSpriteWidth = 36 / 2;
			const int HalfSpriteHeight = 38 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			if (glowColor is null && baseColorRenderTarget is not null) {
				Color[] retrievedColor = new Color[1];
				baseColorRenderTarget.GetData(
					0,
					new(20, 4, 1, 1),
					retrievedColor,
					0,
					1
				);
				Vector3 glow = retrievedColor[0].ToVector3();
				glowColor = glow;
				Vector3 normalizedGlow = Vector3.Normalize(glow);
				if (!float.IsNaN(normalizedGlow.X) && !float.IsNaN(normalizedGlow.Y) && !float.IsNaN(normalizedGlow.Z)) {
					const float power = 2.7268f; // Menger sponge the normalized light to make white not better than the other colors
												 // I was going to England it, but its Hausdorff dimensionality is too low, and I'd have to update it if a clod be washed away by the sea
					glowColor = new(MathF.Pow(normalizedGlow.X, power), MathF.Pow(normalizedGlow.Y, power), MathF.Pow(normalizedGlow.Z, power));
				}
			}
			{
				if (glowColor is Vector3 glow) Lighting.AddLight(Projectile.Center, glow.X, glow.Y, glow.Z);
			}
			#endregion
		}
		Vector3? glowColor;
		RenderTarget2D baseColorRenderTarget;
		public override void OnKill(int timeLeft) {
			if (baseColorRenderTarget is not null) {
				Main.QueueMainThreadAction(baseColorRenderTarget.Dispose);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (baseColorRenderTarget is null) {
				SetupRenderTargets();

				SpriteBatchState state = Main.spriteBatch.GetState();
				RenderTargetBinding[] oldRenderTargets = Main.graphics.GraphicsDevice.GetRenderTargets();
				Main.graphics.GraphicsDevice.SetRenderTarget(baseColorRenderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);
				Main.spriteBatch.Restart(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, Main.Rasterizer, null, Matrix.Identity);
				Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Vector2.Zero, null, Gelatin_Bloom_Brooch.GetGolor(Main.player[Projectile.owner].name), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
				Main.spriteBatch.End();
				Main.graphics.GraphicsDevice.UseOldRenderTargets(oldRenderTargets);
				Main.spriteBatch.Begin(state);
			}
			Rectangle frame = TextureAssets.Projectile[Type].Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				baseColorRenderTarget,
				Projectile.Center - Main.screenPosition,
				frame,
				Color.White,
				0,
				frame.Size() * 0.5f,
				Vector2.One,
				Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			return false;
		}
		void SetupRenderTargets() {
			if (baseColorRenderTarget is not null && !baseColorRenderTarget.IsDisposed) return;
			Asset<Texture2D> texture = TextureAssets.Projectile[Type];
			baseColorRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, texture.Width(), texture.Height(), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
}

namespace Origins.Buffs {
	public class Gelatin_Aspect_Buff : ModBuff {
		AutoLoadingAsset<Texture2D> overlayTexture;
		public override void SetStaticDefaults() {
			overlayTexture = Texture + "_Overlay";
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.lightPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;

			int projType = ModContent.ProjectileType<Gelatin_Buddy>();

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0) {
				IEntitySource entitySource = player.GetSource_Buff(buffIndex);

				Projectile.NewProjectile(entitySource, player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
			}
		}

		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			Color color = Gelatin_Bloom_Brooch.GetGolor(Main.LocalPlayer?.name);
			if (color == Color.White) return;

			spriteBatch.Draw(overlayTexture, drawParams.Position, drawParams.SourceRectangle, color.MultiplyRGBA(drawParams.DrawColor), 0f, default, 1f, SpriteEffects.None, 0f);
		}
	}
}