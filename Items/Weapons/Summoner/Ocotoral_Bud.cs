using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.NPCs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Summoner {
	public class Ocotoral_Bud : ModItem, ITornSource {
		public static float TornSeverity => 0.5f;
		float ITornSource.Severity => TornSeverity;
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 21;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Thrust;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.shoot = ModContent.ProjectileType<Minions.Barnacle_Turret>();
			Item.shootSpeed = 1;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.sentry = true;
			Item.glowMask = glowmask;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse != 2) {
				Projectile.NewProjectile(source, Main.MouseWorld, default, type, Item.damage, Item.knockBack, player.whoAmI);
				player.UpdateMaxTurrets();
			}
			return false;
		}
	}
}

namespace Origins.Items.Weapons.Summoner.Minions {
	public class Barnacle_Turret : ModProjectile {
		public override string Texture => typeof(Barnacle_Turret).GetDefaultTMLName() + "_Base";
		static readonly AutoLoadingAsset<Texture2D> baseGlowTexture = typeof(Barnacle_Turret).GetDefaultTMLName() + "_Base_Glow";
		static readonly AutoLoadingAsset<Texture2D> headTexture = typeof(Barnacle_Turret).GetDefaultTMLName() + "_Head";
		static readonly AutoLoadingAsset<Texture2D> headGlowTexture = typeof(Barnacle_Turret).GetDefaultTMLName() + "_Head_Glow";
		Vector2 headCenterOffset => new(Projectile.width * 0.5f, 4);
		public override void SetStaticDefaults() {
			// Sets the amount of frames this minion has on its spritesheet
			Main.projFrames[Projectile.type] = 4;
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults() {
			//Projectile.CloneDefaults(ProjectileID.FrostHydra);
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 32;
			Projectile.height = 26;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.sentry = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region General behavior
			Vector2 idlePosition = player.Bottom;

			// die if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
				Projectile.Kill();
				return;
			}
			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.type == Type && other.owner == Projectile.owner && other.Hitbox.Intersects(Projectile.Hitbox)) {
					Projectile.velocity.X += Math.Sign(Projectile.position.X - other.position.X) * 0.03f;
				}
			}
			#endregion

