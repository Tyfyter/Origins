using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Tools {
	public class Trash_Lid : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Movement",
			"Mount"
		];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToMount(ModContent.MountType<Trash_Lid_Mount>());
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 2);
			Item.useTime = Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
        }
	}
	public class Trash_Lid_Mount : ModMount {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			// Movement
			MountData.jumpHeight = 1; // How high the mount can jump.
			MountData.acceleration = 0f; // The rate at which the mount speeds up.
			MountData.jumpSpeed = 4f; // The rate at which the player and mount ascend towards (negative y velocity) the jump height when the jump button is pressed.
			MountData.blockExtraJumps = true; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
			MountData.constantJump = false; // Allows you to hold the jump button down.
			MountData.fallDamage = 1f; // Fall damage multiplier.
			MountData.runSpeed = 0f; // The speed of the mount
			MountData.dashSpeed = 0f; // The speed the mount moves when in the state of dashing.
			MountData.flightTimeMax = 0; // The amount of time in frames a mount can be in the state of flying.
			MountData.spawnDust = No_Dust.ID;
			MountData.textureWidth = 28;
			MountData.textureHeight = 48;
			MountData.heightBoost = -4;
			MountData.yOffset = -4;
			MountData.playerHeadOffset = -10;

			// Misc
			MountData.fatigueMax = 0;
			MountData.buff = ModContent.BuffType<Trash_Lid_Mount_Buff>(); // The ID number of the buff assigned to the mount.

			MountData.totalFrames = 1; // Amount of animation frames for the mount
			MountData.playerYOffsets = Enumerable.Repeat(10, MountData.totalFrames).ToArray();
			ID = Type;
		}
		public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
			mountedPlayer.legFrame.Y = 0;
			return false;
		}
		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
			
			drawOrigin.X -= 2 * drawPlayer.direction;
			return true;
		}
	}
	public class Trash_Lid_Mount_Buff : ModBuff, ICustomWikiStat {
		public string CustomStatPath => nameof(Brainy_Buff);
		public override string Texture => "Origins/Buffs/Trash_Lid_Buff";
		public override void SetStaticDefaults() {
			BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
				mountID = ModContent.MountType<Trash_Lid_Mount>()
			};
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
		}
	}
}
