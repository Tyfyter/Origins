using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Origins.Misc;
using Origins.UI;
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
	static AutoLoadingTexture shoulderTexture = typeof(Star_Soldier).GetDefaultTMLName("_Shoulder");
	static AutoLoadingTexture forearmTexture = typeof(Star_Soldier).GetDefaultTMLName("_Forearm");
	public class MountHandler {
		public int bodyFrame;
		public float bodyFrameCounter;
		public int walkFrame;
		public float walkFrameCounter;
		public Arm chosenItem = new() { item = new(ModContent.ItemType<Star_Soldier_Gun>()) };
		public Arm altItem = new() { item = new(ModContent.ItemType<Star_Soldier_Gun>()) };
		int fallCounter;
		int jumpCounter;
		public void Update(Player player) {
			if (player.whoAmI == Main.myPlayer) new Set_Relative_Target_Action(player, Main.MouseWorld - player.Bottom).Perform();

			player.direction = (player.OriginPlayer().relativeTarget.X >= 0).ToDirectionInt();
			chosenItem.UpdateRotations(player);
			altItem.UpdateRotations(player);

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
						SoundEngine.PlaySound(Origins.Sounds.SmallSawStart.WithPitch(-0.7f).WithVolume(0.5f), player.Center);
						SoundEngine.PlaySound(SoundID.Item88.WithPitch(-0.4f), player.Center);
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
							while (walkFrameCounter.CycleDown(20, speed)) {
								walkFrame.CycleDownWithZero(13);
								speed = 0;
								switch (walkFrame) {
									case 6:
									StepSound(player);
									break;
								}
							}
						} else {
							while (walkFrameCounter.CycleUp(20, speed)) {
								walkFrame.CycleUp(13);
								speed = 0;
								switch (walkFrame) {
									case 6:
									StepSound(player);
									break;
								}
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
					if (bodyFrame >= 2) {
						//SoundEngine.PlaySound(SoundID.Item89.WithPitch(1.5f), player.Center);
						SoundEngine.PlaySound(SoundID.Item100.WithPitch(-0.4f), player.Center);
					}
					bodyFrame.CycleUp(BodyTextureFrames - 1, 0);
					bodyFrame++;
				}
			} else bodyFrame = 0;

			chosenItem.ItemCheck(player, ref player.controlUseItem);
			altItem.ItemCheck(player, ref player.controlUseTile);

			static void StepSound(Player player) {
				SoundEngine.PlaySound(Origins.Sounds.TrenchmakerStep.WithPitch(3f).WithVolume(0.1f), player.Bottom);
			}
		}
		public record class Star_Soldier_Weapon_Sound(Player Player, int ItemType) : AutoSyncedAction {
			public Star_Soldier_Weapon_Sound() : this(default, default) { }
			protected override bool ShouldPerform => Player.active && !Player.dead;
			protected override void Perform() => SoundEngine.PlaySound(ContentSamples.ItemsByType[ItemType].UseSound, Player.MountedCenter);
		}
		public struct Arm {
			public Item item;
			public int itemAnimation;
			public int itemAnimationMax;
			public int itemTime;
			public int itemTimeMax;
			static Player player;
			public float shoulderRotation;
			public float forearmRotation;
			public float gunRotation;
			readonly Star_Soldier_Weapon Weapon => (Star_Soldier_Weapon)item.ModItem;
			public readonly void GetPositions(Vector2 basePosition, float baseRotation, Vector2 directions, out Vector2 shoulderPos, out Vector2 forearmPos, out Vector2 gunPos) {
				shoulderPos = basePosition - (new Vector2(4, 20) * directions).RotatedBy(baseRotation);
				if (directions.Sum() == 0) baseRotation += MathHelper.Pi;
				forearmPos = shoulderPos + (new Vector2(-22, 24) * directions).RotatedBy(baseRotation + shoulderRotation);
				gunPos = forearmPos + (new Vector2(20, 16) * directions).RotatedBy(baseRotation + forearmRotation);
			}
			public void UpdateRotations(Player player) {
				Vector2 relativeTarget = player.OriginPlayer().relativeTarget;
				GetPositions(GetBodyCenter(player, player.Center), player.fullRotation, player.Directions, out _, out _, out Vector2 gunPos);
				float targetRotation = (relativeTarget + player.Bottom - gunPos).ToRotation();

				GeometryUtils.AngularSmoothing(ref gunRotation, targetRotation, 0.2f);
				if (!float.IsFinite(gunRotation)) gunRotation = 0;

				GeometryUtils.AngularSmoothing(ref forearmRotation, targetRotation, 0.2f);
				if (!float.IsFinite(forearmRotation)) forearmRotation = 0;

				float baseRot = MathHelper.PiOver2 - player.direction * 2f;
				targetRotation = baseRot + Math.Clamp(GeometryUtils.AngleDif(baseRot, targetRotation, out int dir) * dir * player.direction, -0.9f, MathHelper.PiOver4) * player.direction;
				GeometryUtils.AngularSmoothing(ref shoulderRotation, targetRotation, 0.1f);
				if (!float.IsFinite(shoulderRotation)) shoulderRotation = 0;
			}
			public void ItemCheck(Player player, ref bool control) {
				Arm.player = player;
				Weapon.UpdateEquipped(player, this);
				if (control) {
					if (itemAnimation == 0) WithItemTimeOverride(StartUseAnimation);
					control = false;
				}
				itemAnimation.Cooldown();
				itemTime.Cooldown();
				if (itemTime <= 0 && itemAnimation > 0) {
					WithItemTimeOverride(StartUseItem);
				}
			}
			readonly void StartUseAnimation() {
				if (!Weapon.CanUseItem(player)) return;
				player.ApplyItemAnimation(item);
			}
			readonly void StartUseItem() => ShootItem(item);
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
			readonly void ShootItem(Item item) {
				if (!player.IsLocallyOwned()) {
					player.ApplyItemTime(item, callUseItem: false);
					return;
				}
				int projToShoot;
				float speed;
				int Damage;
				float Knockback;
				int usedAmmoItemId = 0;
				if (item.useAmmo > 0) {
					if (!player.PickAmmo(item, out projToShoot, out speed, out Damage, out Knockback, out usedAmmoItemId, ItemID.Sets.gunProj[item.type])) return;
				} else {
					projToShoot = item.shoot;
					speed = item.shootSpeed;
					Damage = player.GetWeaponDamage(item);
					Knockback = item.knockBack;
				}
				new Star_Soldier_Weapon_Sound(player, item.type).Perform();
				Knockback = player.GetWeaponKnockback(item, Knockback);
				EntitySource_ItemUse_WithAmmo projectileSource = new(player, item, usedAmmoItemId, nameof(Star_Soldier));
				player.ApplyItemTime(item, callUseItem: false);

				MountHandler handler = GetHandler(player);
				GetPositions(GetBodyCenter(player, player.Center), player.fullRotation, player.Directions, out _, out _, out Vector2 gunPos);

				Vector2 vector = gunRotation.ToRotationVector2();
				Vector2 velocity = vector * item.shootSpeed;
				CombinedHooks.ModifyShootStats(player, item, ref gunPos, ref velocity, ref projToShoot, ref Damage, ref Knockback);
				if (CombinedHooks.Shoot(player, item, projectileSource, gunPos, velocity, projToShoot, Damage, Knockback)) {
					Projectile.NewProjectile(projectileSource, gunPos, velocity, projToShoot, Damage, Knockback);
				}
			}
			public readonly void DrawArm(List<DrawData> playerDrawData, Color drawColor, float rotation, SpriteEffects spriteEffects, float drawScale, MountHandler handler, Vector2 bodyCenter) {
				if (item is null) return;
				if (player is null) return;
				GetPositions(bodyCenter, rotation, Vector2.One.Apply(spriteEffects), out Vector2 shoulderPos, out Vector2 forearmPos, out Vector2 gunPos);
				if (spriteEffects is SpriteEffects.FlipHorizontally or SpriteEffects.FlipVertically) rotation += MathHelper.Pi;
				playerDrawData.Add(new(
					shoulderTexture,
					shoulderPos,
					null,
					drawColor,
					rotation + shoulderRotation,
					spriteEffects.ApplyToOrigin(new(29, 5), shoulderTexture.Value.Bounds),
					drawScale,
					spriteEffects
				) {
					shader = player.cMount
				});

				playerDrawData.Add(new(
					forearmTexture,
					forearmPos,
					null,
					drawColor,
					rotation + forearmRotation,
					spriteEffects.ApplyToOrigin(new(9, 3), forearmTexture.Value.Bounds),
					drawScale,
					spriteEffects
				) {
					shader = player.cMount
				});

				Rectangle frame = TextureAssets.Item[item.type].Value.Bounds;
				DrawData data = new(
					TextureAssets.Item[item.type].Value,
					gunPos,
					frame,
					drawColor,
					rotation + gunRotation,
					spriteEffects.ApplyToOrigin(new(25, 9), frame),
					drawScale,
					spriteEffects
				) {
					shader = player.cMount
				};
				if (item.ModItem is Star_Soldier_Weapon weapon) weapon.ModifyDrawData(handler, ref data);
				playerDrawData.Add(data);
				if (item.glowMask >= 0) {
					data.texture = TextureAssets.GlowMask[item.glowMask].Value;
					data.color = Color.White;
					playerDrawData.Add(data);
				}
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
		OriginsSets.Mounts.DisableDirectionChange[Type] = true;
	}
	public override void SetMount(Player player, ref bool skipDust) {
		player.mount._mountSpecificData = new MountHandler();
	}
	struct HideItemHUD : IBroken {
		static string IBroken.BrokenReason => "Hide item HUD";
	}
	public override void UpdateEffects(Player player) {
		//SwitchableUIState.SharedInterfaces.ItemUseHUD.Hidden = true;
		GetHandler(player).Update(player);
		player.OriginPlayer().mountOnly = true;
	}

	public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) => false;
	static MountHandler GetHandler(Player player) {
		if (player.mount._mountSpecificData is not MountHandler data) player.mount._mountSpecificData = data = new MountHandler();
		return data;
	}
	static Vector2 GetBodyCenter(Player player, Vector2 hitboxCenter) => hitboxCenter - Vector2.UnitY * ((player.height - Player.defaultHeight) * 0.5f - 8);
	public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle _, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
		if (drawType == 3) {
			MountHandler handler = GetHandler(drawPlayer);
			Rectangle frame = backLegTexture.Frame(verticalFrames: LegTextureFrames, frameY: handler.walkFrame);
			Vector2 bodyCenter = GetBodyCenter(drawPlayer, drawPosition);
			Matrix rotationMatrix = Matrix.CreateRotationZ(rotation);
			Vector2 hips = bodyCenter + new Vector2(drawPlayer.direction * -16, 32).Transform(rotationMatrix);

			handler.altItem.DrawArm(playerDrawData, drawColor.MultiplyRGBA(Color.Gray), rotation, spriteEffects, drawScale, handler, bodyCenter);
			playerDrawData.Add(new(
				backLegTexture,
				hips,
				frame,
				drawColor,
				rotation,
				spriteEffects.ApplyToOrigin(new(15, 23), frame),
				drawScale,
				spriteEffects
			) {
				shader = drawPlayer.cMount
			});

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
			) {
				shader = drawPlayer.cMount
			});

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
			) {
				shader = drawPlayer.cMount
			});
			handler.chosenItem.DrawArm(playerDrawData, drawColor, rotation, spriteEffects, drawScale, handler, bodyCenter);
		}
		return false;
	}
	public class Star_Soldier_UI : SwitchableUIState {
		public override void AddToList() => OriginSystem.Instance.MountHUD.AddState(this);
		public override bool IsActive() => Main.LocalPlayer.mount.Active && Main.LocalPlayer.mount.Type == ModContent.MountType<Star_Soldier>();
		public Star_Soldier_UI() : base() {
			OverrideSamplerState = SamplerState.PointClamp;
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Player player = Main.LocalPlayer;
			Vector2 pos = player.MountedCenter - Main.screenPosition;
			pos.Y += player.height * 0.5f + 8;

			Main.UIScaleMatrix.Decompose(out Vector3 scale, out _, out _);
			pos.X = ((int)pos.X) / scale.X;
			pos.Y = ((int)pos.Y) / scale.Y;

			MountHandler handler = GetHandler(player);
			(handler.chosenItem.item?.ModItem as Star_Soldier_Weapon)?.DrawHud(spriteBatch, ref pos, scale.XY());
			(handler.altItem.item?.ModItem as Star_Soldier_Weapon)?.DrawHud(spriteBatch, ref pos, scale.XY());
		}
	}
}
public abstract class Star_Soldier_Weapon : ModItem, IExpectToBeUnobtainable {
	Asset<Texture2D> icon;
	public override ModItem NewInstance(Item entity) {
		Star_Soldier_Weapon item = (Star_Soldier_Weapon)base.NewInstance(entity);
		item.icon = icon;
		return item;
	}
	public override void AutoStaticDefaults() {
		base.AutoStaticDefaults();
		icon = ModContent.Request<Texture2D>(Texture + "_Icon");
	}
	public virtual void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) { }
	public virtual void UpdateEquipped(Player player, Star_Soldier.MountHandler.Arm arm) { }
	public override bool NeedsAmmo(Player player) => false;
	public abstract void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale);
}
public class Star_Soldier_Gun : Star_Soldier_Weapon {
	static int AmmoMax => 15;
	static int ReloadLength => 51;
	int ammo = 0;
	int reloadTime = 0;
	bool usedFakeAmmo;
	public override void SetDefaults() {
		Item.CloneDefaults(ItemID.SDMG);
		Item.damage = 94;
		Item.useAnimation = 24;
		Item.useTime = 8;
		Item.shootSpeed = 19;
		Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(2f).WithVolume(0.6f);
	}
	public override bool CanUseItem(Player player) => ammo > 0;
	public override void UpdateEquipped(Player player, Star_Soldier.MountHandler.Arm arm) {
		if (arm.itemAnimation != 0) {
			reloadTime = 0;
			if (arm.itemTime == arm.itemTimeMax && usedFakeAmmo) ammo--;
		} else if (ammo < AmmoMax && reloadTime.Warmup(ReloadLength)) {
			ammo = AmmoMax;
		}
	}
	public override bool? CanChooseAmmo(Item ammo, Player player) {
		usedFakeAmmo = false;
		return base.CanChooseAmmo(ammo, player);
	}
	public override bool NeedsAmmo(Player player) {
		if (needsAmmoChecking) return true;
		using (needsAmmoChecking.ScopedOverride(true)) {
			usedFakeAmmo = ItemLoader.NeedsAmmo(Item, player);
		}
		return false;
	}
	static bool needsAmmoChecking = false;
	public override void OnConsumeAmmo(Item ammo, Player player) => this.ammo--;
	public override void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) {
		drawData.sourceRect = drawData.texture.Frame(1, 4, 0, 0);
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale) {
		int width = 64;
		int halfWidth = width / 2;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position,
			new Rectangle(0, 0, width, 4),
			Color.Black,
			0,
			new Vector2(halfWidth, 2),
			1,
			SpriteEffects.None,
		0);
		if (reloadTime > 1 && reloadTime < ReloadLength) {
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, ((reloadTime - 1) * width) / (ReloadLength - 1), 4),
				Color.White,
				0,
				new Vector2(halfWidth, 2),
				1,
				SpriteEffects.None,
			0);
		}
		float mult = width / (float)AmmoMax;
		for (int i = 0; i < ammo; i++) {
			Vector2 pos = position + new Vector2(halfWidth - i * mult, 0);
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				pos,
				new Rectangle(0, 0, 2, 4),
				Color.OrangeRed,
				0,
				new Vector2(1, 2),
				1,
				SpriteEffects.None,
			0);
		}
		position.Y += 8;
	}
}
public class Star_Soldier_Pod : Star_Soldier_Weapon {
	public override void SetStaticDefaults() {
		AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Type] = ItemID.RocketLauncher;
	}
	public override void SetDefaults() {
		Item.CloneDefaults(ItemID.RocketLauncher);
		Item.useAnimation /= 3;
		Item.useTime /= 3;
	}
	public override void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) {
		drawData.sourceRect = drawData.texture.Frame(1, 2, 0, 0);
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale) { }
}
public class Star_Soldier_Proper_Buff : ModBuff {
	public override string Texture => "Origins/Buffs/Star_Soldier_Buff";
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
		MountData.acceleration = 0.001f; // The rate at which the mount speeds up.
		MountData.blockExtraJumps = true; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
		MountData.constantJump = false; // Allows you to hold the jump button down.
		MountData.runSpeed = 32f; // The speed of the mount
		MountData.dashSpeed = 32f; // The speed the mount moves when in the state of dashing.
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
	public override string Texture => "Origins/Buffs/Star_Soldier_Buff";
	protected virtual int MountID => ModContent.MountType<Star_Soldier_Wagon>();
	public override void SetStaticDefaults() {
		BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
			mountID = MountID
		};
	}
}