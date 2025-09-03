using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;
namespace Origins.Items.Tools {
	public class Indestructible_Saddle : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Movement",
			"Mount"
		];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToMount(ModContent.MountType<Indestructible_Saddle_Mount>());
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 20);
			Item.DamageType = DamageClasses.Explosive;
			Item.damage = 150;
			Item.knockBack = 10;
			Item.useTime = Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
		}
		public override bool CanUseItem(Player player) {
			return !player.HasBuff<Indestructible_Saddle_Mount_Cooldown>();
		}
	}
	public class Indestructible_Saddle_Mount : ModMount {
		public static int ID { get; private set; }
		public override void Load() {
			On_Mount.CanMount += (orig, self, m, mountingPlayer) => {
				return orig(self, m, mountingPlayer) && !(m == ID && mountingPlayer.HasBuff<Indestructible_Saddle_Mount_Cooldown>());
			};
			MonoModHooks.Add(typeof(Mount).GetProperty(nameof(Mount.AllowDirectionChange)).GetGetMethod(), (Func<Mount, bool> orig, Mount self) => {
				return orig(self) && self.Type != ID;
			});
		}
		public override void SetStaticDefaults() {
			// Movement
			MountData.jumpHeight = 0; // How high the mount can jump.
			MountData.acceleration = 0f; // The rate at which the mount speeds up.
			MountData.jumpSpeed = 0f; // The rate at which the player and mount ascend towards (negative y velocity) the jump height when the jump button is pressed.
			MountData.blockExtraJumps = true; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
			MountData.constantJump = false; // Allows you to hold the jump button down.
			MountData.fallDamage = 0f; // Fall damage multiplier.
			MountData.runSpeed = 0f; // The speed of the mount
			MountData.dashSpeed = 0f; // The speed the mount moves when in the state of dashing.
			MountData.flightTimeMax = 0; // The amount of time in frames a mount can be in the state of flying.
			MountData.spawnDust = No_Dust.ID;
			MountData.textureWidth = 112;
			MountData.textureHeight = 128;
			MountData.heightBoost = 22;
			MountData.yOffset = 22;
			MountData.playerHeadOffset = -10;

			// Misc
			MountData.fatigueMax = 0;
			MountData.buff = ModContent.BuffType<Indestructible_Saddle_Mount_Buff>(); // The ID number of the buff assigned to the mount.

			MountData.totalFrames = 4; // Amount of animation frames for the mount
			MountData.playerYOffsets = Enumerable.Repeat(10, MountData.totalFrames).ToArray();
			// Standing
			MountData.runningFrameCount = 0;
			MountData.runningFrameDelay = 0;
			MountData.runningFrameStart = 0;
			MountData.abilityCooldown = 300;
			ID = Type;
		}
		public static float GetControlDir(Player player, bool isSpawning) {
			bool controlTiltUp = player.controlUp || (player.direction == -1 ? player.controlRight : player.controlLeft);
			bool controlTiltDown = player.controlDown || (!isSpawning && (player.direction == 1 ? player.controlRight : player.controlLeft));
			if (controlTiltUp) {
				if (!controlTiltDown) return -0.75f;
				return 0f;
			} else if (controlTiltDown) {
				return 1f;
			}
			return player.mount._frameExtraCounter;
		}
		public override void SetMount(Player player, ref bool skipDust) {
			player.mount._frameExtraCounter = GetControlDir(player, true);
			player.mount._abilityCooldown = MountData.abilityCooldown;
		}
		public override void UpdateEffects(Player player) {
			player.velocity *= 0.935f;
			player.velocity += GeometryUtils.Vec2FromPolar(2, player.direction * (MathHelper.PiOver2 + player.mount._frameExtraCounter) - MathHelper.PiOver2);
			player.gravity = 0;
			player.maxFallSpeed = 35;
			const float control_speed = 0.02f;
			OriginExtensions.LinearSmoothing(
				ref player.mount._frameExtraCounter,
				GetControlDir(player, false),
				control_speed
			);
			Rectangle hitbox = new(0, 0, 12, 12);
			Vector2 position = (player.MountedCenter + new Vector2(0, 22)).RotatedBy(player.fullRotation, player.position + player.fullRotationOrigin - new Vector2(0, 10));//player.RotatedRelativePointOld(player.MountedCenter + new Vector2(0, 16).RotatedBy(player.fullRotation));
			Vector2 move = new Vector2(16 * player.direction, 0).RotatedBy(player.fullRotation);
			Item item = player.miscEquips[3].type == Indestructible_Saddle.ID ? player.miscEquips[3] : ContentSamples.ItemsByType[Indestructible_Saddle.ID];
			void Explode() {
				Projectile.NewProjectile(
					player.GetSource_ItemUse(item),
					position + move * 2 + new Vector2(8),
					Vector2.Zero,
					ModContent.ProjectileType<Indestructible_Saddle_Explosion>(),
					player.GetWeaponDamage(item),
					player.GetWeaponKnockback(item),
					player.whoAmI,
					ai2: player.mount._abilityCooldown
				);
				player.mount._abilityCooldown = 0;
				player.mount.Dismount(player);
			}
			Dust.NewDustPerfect(position - move * 2.5f - move.RotatedBy(MathHelper.PiOver2 * player.direction) * Main.rand.NextFloat(-0.1f, 0.5f), 6, player.velocity * 0.85f).noGravity = true;
			if (player.mount._abilityCooldown <= 0) {
				Explode();
				return;
			}
			for (int i = 2; i > -1; i--) {
				hitbox.X = (int)(position.X + move.X * i - 6);
				hitbox.Y = (int)(position.Y + move.Y * i - 6);
				//hitbox.DrawDebugOutline();
				if (hitbox.OverlapsAnyTiles()) {
					Explode();
					return;
				}
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.CanBeHitBy(player, item, false) && hitbox.Intersects(npc.Hitbox)) {
						Explode();
						return;
					}
				}
				if (player.hostile) {
					foreach (Player other in Main.ActivePlayers) {
						if (other.hostile && other.team != player.team && hitbox.Intersects(other.Hitbox)) {
							Explode();
							return;
						}
					}
				}
			}
			player.GoingDownWithGrapple = true;
			if (Collision.SolidCollision(player.position + player.velocity, player.width, player.height)) {
				player.mount._abilityCooldown /= 3;
				Projectile.NewProjectile(
					player.GetSource_ItemUse(item),
					position,
					player.velocity,
					ModContent.ProjectileType<Indestructible_Saddle_Projectile>(),
					player.GetWeaponDamage(item),
					player.GetWeaponKnockback(item),
					player.whoAmI,
					player.mount._frameExtraCounter,
					player.direction,
					player.mount._abilityCooldown
				);
				player.Hurt(PlayerDeathReason.ByOther(0), (int)(player.velocity.Length() * 3), -player.direction, true, cooldownCounter: ImmunityCooldownID.WrongBugNet, dodgeable: false);
				player.mount.Dismount(player);
				return;
			}
			//Dust.NewDustPerfect(player.position + player.fullRotationOrigin - new Vector2(0, 10), 27, Vector2.Zero).noGravity = true;
		}
		public override void Dismount(Player player, ref bool skipDust) {
			int timeLeft = player.mount._abilityCooldown;
			player.AddBuff(ModContent.BuffType<Indestructible_Saddle_Mount_Cooldown>(), 60 + (timeLeft > 0 ? 30 : 0) + (int)(timeLeft * 1.5f));
		}
		public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
			if (++mountedPlayer.mount._frameCounter >= 5) {
				mountedPlayer.mount._frameCounter = 0;
				if (++mountedPlayer.mount._frame >= 4) {
					mountedPlayer.mount._frame = 0;
				}
			}
			mountedPlayer.fullRotation = mountedPlayer.direction * mountedPlayer.mount._frameExtraCounter;
			mountedPlayer.fullRotationOrigin = new(mountedPlayer.width * 0.5f, 52);
			return false;
		}
		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
			//rotation += drawPlayer.direction * drawPlayer.mount._frameExtraCounter;
			drawOrigin.X += 8 * drawPlayer.direction;
			return true;
		}
	}
	public class Indestructible_Saddle_Projectile : ModProjectile {
		public override string Texture => "Origins/Items/Tools/Indestructible_Saddle_Mount_Back";
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults() {
			Projectile.width = Projectile.height = 0;
			Projectile.friendly = true;
		}
		Vector2 HitboxMovement => new Vector2(16 * Projectile.ai[1], 0).RotatedBy(Projectile.ai[1] * Projectile.ai[0]);
		public override void AI() {
			if (Projectile.ai[2].Cooldown()) {
				Projectile.Kill();
				return;
			}
			Projectile.velocity *= 0.935f;
			Projectile.velocity += GeometryUtils.Vec2FromPolar(2, Projectile.ai[1] * (MathHelper.PiOver2 + Projectile.ai[0]) - MathHelper.PiOver2);
			Player owner = Main.player[Projectile.owner];
			Rectangle hitbox = new(0, 0, 12, 12);
			Vector2 move = HitboxMovement;
			for (int i = 3; i > 0; i--) {
				hitbox.X = (int)(Projectile.position.X + move.X * i - 6);
				hitbox.Y = (int)(Projectile.position.Y + move.Y * i - 6);
				//hitbox.DrawDebugOutline();
				if (hitbox.OverlapsAnyTiles()) {
					Projectile.Kill();
					return;
				}
				if (owner.hostile) {
					foreach (Player other in Main.ActivePlayers) {
						if (other.hostile && other.team != owner.team && hitbox.Intersects(other.Hitbox)) {
							Projectile.Kill();
							return;
						}
					}
				}
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Rectangle hitbox = new(0, 0, 12, 12);
			Vector2 move = HitboxMovement;
			for (int i = 3; i > 0; i--) {
				hitbox.X = (int)(Projectile.position.X + move.X * i - 6);
				hitbox.Y = (int)(Projectile.position.Y + move.Y * i - 6);
				if (hitbox.Intersects(targetHitbox)) return true;
			}
			return false;
		}
		public override void OnKill(int timeLeft) {
			Projectile.NewProjectile(
				Projectile.GetSource_Death(),
				Projectile.position + HitboxMovement * 2 + new Vector2(8),
				Vector2.Zero,
				ModContent.ProjectileType<Indestructible_Saddle_Explosion>(),
				Projectile.damage,
				Projectile.knockBack,
				Projectile.owner,
				ai2: Projectile.ai[2]
			);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.position - Main.screenPosition,
				TextureAssets.Projectile[Type].Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				lightColor,
				Projectile.ai[1] * Projectile.ai[0],
				new(48, 16),
				1,
				Projectile.ai[1] == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally 
			);
			return false;
		}
	}
	public class Indestructible_Saddle_Mount_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Indestructible_Saddle_Mount_Buff";
		public override void SetStaticDefaults() {
			BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
				mountID = ModContent.MountType<Indestructible_Saddle_Mount>()
			};
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
		}
	}
	public class Indestructible_Saddle_Mount_Cooldown : ModBuff {
		public override string Texture => "Origins/Buffs/Indestructible_Saddle_Mount_Cooldown";
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
		}
	}
	public class Indestructible_Saddle_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.Explosive;
		public override int Size => 144;
		public override bool DealsSelfDamage => true;
	}
	public class Indestructible_Saddle_Crit_Type : CritType<Indestructible_Saddle> {
		static int CritThreshold => 120;
		public override LocalizedText Description => base.Description.WithFormatArgs(CritThreshold / 60f);
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => projectile.ai[2] <= CritThreshold;
		public override float CritMultiplier(Player player, Item item) => 3f;
	}
}
