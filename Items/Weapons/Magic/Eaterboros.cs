using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
namespace Origins.Items.Weapons.Magic {
	public class Eaterboros : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Sword
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 27;
			Item.DamageType = DamageClasses.MeleeMagic;
			Item.mana = 6;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 15;
			Item.useAnimation = 20;
			Item.shoot = ModContent.ProjectileType<Eaterboros_Slash>();
			Item.shootSpeed = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6f;
			Item.useTurn = false;
			Item.channel = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1; 
		}
		public override bool MeleePrefix() => true;
		public bool? Hardmode => false;
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) type = ModContent.ProjectileType<Eaterboros_Shoot>();
		}
	}
	public class Eaterboros_Slash : ModProjectile {
		public const int base_segments = 2;
		public const int max_segments = 7;
		public const float swing_angle_extend = 0.5f;
		const float swing_angle_extend_factor = max_segments / swing_angle_extend;
		public override string Texture => "Origins/Items/Weapons/Magic/Eaterboros_Hilt";
		static AutoLoadingAsset<Texture2D> eaterTexture = "Origins/Items/Weapons/Magic/Eaterboros_Segment_Attached";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.DamageType = DamageClasses.MeleeMagic;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
			Projectile.noEnchantmentVisuals = true;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = base_segments;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead) {
				Projectile.active = false;
				return;
			}
			bool newEater = false;
			if (player.channel) {
				player.itemAnimation = player.itemAnimationMax;
				player.TryUpdateChannel(Projectile);

				int before = (int)Projectile.ai[2];
				Projectile.ai[2] += 1f / player.itemTimeMax;
				newEater = before != (int)Projectile.ai[2];
				if ((newEater && !player.CheckMana(player.HeldItem, pay: true)) || Projectile.ai[2] >= max_segments) player.TryCancelChannel(Projectile);
				player.manaRegenDelay = (int)player.maxRegenDelay;

				if (Main.myPlayer == Projectile.owner) {
					Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(default);
					if (direction.X != Projectile.velocity.X || direction.Y != Projectile.velocity.Y) {
						// This will sync the projectile, most importantly, the velocity.
						Projectile.netUpdate = true;
					}
					Projectile.velocity = direction;
				}
				Projectile.friendly = false;
			} else {
				if (!Projectile.friendly) SoundEngine.PlaySound(player.HeldItem.UseSound, Projectile.Center);
				Projectile.friendly = true;
			}
			if (Projectile.localAI[2] < Projectile.ai[2]) Projectile.localAI[2] = Projectile.ai[2];
			Projectile.ai[1] = player.direction = Math.Sign(Projectile.velocity.X);

			float swingFactor = 1 - player.itemAnimation / (float)player.itemAnimationMax;
			swingFactor = MathHelper.Lerp(MathF.Pow(swingFactor, 2f), MathF.Pow(swingFactor, 0.5f), swingFactor * swingFactor);
			Projectile.rotation = MathHelper.Lerp(-2f, 2f, swingFactor) * Projectile.ai[1] * (1 + Projectile.localAI[2] / swing_angle_extend_factor);
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.timeLeft = player.itemAnimation * Projectile.MaxUpdates;
			player.heldProj = Projectile.whoAmI;

			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetCompositeArmPosition(false) + (Vector2)new PolarVec2(0, realRotation);

			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation - MathHelper.PiOver4 * Projectile.ai[1]) * Projectile.width * Projectile.scale;
			Vector2 boxPos = default;
			Vector2 halfSize = Projectile.Size * Projectile.scale / 2;
			for (int j = 0; j <= Projectile.ai[2] + 1; j++) {
				boxPos = Projectile.Center + vel * (j + 0.75f) - halfSize;
				Projectile.EmitEnchantmentVisualsAt(boxPos, (int)halfSize.X, (int)halfSize.Y);
			}
			if (newEater) {
				for (int i = 0; i < 4; i++) {
					Dust.NewDust(
						boxPos,
						Projectile.width,
						Projectile.height,
						DustID.ScourgeOfTheCorruptor
					);
				}
			}
			player.itemTime = 2;
		}
		public override bool ShouldUpdatePosition() => false;
		int collideIndex = -1;
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Player player = Main.player[Projectile.owner];
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation * player.gravDir - MathHelper.PiOver4 * Projectile.ai[1] * player.gravDir) * Projectile.width * Projectile.scale;
			collideIndex = -1;
			for (int j = 0; j <= Projectile.ai[2] + 1; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * (j + 0.5f);
				hitbox.Offset((int)offset.X, (int)offset.Y);
				collideIndex = j;
				if (hitbox.Intersects(targetHitbox)) return true;
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (collideIndex != -1) {
				if (Main.myPlayer == Projectile.owner) {
					Player player = Main.player[Projectile.owner];
					Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation * player.gravDir - MathHelper.PiOver4 * Projectile.ai[1] * player.gravDir) * Projectile.width * Projectile.scale;
					Vector2 halfSize = Projectile.Size * Projectile.scale / 2;
					int projType = ModContent.ProjectileType<Eaterboros_Segment_Free>();
					int lastProj = -1;
					for (int i = (int)Projectile.ai[2] + 2; --i > collideIndex;) {
						lastProj = Projectile.NewProjectile(
							Projectile.GetSource_OnHit(target),
							Projectile.Center + vel * (i + 0.75f),
							vel.RotatedBy(Projectile.ai[1] * 0.4f) * 8f / Projectile.width,
							projType,
							Projectile.damage / 2,
							Projectile.knockBack / 5,
							Projectile.owner,
							lastProj
						);
						for (int j = 0; j < 4; j++) {
							Dust.NewDust(
								Projectile.Center + vel * (i + 0.75f) - halfSize,
								Projectile.width,
								Projectile.height,
								DustID.ScourgeOfTheCorruptor
							);
						}
					}
					Projectile.ai[2] = collideIndex - 2;
				}
			}
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation - MathHelper.PiOver4 * Projectile.ai[1]).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			float rotation = Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() - MathHelper.PiOver4 * Projectile.ai[1] * player.gravDir;
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation * player.gravDir - MathHelper.PiOver4 * Projectile.ai[1] * player.gravDir) * Projectile.width * Projectile.scale;
			SpriteEffects effects = Projectile.ai[1] * player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			for (int i = (int)Projectile.ai[2] + 2; --i > 0;) {
				Main.EntitySpriteDraw(
					eaterTexture,
					(Projectile.Center + vel * (i + 0.75f)) - Main.screenPosition,
					null,
					lightColor,
					rotation,
					new Vector2(18, 5),// origin point in the sprite, 'round which the whole sword rotates
					Projectile.scale,
					effects,
					0
				);
			}
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				new Vector2(0, 13),// origin point in the sprite, 'round which the whole sword rotates
				Projectile.scale,
				effects,
				0
			);
			return false;
		}
	}
	public class Eaterboros_Segment_Free : ModProjectile {
		static int frameIndex = 0;
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.TinyEater);
			Projectile.DamageType = DamageClasses.MeleeMagic;
			Projectile.width = Projectile.height = 26;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.friendly = false;
			Projectile.alpha = 0;
			Projectile.frame = frameIndex = (frameIndex + 1) % Main.projFrames[Type];
			DrawOriginOffsetY = 6;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj) {
				Projectile.scale *= parentProj.scale;
			}
		}
		public override bool ShouldUpdatePosition() => Projectile.ai[0] == -1;
		public override void AI() {
			int lastIndex = (int)Projectile.ai[0];
			if (lastIndex != -1) {
				Projectile last = Main.projectile[lastIndex];
				if (!last.active || last.type != Type) {
					Projectile.ai[0] = -1;
					return;
				}
				float dX = last.Center.X - Projectile.Center.X;
				float dY = last.Center.Y - Projectile.Center.Y;
				Projectile.rotation = (float)Math.Atan2(dY, dX);
				if (dX != 0f || dY != 0f) {
					float dist = (float)Math.Sqrt(dY * dY + dX * dX);
					dist = (dist - 16 * Projectile.scale) / dist;
					dX *= dist;
					dY *= dist;
					Projectile.position.X += dX;
					Projectile.position.Y += dY;
				}
				Projectile.velocity = last.velocity;
			} else {
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.friendly = true;
				float targetDist = 600f * 600f;
				Vector2 targetCenter = Projectile.position;
				int target = -1;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.CanBeChasedBy()) {
						Vector2 diff = npc.Hitbox.ClosestPointInRect(Projectile.Center) - Projectile.Center;
						float dist = diff.LengthSquared();
						if (dist < targetDist) {
							targetDist = dist;
							targetCenter = npc.Center;
							target = npc.whoAmI;
						}
					}
				}
				if (target != -1) {
					float turnSpeed = 2f;
					float currentSpeed = Projectile.velocity.Length();
					Projectile.velocity = Vector2.Normalize(Projectile.velocity + (targetCenter - Projectile.Center).SafeNormalize(default) * turnSpeed) * currentSpeed;
				}
			}
			if (++Projectile.frameCounter > 5) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 14;
			height = 14;
			return true;
		}
		public override void OnKill(int timeLeft) {
			for (int j = 0; j < 4; j++) {
				Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.ScourgeOfTheCorruptor
				);
			}
		}
	}
	public class Eaterboros_Shoot : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Eaterboros_Hilt";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.DamageType = DamageClasses.MeleeMagic;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
			Projectile.noEnchantmentVisuals = true;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = float.PositiveInfinity;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead) {
				Projectile.active = false;
				return;
			}
			if (player.controlUseTile) {
				//player.TryUpdateChannel(Projectile);

				if (Main.myPlayer == Projectile.owner) {
					Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(default);
					if (direction.X != Projectile.velocity.X || direction.Y != Projectile.velocity.Y) {
						// This will sync the projectile, most importantly, the velocity.
						Projectile.netUpdate = true;
					}
					Projectile.velocity = direction;
				}
				Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
				Projectile.timeLeft = player.itemAnimation * Projectile.MaxUpdates;
				player.heldProj = Projectile.whoAmI;
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

				if (++Projectile.ai[2] >= player.itemAnimationMax) {
					Projectile.ai[2] = 0;
					if (player.CheckMana(player.HeldItem, pay: true)) {
						Vector2 vel = Projectile.velocity * Projectile.width * Projectile.scale;
						Vector2 halfSize = Projectile.Size * Projectile.scale / 2;
						int projType = ModContent.ProjectileType<Eaterboros_Segment_Free>();
						int lastProj = -1;
						for (int i = 2; i-- > 0;) {
							lastProj = Projectile.NewProjectile(
								Projectile.GetSource_FromAI(),
								Projectile.Center + vel * (i + 0.75f),
								Projectile.velocity * 8 * player.GetTotalAttackSpeed(DamageClass.Melee),
								projType,
								Projectile.damage / 4,
								Projectile.knockBack / 3,
								Projectile.owner,
								lastProj
							);
							for (int j = 0; j < 4; j++) {
								Dust.NewDust(
									Projectile.Center + vel * (i + 0.75f) - halfSize,
									Projectile.width,
									Projectile.height,
									DustID.ScourgeOfTheCorruptor
								);
							}
						}
						SoundEngine.PlaySound(player.HeldItem.UseSound, Projectile.Center);
					} else {
						Projectile.Kill();
						//player.TryCancelChannel(Projectile);
					}
				}
				player.manaRegenDelay = (int)player.maxRegenDelay;
				player.SetDummyItemTime(5);
			} else {
				Projectile.Kill();
			}
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				new Vector2(0, 13),// origin point in the sprite, 'round which the whole sword rotates
				Projectile.scale,
				Projectile.ai[1] > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically,
				0
			);
			return false;
		}
	}
}
