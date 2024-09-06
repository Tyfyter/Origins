using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Accessories;
using System;
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
			"Movement"
		];
        public override void SetDefaults() {
			Item.DefaultToMount(ModContent.MountType<Indestructible_Saddle_Mount>());
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
        }
	}
	public class Indestructible_Saddle_Mount : ModMount {
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
		}
		public override void UpdateEffects(Player player) {
			player.velocity *= 0.97f;
			player.velocity += GeometryUtils.Vec2FromPolar(1, player.direction * (MathHelper.PiOver2 + player.mount._frameExtraCounter) - MathHelper.PiOver2);
			player.gravity = 0;
			const float control_limit = 0.5f;
			const float control_speed = 0.015f;
			OriginExtensions.LinearSmoothing(
				ref player.mount._frameExtraCounter,
				control_limit.Mul(player.controlDown) - control_limit.Mul(player.controlUp),
				control_speed
			);
			Rectangle hitbox = new(0, 0, 12, 12);
			Vector2 position = player.RotatedRelativePointOld(player.MountedCenter + new Vector2(0, 16));
			Vector2 move = new Vector2(16 * player.direction, 0).RotatedBy(player.fullRotation);
			position -= move * player.mount._frameExtraCounter;
			if (player.direction == -1) position.X -= 16;
			void Explode() {//TODO: replace placeholder explosion & explode on timeout
				player.mount.Dismount(player);
				Projectile.NewProjectile(
					player.GetSource_Misc("mount_explosion"),
					position + move * 2 + new Vector2(8),
					Vector2.Zero,
					ModContent.ProjectileType<Impactaxe_Explosion>(),
					50,
					10,
					player.whoAmI
				);
			}
			for (int i = 2; i > -3; i--) {
				hitbox.X = (int)(position.X + move.X * i);
				hitbox.Y = (int)(position.Y + move.Y * i);
				if (hitbox.OverlapsAnyTiles()) {
					Explode();
					return;
				}
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.CanBeChasedBy(this) && hitbox.Intersects(npc.Hitbox)) {
						Explode();
						return;
					}
				}
			}
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
		public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
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
}
