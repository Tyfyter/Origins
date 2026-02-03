using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Other.Consumables.Broths;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Sunflower_In_A_Pot : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the player target anywhere on the whole screen while using a controller
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 9;
			Item.knockBack = 1f;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 14;
			Item.shootSpeed = 9f;
			Item.width = 24;
			Item.height = 38;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = true;
			Item.value = Item.sellPrice(silver: 1, copper: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item44;
			Item.buffType = Sunny_Sunflower_Buff.ID;
			Item.shoot = Sunflower_Sunny.ID;
			Item.noMelee = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.ClayPot)
			.AddIngredient(ItemID.DirtBlock, 5)
			.AddIngredient(ItemID.FallenStar, 3)
			.AddIngredient(ItemID.Sunflower)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Sunny_Sunflower_Buff : MinionBuff {
		public static int ID { get; private set; }
		public override IEnumerable<int> ProjectileTypes() => [
			Sunflower_Sunny.ID
		];
		public override bool IsArtifact => true;
	}
}
namespace Origins.Items.Weapons.Summoner.Minions {
	[LegacyName("Sunny_Sunflower")]
	public class Sunflower_Sunny : ModProjectile, IArtifactMinion {
		public int MaxLife { get; set; }
		public float Life { get; set; }
		public static int ID { get; private set; }
		public static Dictionary<BrothBase, Asset<Texture2D>> BrothTextures { get; } = [];
		public static Dictionary<BrothBase, Asset<Texture2D>> BrothGlowTextures { get; } = [];
		//public override string Texture => typeof(Sunny_Sunflower).GetDefaultTMLName() + "_Base";
		//static readonly AutoLoadingAsset<Texture2D> headTexture = typeof(Sunny_Sunflower).GetDefaultTMLName() + "_Head";
		Vector2 HeadCenterOffset => new(Projectile.width * 0.5f, 6);
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 15;
			// Sets the amount of frames this minion has on its spritesheet
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ID = Type;

