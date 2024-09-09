using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using Origins.Projectiles;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
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
		public static float GetControlDir(Player player) {
			bool controlForewards = player.direction == 1 ? player.controlRight : player.controlLeft;
			bool controlBackwards = player.direction == -1 ? player.controlRight : player.controlLeft;
			if (player.controlUp || controlBackwards) {
				if (!player.controlDown && !controlForewards) return -0.75f;
				return 0f;
			} else if (player.controlDown || controlForewards) {
				return 1f;
			}
			return player.mount._frameExtraCounter;
		}
		public override void SetMount(Player player, ref bool skipDust) {
			player.mount._frameExtraCounter = GetControlDir(player);
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
				GetControlDir(player),
				control_speed
			);
			Rectangle hitbox = new(0, 0, 12, 12);
			Vector2 position = (player.MountedCenter + new Vector2(0, 22)).RotatedBy(player.fullRotation, player.position + player.fullRotationOrigin - new Vector2(0, 10));//player.RotatedRelativePointOld(player.MountedCenter + new Vector2(0, 16).RotatedBy(player.fullRotation));
			Vector2 move = new Vector2(16 * player.direction, 0).RotatedBy(player.fullRotation);
			Item item = player.miscEquips[3].type == Indestructible_Saddle.ID ? player.miscEquips[3] : ContentSamples.ItemsByType[Indestructible_Saddle.ID];
			void Explode() {//TODO: replace placeholder explosion & explode on timeout
				player.mount._abilityCooldown = 0;
				player.mount.Dismount(player);
				Projectile.NewProjectile(
					player.GetSource_ItemUse(item),
					position + move * 2 + new Vector2(8),
					Vector2.Zero,
					ModContent.ProjectileType<Indestructible_Saddle_Explosion>(),
					player.GetWeaponDamage(item),
					player.GetWeaponKnockback(item),
					player.whoAmI
				);
			}
			if (player.mount._abilityCooldown <= 0) {
				Explode();
				return;
			}
			for (int i = 2; i > -1; i--) {
				hitbox.X = (int)(position.X + move.X * i - 8);
				hitbox.Y = (int)(position.Y + move.Y * i - 8);
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
}
