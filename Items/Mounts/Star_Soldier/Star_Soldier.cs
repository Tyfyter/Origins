using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using PegasusLib.Networking;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
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
public class Star_Soldier : ModMount {
	static int BodyTextureFrames => 5;
	AutoLoadingTexture bodyTexture = typeof(Star_Soldier).GetDefaultTMLName();
	static int LegTextureFrames => 18;
	AutoLoadingTexture frontLegTexture = typeof(Star_Soldier).GetDefaultTMLName("_Front_Leg");
	AutoLoadingTexture backLegTexture = typeof(Star_Soldier).GetDefaultTMLName("_Back_Leg");
	public class MountHandler {
		public int bodyFrame;
		public float bodyFrameCounter;
		public int walkFrame;
		public float walkFrameCounter;
		public Item chosenItem = new(ModContent.ItemType<Star_Soldier_Gun>());
		public int itemAnimation;
		public int itemAnimationMax;
		public int itemTime;
		public int itemTimeMax;
		public float itemRotation;
		int fallCounter;
		int jumpCounter;
		Player player;
		public void Update(Player player) {
			this.player = player;

			if (player.whoAmI == Main.myPlayer) new Set_Relative_Target_Action(player, Main.MouseWorld - player.Bottom).Perform();
			Vector2 playerCenter = GetAimOrigin();
			GeometryUtils.AngularSmoothing(ref itemRotation, ((player.OriginPlayer().relativeTarget + player.Bottom) - playerCenter).ToRotation(), 0.2f);
			if (!float.IsFinite(itemRotation)) itemRotation = 0;
			player.direction = (GeometryUtils.AngleDif(itemRotation, 0, out _) < MathHelper.PiOver2).ToDirectionInt();

			//player.mount._flyTime = 0;
			bool collidingY = player.OriginPlayer().collidingY;

			if (!collidingY) player.mount._data.jumpSpeed = Math.Max(-player.velocity.Y, 0) + 0.04f;
			else player.mount._data.jumpSpeed = -1;

			if (!collidingY) {
				if (++fallCounter < 5) collidingY = true;
			} else fallCounter = 0;
			if (collidingY) {
				if (player.controlJump || jumpCounter > 0) {
					jumpCounter++;
					walkFrame = 17 - (jumpCounter / 4);
					if (walkFrame <= 13) {
						walkFrame = 13;
						jumpCounter = 0;
						if (player.controlJump) player.mount._data.jumpSpeed = 10;
						else player.velocity.Y -= 12;

					}
				} else {
					float speed = Math.Abs(player.velocity.X);
					if (speed < 0.5f) {
						walkFrame = 0;
						walkFrameCounter = 0;
					} else {
						if (player.velocity.X * player.direction < 0) {
							while (walkFrameCounter.CycleDown(32, speed)) {
								walkFrame.CycleDownWithZero(13);
								speed = 0;
							}
						} else {
							while (walkFrameCounter.CycleUp(32, speed)) {
								walkFrame.CycleUp(13);
								speed = 0;
							}
						}
					}
				}
			} else {
				walkFrame = 13;
				walkFrameCounter = 0;
				jumpCounter = 0;
			}

			if (player.controlJump && jumpCounter == 0) {
				if (bodyFrameCounter.CycleUp(4)) {
					bodyFrame.CycleUp(BodyTextureFrames - 1, 0);
					bodyFrame++;
				}
			} else bodyFrame = 0;
			if (player.controlUseItem) {
				if (itemAnimation == 0) WithItemTimeOverride(StartUseAnimation);
				player.controlUseItem = false;
			}
			itemAnimation.Cooldown();
			itemTime.Cooldown();
			if (itemTime <= 0 && itemAnimation > 0) {
				WithItemTimeOverride(StartUseItem);
			}
		}
		void StartUseAnimation() {
			player.ApplyItemAnimation(chosenItem);
		}
		void StartUseItem() => ShootItem(chosenItem);
		void ShootItem(Item sItem) {
			if (!player.IsLocallyOwned()) {
				player.ApplyItemTime(chosenItem, callUseItem: false);
				return;
			}
			int projToShoot;
			float speed;
			int Damage;
			float Knockback;
			int usedAmmoItemId = 0;
			if (sItem.useAmmo > 0) {
				if (!player.PickAmmo(sItem, out projToShoot, out speed, out Damage, out Knockback, out usedAmmoItemId, ItemID.Sets.gunProj[sItem.type])) return;
			} else {
				projToShoot = sItem.shoot;
				speed = sItem.shootSpeed;
				Damage = player.GetWeaponDamage(sItem);
				Knockback = sItem.knockBack;
			}
			new Star_Soldier_Weapon_Sound(player).Perform();
			Knockback = player.GetWeaponKnockback(sItem, Knockback);
			EntitySource_ItemUse_WithAmmo projectileSource = new(player, sItem, usedAmmoItemId, nameof(Star_Soldier));
			player.ApplyItemTime(chosenItem, callUseItem: false);

			Vector2 playerCenter = GetAimOrigin();

			Vector2 vector = itemRotation.ToRotationVector2();
			Vector2 velocity = vector * sItem.shootSpeed;
			CombinedHooks.ModifyShootStats(player, sItem, ref playerCenter, ref velocity, ref projToShoot, ref Damage, ref Knockback);
			if (CombinedHooks.Shoot(player, sItem, projectileSource, playerCenter, velocity, projToShoot, Damage, Knockback)) {
				Projectile.NewProjectile(projectileSource, playerCenter, velocity, projToShoot, Damage, Knockback);
			}
		}
		Vector2 GetAimOrigin() => player.RotatedRelativePoint(player.MountedCenter);
		void WithItemTimeOverride(Action action) {
			using ScopedOverride<int> o0 = player.itemAnimation.ScopedOverride(itemAnimation);
			using ScopedOverride<int> o1 = player.itemAnimationMax.ScopedOverride(itemAnimationMax);
			using ScopedOverride<int> o2 = player.itemTime.ScopedOverride(itemTime);
			using ScopedOverride<int> o3 = player.itemTimeMax.ScopedOverride(itemTimeMax);
			action();
			itemAnimation = player.itemAnimation;
			itemAnimationMax = player.itemAnimationMax;
			itemTime = player.itemTime;
			itemTimeMax = player.itemTimeMax;
		}
		public record class Star_Soldier_Weapon_Sound(Player Player) : AutoSyncedAction {
			public Star_Soldier_Weapon_Sound() : this(default(Player)) { }
			protected override bool ShouldPerform => Player.active && !Player.dead;
			protected override void Perform() {
				if (!Main.dedServ) SoundEngine.PlaySound(GetHandler(Player).chosenItem.UseSound, Player.MountedCenter);
			}
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
		MountData.heightBoost = 116 - Player.defaultHeight;
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
	public override void UpdateEffects(Player player) {
		GetHandler(player).Update(player);
		player.OriginPlayer().mountOnly = true;
	}