			if (Main.dedServ) return;
			const string texture_path = "Origins/Items/Weapons/Summoner/Minions/Sunflower_";
			BrothTextures.Add(ModContent.GetInstance<Spicy_Broth>(), ModContent.Request<Texture2D>(texture_path + "Firey"));
			BrothTextures.Add(ModContent.GetInstance<Greasy_Broth>(), ModContent.Request<Texture2D>(texture_path + "Greasy"));
			BrothTextures.Add(ModContent.GetInstance<Minty_Broth>(), ModContent.Request<Texture2D>(texture_path + "Icy"));
			BrothTextures.Add(ModContent.GetInstance<Savory_Broth>(), ModContent.Request<Texture2D>(texture_path + "Shadowy"));
			BrothGlowTextures.Add(ModContent.GetInstance<Savory_Broth>(), ModContent.Request<Texture2D>(texture_path + "Shadowy_Glow"));
			BrothTextures.Add(ModContent.GetInstance<Sharp_Broth>(), ModContent.Request<Texture2D>(texture_path + "Zappy"));
		}

		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			Projectile.manualDirectionChange = true;
			Projectile.netImportant = true;
			MaxLife = 100;
		}
		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];

			#region Active check
			// This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
			if (player.dead || !player.active) {
				player.ClearBuff(Sunny_Sunflower_Buff.ID);
			} else if (player.HasBuff(Sunny_Sunflower_Buff.ID)) {
				Projectile.timeLeft = 2;
			}
			#endregion

			#region General behavior
			Vector2 idlePosition = player.Bottom - new Vector2(player.direction * (Projectile.minionPos + 1) * 32, Projectile.height * 0.5f);

			// die if distance is too big
			Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
			float distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == player.whoAmI) {
				if (distanceToIdlePosition > 400) {
					if (distanceToIdlePosition > 2000) {
						Projectile.position = idlePosition;
						Projectile.velocity *= 0.1f;
						Projectile.netUpdate = true;
					} else if (Projectile.localAI[1] <= 0) {
						Projectile.ai[2] = 1;
						Projectile.netUpdate = true;
					}
				}
			}
			if (Projectile.ai[2] == 1) {
				Projectile.localAI[1] = 300;
				float speed = 8f;
				float inertia = 12f;
				Vector2 direction = vectorToIdlePosition * (speed / distanceToIdlePosition);
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				Projectile.tileCollide = false;
				Projectile.direction = Math.Sign(vectorToIdlePosition.X);
				if (++Projectile.frameCounter >= 20) Projectile.frameCounter = 0;
				Projectile.frame = 9;
				if (distanceToIdlePosition > 64 || Projectile.Hitbox.OverlapsAnyTiles()) return;
				Rectangle floorbox = Projectile.Hitbox;
				floorbox.Offset(0, Projectile.height);
				floorbox.Height = 16 * 4;
				if (!floorbox.OverlapsAnyTiles(false)) return;
				Projectile.ai[2] = 0;
				Projectile.netUpdate = true;
			}
			Projectile.localAI[1]--;
			Projectile.tileCollide = true;
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
					Vector2 offset = HeadCenterOffset;
					int dir = Math.Sign(npc.Center.X - (pos.X + offset.X));
					Vector2 stepDown = new(16, 32);
					for (int i = 0; i < 5; i++) {
						if (i != 0 && !CanWalkOnto(pos, dir)) break;
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						between *= isCurrentTarget ? 0 : 1 + i / 8f;
						bool closer = distanceFromTarget > between;
						if (!closer) break;
						bool lineOfSight = CollisionExt.CanHitRay(pos + offset, npc.Center);
						
						for (int j = 0; j < 40 && !lineOfSight; j++) {
							lineOfSight = CollisionExt.CanHitRay(pos + offset, Main.rand.NextVector2FromRectangle(npc.Hitbox));
						}
						if (lineOfSight) {
							distanceFromTarget = between;
							targetCenter = npc.Center;
							target = npc.whoAmI;
							foundTarget = true;
							hasPriorityTarget = isPriorityTarget;
							bestTargetIsVisible = i == 0;
							break;
						}
						if (Framing.GetTileSafely(pos + stepDown).IsHalfBlock) {
							pos.Y -= 16;
						}
						pos.X += 16 * dir;
					}
				}
			}
			bool foundTarget = player.GetModPlayer<OriginPlayer>().GetMinionTarget(targetingAlgorithm);
			#endregion

			#region Movement
			if (Projectile.velocity.X != 0) Projectile.direction = Math.Sign(Projectile.velocity.X);
			const float walkSpeed = 0.5f;
			const float walkDrag = 0.95f;
			if (foundTarget) {
				Vector2 diff = targetCenter - Projectile.Center;
				Projectile.direction = Math.Sign(diff.X);
				if (bestTargetIsVisible) {
					if (++Projectile.ai[1] >= 25) {
						SoundEngine.PlaySound(Origins.Sounds.EnergyRipple.WithPitch(1f).WithVolume(0.25f), Projectile.Center);
						if (Main.myPlayer == player.whoAmI) Projectile.NewProjectile(
							Projectile.GetSource_FromAI(),
							Projectile.position + HeadCenterOffset,
							diff.SafeNormalize(default) * 16,
							ModContent.ProjectileType<Sunflower_Sunny_P>(),
							Projectile.damage,
							Projectile.knockBack,
							Projectile.owner
						);
						Projectile.ai[0] = -1;
						Projectile.ai[1] = 0;
					}
				} else {
					Projectile.velocity.X += walkSpeed * Math.Sign(diff.X);
				}
				Projectile.velocity.X *= walkDrag;
			} else {
				if (distanceToIdlePosition > 100) {
					Projectile.velocity.X += walkSpeed * Math.Sign(vectorToIdlePosition.X);
				}
				Projectile.velocity.X *= walkDrag;
				OriginExtensions.AngularSmoothing(ref Projectile.rotation, MathHelper.PiOver2 - 1.6f * Math.Sign(vectorToIdlePosition.X), 0.05f);
			}
			/*if (Projectile.velocity.Y < 0.8f && Projectile.velocity.Y >= 0f && !CanWalkOnto(Projectile.position + Projectile.velocity, Projectile.direction)) {
				Projectile.velocity.X = 0;
			}*/
			#endregion

			#region Animation and visuals
			if (Math.Abs(Projectile.velocity.X) <= 0.1f) Projectile.velocity.X = 0;
			// This is a simple "loop through all frames from top to bottom" animation
			if (Projectile.velocity.X == 0 || foundTarget && bestTargetIsVisible) {
				if (++Projectile.frameCounter >= 5) {
					Projectile.frameCounter = 0;
					if (++Projectile.frame >= 8) Projectile.frame = 0;
				}
			} else if (++Projectile.frameCounter >= 5) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type]) Projectile.frame = 8;
			}
			#endregion

			if (Projectile.localAI[2] <= 0) {
				if (this.GetHurtByHostiles()) {
					// add sound
					Projectile.localAI[2] = 20;
				}
			} else {
				Projectile.localAI[2]--;
			}
			if (Projectile.lavaWet) {
				Projectile.localAI[2] = -1;
				this.DamageArtifactMinion(200);
			}
			if (Main.dayTime) {
				foreach (Player healee in Main.ActivePlayers) {
					if (healee.team == player.team && Projectile.Center.Clamp(healee.Hitbox).WithinRange(Projectile.Center, 16 * 15)) {
						healee.lifeRegen += 2;
						Dust dust = Dust.NewDustDirect(
							healee.position,
							healee.width,
							healee.height,
							DustID.YellowTorch,
							SpeedY: -2
						);
						dust.velocity *= 0.5f;
						dust.noGravity = true;
					}
				}
			}
			if (Projectile.velocity.Y != 0) {
				Projectile.frameCounter = 0;
				Projectile.frame = 9;
			}
			if (Projectile.velocity.Y == 0 && vectorToIdlePosition.Y < 16 * 2) {
				Rectangle floorbox = Projectile.Hitbox;
				floorbox.Inflate(-8, 0);
				floorbox.Offset((int)(Projectile.velocity.X * 4 + Projectile.direction * 8), Projectile.height - 16);
				floorbox.Height = 16 * 7;
				if (!floorbox.OverlapsAnyTiles(false)) {
					Projectile.direction = Math.Sign(Projectile.velocity.X);
					Projectile.velocity.X = 0;
				}
			}
			Projectile.velocity.Y += 0.4f;
		}
		public bool CanWalkOnto(Vector2 position, int dir = 0) {
			List<Point> tiles = Collision.GetTilesIn(position + new Vector2(0, Projectile.height), position + Projectile.Size);
			bool[] solid = new bool[3];
			for (int i = 0; i < tiles.Count; i++) {
				solid[i] = Framing.GetTileSafely(tiles[i]).HasSolidTile();
			}

			if (tiles.Count == 3) {
				solid[1 - dir] = true;
				solid[1] = true;
				Tile centerTile = Framing.GetTileSafely(tiles[1]);
				if (centerTile.HasSolidTile()) {
					if (centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownLeft) return true;

					if (centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownRight) return true;
				}
				centerTile = Framing.GetTileSafely(tiles[0]);
				if (centerTile.HasSolidTile() && (centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownLeft)) return true;

				centerTile = Framing.GetTileSafely(tiles[2]);
				if (centerTile.HasSolidTile() && (centerTile.IsHalfBlock || centerTile.Slope == SlopeType.SlopeDownRight)) return true;
			}
			for (int i = 0; i < tiles.Count; i++) {
				if (!solid[i]) return false;
			}
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) {
				Vector2 pos = Projectile.position;
				Collision.StepDown(ref Projectile.position, ref oldVelocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
				Collision.StepUp(ref Projectile.position, ref oldVelocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);

				if (Projectile.position == pos) {
					int dir = Math.Sign(oldVelocity.X);
					Vector2 collisionPos = (Projectile.Bottom + new Vector2(18 * dir, 0));
					if (Framing.GetTileSafely(collisionPos.ToTileCoordinates()).HasFullSolidTile() && !Framing.GetTileSafely((collisionPos - new Vector2(0, 12)).ToTileCoordinates()).HasFullSolidTile()) {
						Projectile.velocity.Y = -5;
					}
				}
			}
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			//fallThrough = targetIsBelow;
			return true;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 0.5f;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(Projectile.localAI[2] == -1 ? SoundID.NPCDeath66 : SoundID.NPCDeath1, Projectile.Center);
		}
		public void OnHurt(int damage, bool fromDoT) {
			if (fromDoT) return;
			if (Life > 0) SoundEngine.PlaySound(SoundID.FemaleHit.WithPitch(1f).WithVolume(0.25f), Projectile.Center);
		}
		public override bool PreDraw(ref Color lightColor) {
			Asset<Texture2D> textureAsset = null;
			Asset<Texture2D> glowTexture = null;
			if (Main.player[Projectile.owner]?.OriginPlayer()?.broth is BrothBase broth) {
				BrothTextures.TryGetValue(broth, out textureAsset);
				BrothGlowTextures.TryGetValue(broth, out glowTexture);
			}
			textureAsset ??= TextureAssets.Projectile[Type];
			Texture2D baseTexture = textureAsset.Value;
			Rectangle baseFrame = baseTexture.Frame(verticalFrames: Main.projFrames[Projectile.type], frameY: Projectile.frame);
			SpriteEffects baseEffects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Vector2 offset = Vector2.UnitY * 2;
			if (Projectile.localAI[2] > 0 && Projectile.localAI[2] % 10 > 5) {
				lightColor *= 0.3f;
			}
			Vector2 gfxOffset = new(0, Projectile.gfxOffY);
			if (Projectile.ai[2] == 1) {
				Main.instance.LoadProjectile(ProjectileID.DandelionSeed);
				Texture2D wingTexture = TextureAssets.Projectile[ProjectileID.DandelionSeed].Value;
				for (int i = -1; i <= 1; i++) {
					int frameNum = ((Projectile.frameCounter / 5) + i + 1) % 4;
					Rectangle wingFrame = wingTexture.Frame(horizontalFrames: 4, frameX: frameNum);
					Vector2 wingOrigin = wingFrame.Size() * new Vector2(0.5f, 1);
					switch (frameNum) {
						case 1:
						wingOrigin.X += 2 * Projectile.direction;
						break;
						case 3:
						wingOrigin.X -= 2 * Projectile.direction;
						break;
					}
					Main.EntitySpriteDraw(
						wingTexture,
						Projectile.Top + new Vector2(((baseFrame.Width - 8) / 2) * i, 2 + Math.Abs(i) * 6) + gfxOffset - Main.screenPosition,
						wingFrame,
						lightColor,
						0,
						wingOrigin,
						Projectile.scale,
						baseEffects
					);
				}
			}
			DrawData data = new(
				baseTexture,
				Projectile.Bottom + offset + gfxOffset - Main.screenPosition,
				baseFrame,
				lightColor,
				0,
				baseFrame.Size() * new Vector2(0.5f, 1),
				Projectile.scale,
				baseEffects
			);
			Main.EntitySpriteDraw(data);
			if (glowTexture is not null) {
				data.texture = glowTexture.Value;
				data.color = Color.White;
				Main.EntitySpriteDraw(data);
			}
			return false;
		}
	}
	[LegacyName("Sunny_Sunflower_P")]
	public class Sunflower_Sunny_P : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 6;
			Projectile.height = 4;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 0;
			Projectile.extraUpdates = 1;
			Projectile.ArmorPenetration += 5;
		}
		public override void AI() {
			const int HalfSpriteWidth = 32 / 2;

			int HalfProjWidth = Projectile.width / 2;
			if (Projectile.spriteDirection == 1) {
				DrawOriginOffsetX = -(HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = (int)-DrawOriginOffsetX * 2;
				DrawOriginOffsetY = 0;
			} else {
				DrawOriginOffsetX = (HalfProjWidth - HalfSpriteWidth);
				DrawOffsetX = 0;
				DrawOriginOffsetY = 0;
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 0);
			Dust dust = Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.YellowTorch,
						SpeedY: -2
					);
			dust.velocity *= 0.5f;
			dust.noGravity = true;
		}
		public override Color? GetAlpha(Color lightColor) => new(255, 255, 255, 100);
	}
}