			#region Find target
			// Starting search distance
			float distanceFromTarget = 2000f;
			Vector2 targetCenter = default;
			int target = -1;
			bool hasPriorityTarget = false;
			bool bestTargetIsVisible = true;
			void targetingAlgorithm(NPC npc, float targetPriorityMultiplier, bool isPriorityTarget, ref bool foundTarget) {
				bool isCurrentTarget = npc.whoAmI == Projectile.ai[0];
				if ((isCurrentTarget || isPriorityTarget || !hasPriorityTarget) && npc.CanBeChasedBy()) {
					Vector2 pos = Projectile.position;
					Vector2 offset = headCenterOffset;
					int dir = Math.Sign(npc.Center.X - (pos.X + offset.X));
					for (int i = 0; i < 8; i++) {
						if (i != 0 && !CanWalkOnto(pos, dir)) break;
						float between = Vector2.Distance(npc.Center, pos + offset);
						between *= isCurrentTarget ? 0 : 1 + i / 8f;
						bool closer = distanceFromTarget > between;
						bool lineOfSight = Collision.CanHitLine(pos + offset, 8, 8, npc.position, npc.width, npc.height);
						if ((closer || !foundTarget) && lineOfSight) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							target = npc.whoAmI;
							foundTarget = true;
							hasPriorityTarget = isPriorityTarget;
							bestTargetIsVisible = i == 0;
							break;
						}
						if (Framing.GetTileSafely((pos + new Vector2(16, 28)).ToTileCoordinates()).IsHalfBlock) {
							pos.Y -= 16;
						}
						pos.X += 16 * dir;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);

			#endregion

			#region Movement
			const float walkSpeed = 0.5f;
			const float walkDrag = 0.95f;
			bool resetShootingProgress = true;
			if (foundTarget) {
				Vector2 diff = targetCenter - Projectile.Center;
				if (bestTargetIsVisible) {
					if (OriginExtensions.AngularSmoothing(ref Projectile.rotation, diff.ToRotation(), Projectile.ai[0] == target ? 0.2f : 0.125f)) {
						Projectile.ai[0] = target;
						if (++Projectile.ai[1] >= 45) {
							Projectile.NewProjectile(
								Projectile.GetSource_FromAI(),
								Projectile.position + headCenterOffset,
								diff.SafeNormalize(default) * 16,
								ModContent.ProjectileType<Barnacle_Turret_Shot>(),
								Projectile.damage,
								Projectile.knockBack,
								Projectile.owner
							);
						} else {
							resetShootingProgress = false;
						}
					}
				} else {
					Projectile.velocity.X += walkSpeed * Math.Sign(diff.X);
				}
				Projectile.velocity.X *= walkDrag;
			} else {
				if (distanceToIdlePosition > 600) {
					Projectile.velocity.X += walkSpeed * Math.Sign(vectorToIdlePosition.X);
				}
				Projectile.velocity.X *= walkDrag;
				OriginExtensions.AngularSmoothing(ref Projectile.rotation, MathHelper.PiOver2 - 1.6f * Math.Sign(vectorToIdlePosition.X), 0.05f);
			}
			if (resetShootingProgress) {
				Projectile.ai[0] = -1;
				Projectile.ai[1] = 0;
			}
			if (Projectile.velocity.X != 0) Projectile.direction = Math.Sign(Projectile.velocity.X);
			if (Projectile.velocity.Y < 0.8f && Projectile.velocity.Y >= 0f && !CanWalkOnto(Projectile.position + Projectile.velocity, Projectile.direction)) {
				Projectile.velocity.X = 0;
			}
			Projectile.velocity.Y += 0.4f;
			#endregion

			#region Animation and visuals
			if (Math.Abs(Projectile.velocity.X) <= 0.01f) Projectile.velocity.X = 0;
			// This is a simple "loop through all frames from top to bottom" animation
			if (Projectile.velocity.X == 0) {
				Projectile.frame = 0;
			} else if (++Projectile.frameCounter >= 5) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) {
					Projectile.frame = 0;
				}
			}

			// Some visuals here
			float factor = (1 + (Projectile.ai[1] / 45f));
			Lighting.AddLight(Projectile.Center, new Vector3(0.086f, 0.127f * factor, 0.250f * factor));
			#endregion
		}
		public static bool CanWalkOnto(Vector2 position, int dir = 0) {
			List<Point> tiles = Collision.GetTilesIn(position + new Vector2(0, 28), position + new Vector2(32, 28));
			bool[] solid = new bool[3];
			for (int i = 0; i < tiles.Count; i++) {
				solid[i] = Framing.GetTileSafely(tiles[i]).HasSolidTile();
			}

			if (tiles.Count == 3) {
				solid[1 - dir] = true;
				solid[1] = true;
				Tile centerTile = Framing.GetTileSafely(tiles[1]);
				if (centerTile.HasSolidTile()) {
					if ((centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownLeft) &&
						Framing.GetTileSafely(tiles[0]).HasSolidTile() &&
						Framing.GetTileSafely(tiles[2].X, tiles[2].Y + 1).HasSolidTile()) return true;

					if ((centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownRight) &&
						Framing.GetTileSafely(tiles[2]).HasSolidTile() &&
						Framing.GetTileSafely(tiles[0].X, tiles[0].Y + 1).HasSolidTile()) return true;
				}
			}
			for (int i = 0; i < tiles.Count; i++) {
				if (!solid[i]) return false;
			}
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) {
				int dir = Math.Sign(oldVelocity.X);
				Vector2 collisionPos = (Projectile.Bottom + new Vector2(18 * dir, 0));
				if (Framing.GetTileSafely(collisionPos.ToTileCoordinates()).HasFullSolidTile() && !Framing.GetTileSafely((collisionPos - new Vector2(0, 12)).ToTileCoordinates()).HasFullSolidTile()) {
					Projectile.velocity.Y = -5;
				}
			}
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 24;
			fallThrough = false;
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D baseTexture = TextureAssets.Projectile[Type].Value;
			Vector2 basePosition = Projectile.position - Main.screenPosition + new Vector2(0, 2);
			Rectangle baseFrame = baseTexture.Frame(verticalFrames: Main.projFrames[Projectile.type], frameY: Projectile.frame);
			SpriteEffects baseEffects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Color glowMaskColor = new Color((lightColor.R + 255) / 510f, (lightColor.G + 255) / 510f, (lightColor.B + 255) / 510f, 0.5f) * (1 + (Projectile.ai[1] / 45f));
			Main.EntitySpriteDraw(
				baseTexture,
				basePosition,
				baseFrame,
				lightColor,
				0,
				default,
				Projectile.scale,
				baseEffects
			);
			Main.EntitySpriteDraw(
				baseGlowTexture,
				basePosition,
				baseFrame,
				glowMaskColor,
				0,
				default,
				Projectile.scale,
				baseEffects
			);

			Main.EntitySpriteDraw(
				headTexture,
				basePosition + headCenterOffset,
				null,
				lightColor,
				Projectile.rotation + MathHelper.Pi,
				new Vector2(13, 7),
				Projectile.scale,
				(SpriteEffects)(((int)baseEffects) << 1)
			);
			Main.EntitySpriteDraw(
				headGlowTexture,
				basePosition + headCenterOffset,
				null,
				glowMaskColor,
				Projectile.rotation + MathHelper.Pi,
				new Vector2(13, 7),
				Projectile.scale,
				(SpriteEffects)(((int)baseEffects) << 1)
			);
			return false;
		}
	}
	public class Barnacle_Turret_Shot : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.SentryShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CursedBullet);
			Projectile.DamageType = DamageClass.Summon;
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 0);
			}
			return Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 120, 300, Ocotoral_Bud.TornSeverity, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
	}
}