	public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) => false;
	static MountHandler GetHandler(Player player) {
		if (player.mount._mountSpecificData is not MountHandler data) player.mount._mountSpecificData = data = new MountHandler();
		return data;
	}
	public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle _, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
		if (drawType == 3) {
			MountHandler handler = GetHandler(drawPlayer);
			Rectangle frame = backLegTexture.Frame(verticalFrames: LegTextureFrames, frameY: handler.walkFrame);
			Vector2 bodyCenter = drawPosition - Vector2.UnitY * ((drawPlayer.height - Player.defaultHeight) * 0.5f - 8);
			Vector2 hips = bodyCenter + new Vector2(drawPlayer.direction * -16, 32);

			playerDrawData.Add(new(
				backLegTexture,
				hips,
				frame,
				drawColor,
				rotation,
				spriteEffects.ApplyToOrigin(new(15, 23), frame),
				drawScale,
				spriteEffects
			));

			frame = bodyTexture.Frame(verticalFrames: BodyTextureFrames, frameY: handler.bodyFrame);
			playerDrawData.Add(new(
				bodyTexture,
				bodyCenter,
				frame,
				drawColor,
				rotation,
				spriteEffects.ApplyToOrigin(new(59, 37), frame),
				drawScale,
				spriteEffects
			));

			frame = frontLegTexture.Frame(verticalFrames: LegTextureFrames, frameY: handler.walkFrame);
			playerDrawData.Add(new(
				frontLegTexture,
				hips,
				frame,
				drawColor,
				rotation,
				spriteEffects.ApplyToOrigin(new(15, 23), frame),
				drawScale,
				spriteEffects
			));
			if (handler.chosenItem is Item item) {
				frame = TextureAssets.Item[item.type].Value.Bounds;
				DrawData data = new(
					TextureAssets.Item[item.type].Value,
					bodyCenter,
					frame,
					drawColor,
					handler.itemRotation + rotation + (spriteEffects == SpriteEffects.FlipHorizontally ? MathHelper.Pi : 0),
					spriteEffects.ApplyToOrigin(new(3, 5), frame),
					drawScale,
					spriteEffects
				);
				if (item.ModItem is Star_Soldier_Weapon weapon) weapon.ModifyDrawData(handler, ref data);
				playerDrawData.Add(data);
				if (item.glowMask >= 0) {
					data.texture = TextureAssets.GlowMask[item.glowMask].Value;
					data.color = Color.White;
					playerDrawData.Add(data);
				}
			}
		}
		return false;
	}
}
public abstract class Star_Soldier_Weapon : ModItem, IExpectToBeUnobtainable {
	Asset<Texture2D> icon;
	public override ModItem NewInstance(Item entity) {
		Star_Soldier_Weapon item = (Star_Soldier_Weapon)base.NewInstance(entity);

		return item;
	}
	public override void AutoStaticDefaults() {
		base.AutoStaticDefaults();
		icon = ModContent.Request<Texture2D>(Texture + "_Icon");
	}
	public virtual void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) { }
}
public class Star_Soldier_Gun : Star_Soldier_Weapon {
	public override void SetDefaults() {
		Item.CloneDefaults(ItemID.SniperRifle);
		Item.useAnimation /= 3;
		Item.useTime /= 3;
	}
	public override void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) {
		drawData.sourceRect = drawData.texture.Frame(1, 4, 0, 0);
	}
}
public class Star_Soldier_Proper_Buff : ModBuff {
	public override string Texture => "Origins/Buffs/Chambersite_Minecart_Buff";
	protected virtual int MountID => ModContent.MountType<Star_Soldier>();
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
			if (++time > 60) player.mount.SetMount(ModContent.MountType<Star_Soldier>(), player, player.direction == -1);
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