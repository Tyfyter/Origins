using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Mounts.Star_Soldier;
public class Star_Soldier_Summon_Item : ModItem, ICustomWikiStat {
	public override string Texture => "Origins/Items/Mounts/Star_Soldier/Star_Soldier_Wagon_Front";
	public override void SetDefaults() {
		Item.DefaultToMount(ModContent.MountType<Star_Soldier_Wagon>());
		Item.useStyle = ItemUseStyleID.Swing;
		Item.width = 16;
		Item.height = 30;
		Item.UseSound = SoundID.Item79;
		Item.useAnimation = 20;
		Item.useTime = 20;
		Item.noMelee = true;
		Item.rare = ItemRarityID.Blue;
		Item.value = Item.sellPrice(gold: 1);
	}
}
public class Star_Soldier_Proper : ModMount {
	public class MountHandler {
		public void Update(Player player) {
			//player.mount._flyTime = 0;
			player.mount._data.jumpSpeed = -player.velocity.Y + 0.04f; // The rate at which the player and mount ascend towards (negative y velocity) the jump height when the jump button is pressed.
		}
	}
	public override void SetStaticDefaults() {
		MountData.buff = ModContent.BuffType<Star_Soldier_Proper_Buff>();
		{// both are 0 so it can have a custom animated jump
			MountData.jumpHeight = 0;
			MountData.jumpSpeed = 0f;
		}
		MountData.acceleration = 0.19f; // The rate at which the mount speeds up.
		MountData.blockExtraJumps = true; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
		MountData.constantJump = true; // Allows you to hold the jump button down.
		MountData.fallDamage = 0.01f; // Fall damage multiplier.
		MountData.runSpeed = 11f; // The speed of the mount
		MountData.dashSpeed = 8f; // The speed the mount moves when in the state of dashing.
		MountData.flightTimeMax = 300; // The amount of time in frames a mount can be in the state of flying.

		MountData.totalFrames = 1;
		MountData.heightBoost = 118 - Player.defaultHeight;
		MountData.playerYOffsets = Enumerable.Repeat(MountData.heightBoost - 10, MountData.totalFrames).ToArray(); // Fills an array with values for less repeating code

		MountData.standingFrameCount = 1;
		MountData.standingFrameDelay = 1;
		MountData.standingFrameStart = 0;
		// Running
		MountData.runningFrameCount = 1;
		MountData.runningFrameDelay = 1;
		MountData.runningFrameStart = 0;
		// Flying
		MountData.flyingFrameCount = 1;
		MountData.flyingFrameDelay = 1;
		MountData.flyingFrameStart = 0;
		// In-air
		MountData.inAirFrameCount = 1;
		MountData.inAirFrameDelay = 1;
		MountData.inAirFrameStart = 0;
		// Idle
		MountData.idleFrameCount = 1;
		MountData.idleFrameDelay = 1;
		MountData.idleFrameStart = 0;
		MountData.idleFrameLoop = true;

		MountData.swimFrameCount = MountData.inAirFrameCount;
		MountData.swimFrameDelay = MountData.inAirFrameDelay;
		MountData.swimFrameStart = MountData.inAirFrameStart;
	}
	public override void SetMount(Player player, ref bool skipDust) {
		player.mount._mountSpecificData = new MountHandler();
	}
	public override void UpdateEffects(Player player) => GetHandler(player).Update(player);
	static MountHandler GetHandler(Player player) {
		if (player.mount._mountSpecificData is not MountHandler data) player.mount._mountSpecificData = data = new MountHandler();
		return data;
	}
	public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
		if (drawType == 0) playerDrawData.Add(new(TextureAssets.MagicPixel.Value, drawPlayer.Hitbox.Add(-Main.screenPosition), Color.Red));
		return base.Draw(playerDrawData, drawType, drawPlayer, ref texture, ref glowTexture, ref drawPosition, ref frame, ref drawColor, ref glowColor, ref rotation, ref spriteEffects, ref drawOrigin, ref drawScale, shadow);
	}
}
public class Star_Soldier_Proper_Buff : ModBuff {
	public override string Texture => "Origins/Buffs/Chambersite_Minecart_Buff";
	protected virtual int MountID => ModContent.MountType<Star_Soldier_Proper>();
	public override void SetStaticDefaults() {
		BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
			mountID = MountID
		};
	}
	public override void Update(Player player, ref int buffIndex) {
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.changeSize = true;
		originPlayer.targetWidth = 48;
		originPlayer.targetHeight = player.mount._data.heightBoost + Player.defaultHeight;
	}
}
public class Star_Soldier_Wagon : ModMount {
	public class MountHandler {
		int time;
		public void Update(Player player) {
			if (++time > 60) player.mount.SetMount(ModContent.MountType<Star_Soldier_Proper>(), player, player.direction == -1);
		}
	}
	public override void SetStaticDefaults() {
		MountData.buff = ModContent.BuffType<Star_Soldier_Wagon_Buff>();

		MountData.jumpHeight = 0;
		MountData.jumpSpeed = 0f;
		MountData.acceleration = 0.02f; // The rate at which the mount speeds up.
		MountData.blockExtraJumps = true; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
		MountData.constantJump = false; // Allows you to hold the jump button down.
		MountData.runSpeed = 1f; // The speed of the mount
		MountData.dashSpeed = 1f; // The speed the mount moves when in the state of dashing.
		MountData.flightTimeMax = 0; // The amount of time in frames a mount can be in the state of flying.

		MountData.totalFrames = 1;
		MountData.playerYOffsets = Enumerable.Repeat(8, MountData.totalFrames).ToArray(); // Fills an array with values for less repeating code

		MountData.standingFrameCount = 1;
		MountData.standingFrameDelay = 1;
		MountData.standingFrameStart = 0;
		// Running
		MountData.runningFrameCount = 1;
		MountData.runningFrameDelay = 1;
		MountData.runningFrameStart = 0;
		// Flying
		MountData.flyingFrameCount = 1;
		MountData.flyingFrameDelay = 1;
		MountData.flyingFrameStart = 0;
		// In-air
		MountData.inAirFrameCount = 1;
		MountData.inAirFrameDelay = 1;
		MountData.inAirFrameStart = 0;
		// Idle
		MountData.idleFrameCount = 1;
		MountData.idleFrameDelay = 1;
		MountData.idleFrameStart = 0;
		MountData.idleFrameLoop = true;

		MountData.swimFrameCount = MountData.inAirFrameCount;
		MountData.swimFrameDelay = MountData.inAirFrameDelay;
		MountData.swimFrameStart = MountData.inAirFrameStart;
	}
	public override void SetMount(Player player, ref bool skipDust) {
		player.mount._mountSpecificData = new MountHandler();
	}
	public override void UpdateEffects(Player player) => GetHandler(player).Update(player);
	static MountHandler GetHandler(Player player) {
		if (player.mount._mountSpecificData is not MountHandler data) player.mount._mountSpecificData = data = new MountHandler();
		return data;
	}
	public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
		if (drawType == 2) {
			drawOrigin = texture.Size() * 0.5f;
			drawOrigin.X -= drawPlayer.direction * 4;
			frame = texture.Bounds;
			drawPosition.Y += 9;
			return true;
		}
		return false;
	}
}
public class Star_Soldier_Wagon_Buff : ModBuff {
	public override string Texture => "Origins/Buffs/Chambersite_Minecart_Buff";
	protected virtual int MountID => ModContent.MountType<Star_Soldier_Wagon>();
	public override void SetStaticDefaults() {
		BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
			mountID = MountID
		};
	}
}
