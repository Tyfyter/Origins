using Avalon;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Weapons.Magic;
using Origins.Misc;
using Origins.Projectiles;
using Origins.UI;
using PegasusLib.Networking;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

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
public class Star_Soldier : ModMount, IModifyTriggers {
	static int BodyTextureFrames => 5;
	AutoLoadingTexture bodyTexture = typeof(Star_Soldier).GetDefaultTMLName();
	AutoLoadingTexture bodyGlowTexture = typeof(Star_Soldier).GetDefaultTMLName("_Glow");
	static int LegTextureFrames => 18;
	AutoLoadingTexture frontLegTexture = typeof(Star_Soldier).GetDefaultTMLName("_Front_Leg");
	AutoLoadingTexture backLegTexture = typeof(Star_Soldier).GetDefaultTMLName("_Back_Leg");
	static AutoLoadingTexture shoulderTexture = typeof(Star_Soldier).GetDefaultTMLName("_Shoulder");
	static AutoLoadingTexture forearmTexture = typeof(Star_Soldier).GetDefaultTMLName("_Forearm");
	public static DynamicSpriteFont Font { get; private set; }
	public static DynamicSpriteFont FontLarge { get; private set; }
	public static HashSet<int> RainbowDyes;
	/*public static ArmorShaderSet<bool> HasDye { get; } = new() { // use me when "ForceInitialize" from PegasusLib is usable by other mods
		[(Main.pixelShader, "ArmorColoredRainbow")] = true,
		[(Main.pixelShader, "ArmorBrightnessRainbow")] = true,
		[(Main.pixelShader, "ArmorLivingRainbow")] = true,
		[(Main.pixelShader, "ArmorMidnightRainbow")] = true
	};*/
	public class MountHandler {
		public static int MaxLife => 750;
		public int life = MaxLife;
		public int dotCount = 0;
		public int bodyFrame;
		public float bodyFrameCounter;
		public int walkFrame;
		public float walkFrameCounter;
		public Arm chosenItem = new() { item = new(ModContent.ItemType<Star_Soldier_Laser>()) };
		public Arm altItem = new() { item = new(ModContent.ItemType<Star_Soldier_Gun>()) };
		readonly WeakReference<Entity> lockOnTarget = new(null);
		public Entity LockOnTarget {
			get => lockOnTarget.TryGetTarget(out Entity target) ? target : null;
			set => lockOnTarget.SetTarget(value);
		}
		static int currentArm;
		ref Arm GetArm(int index) {
			currentArm = index;
			if (index == 1) return ref altItem;
			return ref chosenItem;
		}
		int fallCounter;
		int jumpCounter;
		public void Update(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			if (LockOnTarget is NPC target) {
				if (!target.active) LockOnTarget = null;
				else if (player.MountedCenter.Clamp(target.Hitbox).WithinRange(player.MountedCenter, 16 * 100) == false) LockOnTarget = null;
			}
			Vector2 targetPos = LockOnTarget?.Center ?? Main.MouseWorld;
			Vector2 shoulderPos = player.MountedCenter - player.Directions(4, 20);
			Vector2 targetDir = (targetPos - shoulderPos).Normalized(out float targetDist);
			if (targetDist < 45) targetPos = shoulderPos + targetDir * 45;
			if (player.whoAmI == Main.myPlayer) new Set_Relative_Target_Action(player, targetPos - player.Bottom).Perform();

			player.direction = (originPlayer.relativeTarget.X >= 0).ToDirectionInt();
			GetArm(0).UpdateRotations(player);
			GetArm(1).UpdateRotations(player);

			if (player.bleed) originPlayer.oiled = true;

			player.buffImmune[BuffID.Poisoned] = true;
			if (!originPlayer.oiled) {
				player.buffImmune[BuffID.OnFire] = true;
				player.buffImmune[BuffID.OnFire3] = true;
			}

			player.lifeRegenTime = 0;
			if (player.lifeRegenCount < 0) {
				dotCount -= player.lifeRegenCount;
				player.lifeRegenCount = 0;
			}
			while (dotCount.CycleUp(120, 0)) life--;

			if (life <= 0) {
				player.mount.Dismount(player);
				player.statLife -= player.statLifeMax2 / 4 - 1;
				player.lifeRegenCount = -120;
				return;
			}

			//player.mount._flyTime = 0;
			bool collidingY = originPlayer.collidingY;


			if (!collidingY) {
				player.mount._data.jumpSpeed = Math.Max(-player.velocity.Y, 0) + 1f;
				if (player.controlJump && player.jump == 0 && player.mount._flyTime > 0 && player.velocity.Y > -8) {
					player.velocity.Y -= 0.05f;
				}
			} else {
				player.mount._data.jumpSpeed = -1;
			}

			if (!collidingY) {
				if (++fallCounter < 5) collidingY = true;
			} else fallCounter = 0;
			if (collidingY) {
				if (player.controlJump || jumpCounter > 0) {
					jumpCounter++;
					walkFrame = 14 + (jumpCounter / 4);
					if (walkFrame > 15) {
						SoundEngine.PlaySound(Origins.Sounds.SmallSawStart.WithPitch(-0.7f).WithVolume(0.5f), player.Center);
						SoundEngine.PlaySound(SoundID.Item88.WithPitch(-0.4f), player.Center);
						SoundEngine.PlaySound(Origins.Sounds.TrenchmakerStep.WithPitchRange(0.5f, 0.7f).WithVolume(0.9f), player.Bottom);
						walkFrame = 15;
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
									case 12:
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
									case 12:
									StepSound(player);
									break;
								}
							}
						}
					}
				}
			} else {
				walkFrame = 15;
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

			static void StepSound(Player player) {
				SoundEngine.PlaySound(Origins.Sounds.TrenchmakerStep.WithPitchRange(0.5f, 0.7f).WithVolume(0.3f), player.Bottom);
				SoundEngine.PlaySound(SoundID.Item88.WithPitchRange(1.6f, 1.9f).WithVolume(0.1f), player.Bottom);
			}
		}
		public void HandleHurt(Player player, in Player.HurtInfo info) {
			life -= info.Damage;
			if (life > 0) return;
			player.mount.Dismount(player);
			player.Hurt(info with { Damage = player.statLifeMax2 / 4 });
		}
		public void ItemCheck(Player player) {
			using ScopedOverride<bool> _ = player.controlUseTile.ScopedOverride(player.controlUseTile && !player.tileInteractionHappened);
			GetArm(0).Weapon.PreItemCheck(player, this, ref GetArm(0));
			GetArm(1).Weapon.PreItemCheck(player, this, ref GetArm(1));
			GetArm(0).ItemCheck(player, ref player.controlUseItem);
			GetArm(1).ItemCheck(player, ref player.controlUseTile);
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
			public readonly Star_Soldier_Weapon Weapon => (Star_Soldier_Weapon)item.ModItem;
			public readonly void GetPositions(Vector2 basePosition, float baseRotation, Vector2 directions, out Vector2 shoulderPos, out Vector2 forearmPos, out Vector2 gunPos) {
				shoulderPos = basePosition - (new Vector2(4, 20) * directions).RotatedBy(baseRotation);
				if (directions.Sum() == 0) baseRotation += MathHelper.Pi;
				forearmPos = shoulderPos + (new Vector2(-22, 24) * directions).RotatedBy(baseRotation + shoulderRotation);
				gunPos = forearmPos + (new Vector2(20, 16) * directions).RotatedBy(baseRotation + forearmRotation);
			}
			public void UpdateRotations(Player player) {
				if (!Weapon.UpdateRotations(player, ref this)) return;
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
				Weapon.UpdateEquipped(player, ref this, control);
				if (control) {
					if (itemAnimation == 0) WithItemTimeOverride(StartUseAnimation);
				}
				itemTime.Cooldown();
				if (itemTime <= 0 && itemAnimation > 0) {
					WithItemTimeOverride(StartUseItem);
				}
				itemAnimation.Cooldown();
			}
			readonly void StartUseAnimation() {
				if (!Weapon.CanUseItem(player)) return;
				player.ApplyItemAnimation(item);
			}
			readonly void StartUseItem() => ShootItem(item);
			void WithItemTimeOverride(Action action) {
				using ScopedRedirect<int> o0 = new(ref player.itemAnimation, ref itemAnimation);
				using ScopedRedirect<int> o1 = new(ref player.itemAnimationMax, ref itemAnimationMax);
				using ScopedRedirect<int> o2 = new(ref player.itemTime, ref itemTime);
				using ScopedRedirect<int> o3 = new(ref player.itemTimeMax, ref itemTimeMax);
				action();
			}
			readonly void ShootItem(Item item) {
				if (!player.IsLocallyOwned() || GetHandler(player) is not MountHandler handler) {
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
				EntitySource_ItemUse_WithAmmo projectileSource = new(player, item, usedAmmoItemId, nameof(Star_Soldier) + currentArm);
				player.ApplyItemTime(item, callUseItem: false);

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
		public record class Star_Soldier_Weapon_Sound(Player Player, int ItemType) : AutoSyncedAction {
			public Star_Soldier_Weapon_Sound() : this(default, default) { }
			protected override bool ShouldPerform => Player.active && !Player.dead;
			protected override void Perform() {
				Item item = ContentSamples.ItemsByType[ItemType];
				SoundEngine.PlaySound(item.UseSound, Player.MountedCenter);
				if (item.ModItem is Star_Soldier_Weapon weapon) weapon.PlaySound(Player);
			}
		}
	}
	#region mount handling
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
		OriginsSets.Mounts.EyePosition[Type] = player => player.MountedCenter + player.Directions(new Vector2(24, -16));
		OriginsSets.Mounts.HideTails[Type] = true;

		RainbowDyes = [
			GameShaders.Armor.GetShaderIdFromItemId(ItemID.RainbowDye),
			GameShaders.Armor.GetShaderIdFromItemId(ItemID.IntenseRainbowDye),
			GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye),
			GameShaders.Armor.GetShaderIdFromItemId(ItemID.MidnightRainbowDye)
		];
		On_Player.ScrollHotbar += On_Player_ScrollHotbar;
		if (!Main.dedServ) {
			Asset<Texture2D> fontTexture = ModContent.Request<Texture2D>("Origins/UI/Ashen_Font");
			Task.Run(FontAssets.ItemStack.Wait).ContinueWith(__ => {
				fontTexture.Wait();
				Rectangle glyph = new(0, 0, 10, 14);
				Rectangle padding = new(0, 0, 0, 14);
				Vector3 kerning = new(1, 10, 0);
				FontGenerator.CharRange range = new(fontTexture.Value, 'A'..'Z', glyph, padding, kerning) {
					GlyphXChange = 11
				};
				FontGenerator.CharRange Range(Range chars, int? y = null, int? x = null) {
					return range with { Range = chars, StartGlyph = glyph with { X = x ?? glyph.X, Y = y ?? glyph.Y } };
				}
				FontGenerator.CharRange Single(char @char, int y, int x) {
					return range with { Range = @char..@char, StartGlyph = glyph with { X = x, Y = y } };
				}
				Font = FontGenerator.Monospace(FontAssets.ItemStack.Value, 0, 15, '*',
					range,
					Range('a'..'z'),
					Range(' '..'9', 15),
					Range(':'..'@', 30),
					Range('['..'`', 30, 77),
					Range('{'..'~', 30, 143),
					Single('©', 30, 187),
					Range('‘'..'‟', 30, 198)
				);
			});
			Asset<Texture2D> fontTextureLarge = ModContent.Request<Texture2D>("Origins/UI/Ashen_Font_Large");
			Task.Run(FontAssets.ItemStack.Wait).ContinueWith(__ => {
				fontTextureLarge.Wait();
				Rectangle glyph = new(0, 0, 20, 28);
				Rectangle padding = new(0, 0, 0, 28);
				Vector3 kerning = new(2, 20, 0);
				FontGenerator.CharRange range = new(fontTextureLarge.Value, 'A'..'Z', glyph, padding, kerning) {
					GlyphXChange = 40
				};
				FontGenerator.CharRange Range(Range chars, int? y = null, int? x = null) {
					return range with { Range = chars, StartGlyph = glyph with { X = x ?? glyph.X, Y = y ?? glyph.Y } };
				}
				FontGenerator.CharRange Single(char @char, int y, int x) {
					return range with { Range = @char..@char, StartGlyph = glyph with { X = x, Y = y } };
				}
				FontLarge = FontGenerator.Monospace(FontAssets.ItemStack.Value, 0, 30, '*',
					range,
					Range('a'..'z'),
					Range(' '..'9', 29),
					Range(':'..'@', 58),
					Range('['..'`', 58, 280),
					Range('{'..'~', 58, 520),
					Single('©', 58, 680)
				);
			});
		}
	}
	static void On_Player_ScrollHotbar(On_Player.orig_ScrollHotbar orig, Player self, int Offset) {
		if (!self.mount.IsMount<Star_Soldier>()) orig(self, Offset);
	}
	public override void SetMount(Player player, ref bool skipDust) => player.mount._mountSpecificData = new MountHandler();
	struct HideItemHUD : IBroken {
		static string IBroken.BrokenReason => "Hide item HUD";
	}
	public override void UpdateEffects(Player player) {
		//SwitchableUIState.SharedInterfaces.ItemUseHUD.Hidden = true;
		player.statDefense += 65 - player.armor[0].defense - player.armor[1].defense - player.armor[2].defense;
		player.OriginPlayer().knockbackTaken.Base -= 4.5f;
		player.AddMaxBreath(200);
		GetHandler(player)?.Update(player);
		player.OriginPlayer().mountOnly = true;
		Max(ref player.accWatch, 5);
		Max(ref player.accCompass, 2);
		Max(ref player.accDepthMeter, 2);
	}
	public static void ItemCheck(Player player) => GetHandler(player).ItemCheck(player);
	public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) => false;
	public static bool HandleHurt(Player player, in Player.HurtInfo info) {
		if (GetHandler(player) is not MountHandler handler) return false;
		handler.HandleHurt(player, in info);
		return true;
	}
	public bool ModifyTriggers(Player player, TriggersSet triggersSet) {
		for (int i = 1; i <= 10; i++) triggersSet.KeyStatus["Hotbar" + i] = false;
		triggersSet.HotbarPlus = false;
		triggersSet.HotbarMinus = false;
		return false;
	}
	public static MountHandler GetHandler(Player player) {
		if (!player.mount.IsMount<Star_Soldier>()) return null;
		if (player.mount._mountSpecificData is not MountHandler data) player.mount._mountSpecificData = data = new MountHandler();
		return data;
	}
	static Vector2 GetBodyCenter(Player player, Vector2 hitboxCenter) => hitboxCenter - Vector2.UnitY * ((player.height - Player.defaultHeight) * 0.5f - 8);
	public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle _, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
		if (drawType == 3 && GetHandler(drawPlayer) is MountHandler handler) {
			Rectangle frame = backLegTexture.Frame(verticalFrames: LegTextureFrames, frameY: handler.walkFrame);
			Vector2 bodyCenter = GetBodyCenter(drawPlayer, drawPosition);
			Matrix rotationMatrix = Matrix.CreateRotationZ(rotation);
			Vector2 hips = bodyCenter + new Vector2(drawPlayer.direction * -16, 32).Transform(rotationMatrix);

			(drawPlayer.direction == -1 ? handler.altItem : handler.chosenItem).DrawArm(playerDrawData, drawColor.MultiplyRGBA(Color.Gray), rotation, spriteEffects, drawScale, handler, bodyCenter);
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
			DrawData bodyData = new(
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
			};
			playerDrawData.Add(bodyData);
			bodyData.texture = bodyGlowTexture;
			bodyData.color = Color.White;
			playerDrawData.Add(bodyData);

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
			(drawPlayer.direction == -1 ? handler.chosenItem : handler.altItem).DrawArm(playerDrawData, drawColor, rotation, spriteEffects, drawScale, handler, bodyCenter);
		}
		return false;
	}
	#endregion
	public record class Star_Soldier_Set_Weapons(Player Player, int MainHand, int OffHand) : AutoSyncedAction {
		public Star_Soldier_Set_Weapons() : this(default, default, default) { }
		protected override bool ShouldPerform => Player.active && !Player.dead;
		protected override void Perform() {
			if (GetHandler(Player) is not MountHandler handler) return;
			handler.chosenItem.item.SetDefaults(MainHand);
			handler.altItem.item.SetDefaults(OffHand);
		}
	}
	#region colors
	public static readonly WeaponColors DefaultColors = new(1, WeaponColors.Create(255, 100, 0));
	public static readonly WeaponColors Chrysalis = new(
		Matrix.Multiply(Matrix.Identity, 5) with { M41 = 1, M44 = 0 },
		WeaponColors.Create(0.0215f, 0.1645f, 0.1785f, 10),
		WeaponColors.Create(0.063f, 0.863f, 0.051f),
		new(1f / 3, 0.075f, 0.25f)
	) {
		BeamScale = 0.9f,
		AOEScale = 0.75f
	};
	public static readonly WeaponColors RGB = new(
		Matrix.Multiply(new(
			1, 0, 0, 0,
			0.5f, 0, 0, 0,
			0.25f, 0, 0, 0,
			0, 0, 0, 0
		), 15),
		WeaponColors.Create(1f, 0, 0),
		new(
			0, -2f, 0, 0,
			0, 2, -8f, 0,
			0, 0, 4, 0,
			0, 0, 0, 0
		),
		default
	) {
		BeamScale = 1.3f
	};
	public static readonly WeaponColors Plasma = WeaponColors.CreateGradient(
		new Color(187, 10, 251),
		new(new Color(82, 103, 255), -0.35f, 5),
		new(new Color(212, 0, 95), -0.5f, 2)
	) with {
		BeamScale = 1.7f
	};
	public static void InitializeColor(Player player, ref WeaponColors? colors) {
		if (colors?.BeamScale == 0) {
			if (RainbowDyes.Contains(player.cMount)) colors = RGB;
			else if (!NameColors.TryGetValue(player.name, out WeaponColors color)) colors = null;
			else colors = color;
		}
	}
	public static IReadOnlyDictionary<string, WeaponColors> NameColors { get; } = new Dictionary<string, WeaponColors>(StringComparer.OrdinalIgnoreCase) {
		["Faust"] = Chrysalis,
		["Kathleen"] = Chrysalis,
		["Jennifer"] = Chrysalis,
		["Jennifer_alt"] = Chrysalis,
		["Rei"] = Plasma,
		["Reivax"] = Plasma,
		["temp star"] = Plasma
	};
	public record struct WeaponColors(Matrix InitialMult, Matrix FinalColor, Matrix OverbrightColor, Vector3 ExtraBeamData) {
		public float BeamScale { get; set; } = 1;
		public float AOEScale { get; set; } = 1;
		public Vector4 OverbrightMax { get; set; } = new(float.PositiveInfinity);
		public float DustMinBrightness { get; set; } = 0;
		public float DustMaxBrightness { get; set; } = 1;
		public WeaponColors(float InitialMult, Matrix FinalColor) : this(Matrix.Multiply(Matrix.Identity with { M44 = 0 }, InitialMult), FinalColor, Matrix.Identity with { M44 = 0 }, new(1f / 3, 0.075f, 1.2f)) { }
		public readonly Vector4 GetDustColor() => GetColor(Main.rand.NextFloat(DustMinBrightness, DustMaxBrightness));
		public readonly Vector4 GetColor(float brightness) {
			Vector4 color = new(brightness, brightness, brightness, 1);
			color = InitialMult.TransposedTransform(color);
			Vector4 overbrightness = Vector4.Max(color - Vector4.One, Vector4.Zero);
			return FinalColor.TransposedTransform(color - overbrightness) + OverbrightColor.TransposedTransform(Vector4.Min(overbrightness, OverbrightMax));
		}
		public static Matrix Create(int r, int g, int b, int a = 0) => new(
			r / 255f, 0, 0, 0,
			g / 255f, 0, 0, 0,
			b / 255f, 0, 0, 0,
			a / 255f, 0, 0, 0
		);
		public static Matrix Create(float r, float g, float b, float a = 0) => new(
			r, 0, 0, 0,
			g, 0, 0, 0,
			b, 0, 0, 0,
			a, 0, 0, 0
		);
		public static WeaponColors CreateGradient(Color a, GradientData b) => new(
			new(
				1, 0, 0, 0,
				b.Factor, 0, 0, 1 + b.Start,
				0, 0, 0, 0,
				0, 0, 0, 0
			),
			WeaponColors.Create(a.R, a.G, a.B),
			new(
				0, b.Color.X - a.R / 255f, 0, 0,
				0, b.Color.Y - a.G / 255f, 0, 0,
				0, b.Color.Z - a.B / 255f, 0, 0,
				0, b.Color.W, 0, 0
			),
			default
		) {
			OverbrightMax = Vector4.One
		};
		public static WeaponColors CreateGradient(Color a, GradientData b, GradientData c) => new(
			new(
				1, 0, 0, 0,
				b.Factor, 0, 0, 1 + b.Start,
				c.Factor, 0, 0, 1 + c.Start,
				0, 0, 0, 0
			),
			WeaponColors.Create(a.R, a.G, a.B),
			new(
				0, b.Color.X - a.R / 255f, c.Color.X - b.Color.X, 0,
				0, b.Color.Y - a.G / 255f, c.Color.Y - b.Color.Y, 0,
				0, b.Color.Z - a.B / 255f, c.Color.Z - b.Color.Z, 0,
				0, b.Color.W, c.Color.W - b.Color.W, 0
			),
			default
		) {
			OverbrightMax = Vector4.One
		};
		public static WeaponColors CreateGradient(Color a, GradientData b, GradientData c, GradientData d) => new(
			new(
				1, 0, 0, 0,
				b.Factor, 0, 0, 1 + b.Start,
				c.Factor, 0, 0, 1 + c.Start,
				0, 0, 0, 0
			),
			WeaponColors.Create(a.R, a.G, a.B),
			new(
				0, b.Color.X - a.R / 255f, c.Color.X - b.Color.X, d.Color.X - c.Color.X,
				0, b.Color.Y - a.G / 255f, c.Color.Y - b.Color.Y, d.Color.Y - c.Color.Y,
				0, b.Color.Z - a.B / 255f, c.Color.Z - b.Color.Z, d.Color.Z - c.Color.Z,
				0, b.Color.W, c.Color.W - b.Color.W, d.Color.W - c.Color.W
			),
			default
		) {
			OverbrightMax = Vector4.One
		};
		public record struct GradientData(Vector4 Color, float Start, float Factor) {
			public GradientData(Color Color, float Start, float Factor) : this(Color.ToVector4() * new Vector4(1, 1, 1, 0), Start, Factor) { }
		}
	}
	#endregion
}
public abstract class Star_Soldier_Weapon : ModItem, IExpectToBeUnobtainable {
	public Asset<Texture2D> Icon { get; private set; }
	public static IReadOnlyList<Star_Soldier_Weapon> Weapons => field ??= ModContent.GetContent<Star_Soldier_Weapon>().OrderBy(w => w.Item.DamageType.Type).ToList();
	public override ModItem NewInstance(Item entity) {
		Star_Soldier_Weapon item = (Star_Soldier_Weapon)base.NewInstance(entity);
		item.Icon = Icon;
		return item;
	}
	public override void AutoStaticDefaults() {
		base.AutoStaticDefaults();
		Icon = ModContent.Request<Texture2D>(Texture + "_Icon");
	}
	public virtual void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) { }
	public virtual void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) { }
	public virtual void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) { }
	public virtual bool UpdateRotations(Player player, ref Star_Soldier.MountHandler.Arm arm) => true;
	public override bool NeedsAmmo(Player player) => false;
	public abstract void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale);
	public virtual void PlaySound(Player player) { }
	#region methods which are never used
	public sealed override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) { }
	public sealed override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) { }
	public sealed override void ModifyHitPvp(Player player, Player target, ref Player.HurtModifiers modifiers) { }
	public sealed override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) { }
	#endregion
}
public class Star_Soldier_Blade : Star_Soldier_Weapon {
	static int CooldownTime => 60;
	float cooldown;
	float cooldownAlpha;
	public override void SetStaticDefaults() {
		Origins.AddGlowMask(this);
	}
	public override void SetDefaults() {
		Item.damage = 500;
		Item.DamageType = DamageClass.Melee;
		Item.useAnimation = 35;
		Item.useTime = 35;
		Item.shootSpeed = 1;
		//Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(2f).WithVolume(0.6f);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.width = 60;
		Item.height = 26;
		Item.shoot = ModContent.ProjectileType<Star_Soldier_Blade_P>();
		Item.noMelee = true;
		Item.knockBack = 2.5f;
		Item.UseSound = SoundID.Item105.WithPitch(-0.6f);
	}
	public override bool CanUseItem(Player player) => cooldown == 0;
	public override void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) => player.statDefense.AdditiveBonus += 0.1f;
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		cooldown.Cooldown(rate: player.GetWeaponAttackSpeed(Item));
		if (arm.itemAnimation != 0) cooldown = CooldownTime;
		if (cooldown == 0) cooldownAlpha.Cooldown(rate: 1f / 15);
		else cooldownAlpha = 1;
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale) {
		if (this.cooldownAlpha == 0 || cooldown >= CooldownTime) return;
		float cooldownAlpha = this.cooldownAlpha * this.cooldownAlpha;
		int width = 64;
		int halfWidth = width / 2;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position,
			new Rectangle(0, 0, width, 4),
			Color.OrangeRed * cooldownAlpha,
			0,
			new Vector2(halfWidth, 2),
			1,
			SpriteEffects.None,
		0);
		int factor = (int)((cooldown * width) / (CooldownTime - 1));
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position + halfWidth * Vector2.UnitX,
			new Rectangle(0, 0, factor, 4),
			Color.Black * cooldownAlpha,
			0,
			new Vector2(factor, 2),
			1,
			SpriteEffects.None,
		0);
		position.Y += 8 * float.Pow(Utils.GetLerpValue(0, 0.4f, this.cooldownAlpha, true), 2);
	}
	public class Star_Soldier_Blade_P : ModProjectile, IElementalProjectile {
		static readonly AdvancedMiscShaderData bladeShader = new(ModContent.Request<Effect>("Origins/Effects/Strip"), "StarSoldierLaserBlade", [
			new("uColorMatrix0", Matrix.Identity with { M44 = 0 })
		]);
		public static Parameter uColorMatrix1;
		public static Parameter uFinalColorMatrix;
		public static Parameter uOverbrightMatrix;
		public static Parameter uOverbrightMax;
		public const int trail_length = 20;
		public ushort Element => Elements.Fire;
		public override string Texture => "Origins/Items/Weapons/Melee/Personal_Laser_Blade";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.TrailingMode[Projectile.type] = -1;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = trail_length * 2;
			OriginsSets.Projectiles.FireProjectiles[Type] = true;
			ID = Type;
			bladeShader.LoadThen(() => {
				bladeShader.CreateParameter(ref uColorMatrix1, nameof(uColorMatrix1), Matrix.Identity with { M44 = 0 });
				bladeShader.CreateParameter(ref uFinalColorMatrix, nameof(uFinalColorMatrix), Matrix.Identity);
				bladeShader.CreateParameter(ref uOverbrightMatrix, nameof(uOverbrightMatrix), Matrix.Identity);
				bladeShader.CreateParameter(ref uOverbrightMax, nameof(uOverbrightMax), new Vector4(float.PositiveInfinity));
			});
		}
		protected static int HitboxSteps => 7;
		protected static float Startup => 0.25f;
		protected static float End => 0.25f;
		protected static float SwingStartVelocity => 1f;
		protected static float SwingEndVelocity => 1f;
		protected static float TimeoutVelocity => 1f;
		protected static float MinAngle => -2.5f;
		protected static float MaxAngle => 2.5f;
		protected Rectangle lastHitHitbox;
		Star_Soldier.WeaponColors? colors = default(Star_Soldier.WeaponColors);
		Star_Soldier.WeaponColors Colors => colors ?? (OriginsModIntegrations.CheckAprilFools() ? Star_Soldier.RGB : Star_Soldier.DefaultColors);
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 3;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
			Projectile.noEnchantmentVisuals = true;
			DrawHeldProjInFrontOfHeldItemAndArms = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.scale *= itemUse.Item.scale;
				itemUse.Player.ApplyMeleeScale(ref Projectile.scale);
				Projectile.ai[1] = itemUse.Player.direction;
			}
			string[] contextArgs = source?.Context?.Split(';') ?? [];
			for (int i = 0; i < contextArgs.Length; i++) {
				if (contextArgs[i].StartsWith(nameof(Star_Soldier))) {
					_ = float.TryParse(contextArgs[i][nameof(Star_Soldier).Length..], out Projectile.ai[0]);
					break;
				}
			}
		}
		protected float SwingFactor {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (!Projectile.TryGetOwner(out Player player) || Star_Soldier.GetHandler(player) is not Star_Soldier.MountHandler handler) {
				Projectile.Kill();
				return;
			}
			ref Star_Soldier.MountHandler.Arm arm = ref (Projectile.ai[0] == 1 ? ref handler.altItem : ref handler.chosenItem);
			if (arm.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}
			Projectile.hide = (Projectile.ai[0] == 1) == (player.direction == 1);
			if (Projectile.hide) player.heldProj = Projectile.whoAmI;
			Star_Soldier.InitializeColor(player, ref colors);
			float updateOffset = (Projectile.MaxUpdates - (Projectile.numUpdates + 1)) / (float)(Projectile.MaxUpdates + 1);
			SwingFactor = ((arm.itemTime - updateOffset) / (float)arm.itemTimeMax) * (1 + Startup + End) - End;
			if (SwingFactor > 0) SwingFactor = MathHelper.Lerp(MathF.Pow(SwingFactor, 2f), MathF.Pow(SwingFactor, 0.5f), SwingFactor * SwingFactor);
			Projectile.rotation = MathHelper.Lerp(
				MaxAngle,
				MinAngle,
				MathHelper.Clamp(SwingFactor, 0, 1)
			) * Projectile.ai[1] * player.gravDir;

			float realRotation = Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() * player.gravDir;
			arm.shoulderRotation = realRotation + (SwingFactor - 1) * 1.65f * Projectile.ai[1];
			arm.forearmRotation = realRotation - (SwingFactor - 1) * 0.1f * Projectile.ai[1];
			arm.gunRotation = realRotation - (SwingFactor - 1) * 0.25f * Projectile.ai[1];
			arm.GetPositions(player.MountedCenter, player.fullRotation, player.Directions, out _, out _, out Projectile.position);
			Projectile.position -= Projectile.Size * 0.5f;
			Projectile.position += arm.gunRotation.ToRotationVector2() * 52;
			Projectile.localAI[2] = arm.gunRotation;

			player.direction = Math.Sign(Projectile.velocity.X);
			Projectile.localAI[1].Cooldown();
			EmitEnchantmentVisuals();
			for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
				Projectile.oldPos[i] = Projectile.oldPos[i - 1];
				Projectile.oldRot[i] = Projectile.oldRot[i - 1];
				Projectile.oldSpriteDirection[i] = Projectile.oldSpriteDirection[i - 1];
			}
			Projectile.oldPos[0] = Projectile.position;
			Projectile.oldRot[0] = Projectile.localAI[2];
			Projectile.oldSpriteDirection[0] = Projectile.spriteDirection;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			if (Projectile.hide) overPlayers.Add(index);
		}
		public virtual void EmitEnchantmentVisuals() {
			Vector2 vel = Projectile.localAI[2].ToRotationVector2() * Projectile.velocity.Length() * Projectile.width * 0.95f;
			float velocityMult = 8;
			float rotMult = 0.05f;
			for (int j = 0; j <= HitboxSteps; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
				if (j > 1 && Main.rand.NextFloat(2 * Projectile.MaxUpdates) < 1 + Projectile.ai[0]) {
					Dust dust = Dust.NewDustDirect(
						Projectile.position + vel * j,
						Projectile.width, Projectile.height,
						DustID.PortalBoltTrail,
						newColor: new(Colors.GetDustColor())
					);
					dust.velocity = dust.velocity * 0.25f + Projectile.velocity.RotatedBy(Projectile.rotation * rotMult) * velocityMult;
					dust.position += dust.velocity * 2;
					dust.noGravity = true;
				}
			}
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + (Projectile.localAI[2].ToRotationVector2() * Projectile.velocity.Length() * Projectile.width * HitboxSteps);
			Utils.PlotTileLine(Projectile.Center, end, Projectile.width, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = Projectile.localAI[2].ToRotationVector2() * Projectile.velocity.Length() * Projectile.width;
			Vector2 additionalOffset = vel.SafeNormalize(default) * 12;
			for (int j = 0; j <= HitboxSteps; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j + additionalOffset;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					lastHitHitbox = hitbox;
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			LaserBladeDrawer trailDrawer = default;
			trailDrawer.Length = Projectile.velocity.Length() * Projectile.width * 0.9f * HitboxSteps;
			trailDrawer.Draw(Projectile, Colors);
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.5f;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= 1 + Projectile.ai[0] * 0.5f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			SoundEngine.PlaySound(SoundID.Item72.WithPitch(1.5f), target.Center);
			SoundEngine.PlaySound(SoundID.Item89.WithPitch(1.3f), target.Center);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			SoundEngine.PlaySound(SoundID.Item72.WithPitch(1.5f), target.Center);
			SoundEngine.PlaySound(SoundID.Item89.WithPitch(1.3f), target.Center);
		}
		public struct LaserBladeDrawer {

			private static readonly VertexStrip _vertexStrip = new();

			static Color TrailColor = new(35, 35, 35, 0);
			static Color BladeColor = new(255, 255, 255, 128);
			static Color BladeSecondaryColor = new(255, 255, 255, 64);

			public float Length;

			int[] spriteDirections;
			public void Draw(Projectile proj, in Star_Soldier.WeaponColors colors) {
				if (renderTarget is null) {
					Main.QueueMainThreadAction(SetupRenderTargets);
					Main.OnResolutionChanged += Resize;
					return;
				}
				Origins.shaderOroboros.Capture();
				Main.graphics.GraphicsDevice.Clear(Color.Black);
				{
					MiscShaderData miscShaderData = GameShaders.Misc["EmpressBlade"];
					int num = 1;//1
					int num2 = 0;//0
					int num3 = 0;//0
					float w = 0.6f;//0.6f
					miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
					miscShaderData.Apply();
					float[] oldRot = new float[proj.oldRot.Length];
					Vector2[] oldPos = new Vector2[proj.oldPos.Length];
					Vector2 move = new(Length - 30, 0);
					for (int i = 0; i < oldPos.Length; i++) {
						if (proj.oldPos[i] == default) {
							Array.Resize(ref oldRot, i);
							Array.Resize(ref oldPos, i);
							if (i == 0) return;
							break;
						}
						oldRot[i] = proj.oldRot[i] + MathHelper.PiOver2;
						oldPos[i] = proj.oldPos[i] + move.RotatedBy(oldRot[i] - MathHelper.PiOver2);
					}
					spriteDirections = proj.oldSpriteDirection;
					_vertexStrip.PrepareStrip(oldPos, oldRot, AfterimageColors, AfterimageWidth, -Main.screenPosition + proj.Size / 2f, oldPos.Length, includeBacksides: true);
					_vertexStrip.DrawTrail();
				}
				{
					MiscShaderData miscShaderData = GameShaders.Misc["Origins:LaserBlade"];
					Vector2 velocity = proj.localAI[2].ToRotationVector2() * Length * 1.333f;
					Vector2[] positions = new Vector2[15];
					for (int i = 0; i < positions.Length; i++) {
						positions[i] = proj.Center + velocity * ((i + 1) / (float)(positions.Length + 1));
					}
					float[] rotations = [.. Enumerable.Repeat(proj.velocity.ToRotation() + proj.rotation, positions.Length)];
					miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
					miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.FlameLashTrailShape]);
					miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
					miscShaderData.UseSaturation(-1);
					miscShaderData.UseOpacity(2);
					miscShaderData.Apply();
					_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, BladeSecondaryColors, BladeWidth, -Main.screenPosition, true);
					_vertexStrip.DrawTrail();

					miscShaderData.UseSaturation(0.5f);
					miscShaderData.Apply();
					_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, BladeColors, BladeWidth, -Main.screenPosition, true);
					_vertexStrip.DrawTrail();
				}
				Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));
				Origins.shaderOroboros.Reset(default);
				using GraphicsExt.SpritebatchOverride _ = Main.spriteBatch.OverrideState(SpriteSortMode.Immediate);
				//Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.Black);
				bladeShader.Apply(null,
					uColorMatrix1 with { Value = colors.InitialMult },
					uFinalColorMatrix with { Value = colors.FinalColor },
					uOverbrightMatrix with { Value = colors.OverbrightColor },
					uOverbrightMax with { Value = colors.OverbrightMax }
				);
				Main.spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}

			private readonly Color AfterimageColors(float progressOnStrip) {
				if (float.IsNaN(progressOnStrip)) return default;
				Color result = TrailColor * MathHelper.Lerp(1f, 0.5f, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
				result.A /= 2;
				result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
				return result;
			}
			private readonly float AfterimageWidth(float progressOnStrip) {
				return 60;
			}

			private readonly Color BladeColors(float progressOnStrip) {
				return BladeColor * (progressOnStrip == 0 ? 0 : 1);
			}
			private readonly Color BladeSecondaryColors(float progressOnStrip) {
				return BladeSecondaryColor * (progressOnStrip == 0 ? 0 : 1);
			}
			private readonly float BladeWidth(float progressOnStrip) {
				return 24 - 8 * progressOnStrip;
			}
			static RenderTarget2D renderTarget;
			static void Resize(Vector2 _) {
				if (Main.dedServ) return;
				renderTarget.Dispose();
				SetupRenderTargets();
			}
			static void SetupRenderTargets() {
				if (renderTarget is not null && !renderTarget.IsDisposed) return;
				renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			}
		}
	}
}
public class Star_Soldier_Gun : Star_Soldier_Weapon {
	static int AmmoMax => 32;
	static int ReloadLength => 51;
	int ammo = AmmoMax;
	int reloadTime = 0;
	bool usedFakeAmmo;
	public override void SetDefaults() {
		Item.damage = 50;
		Item.DamageType = DamageClass.Ranged;
		Item.useAnimation = 3;
		Item.useTime = 3;
		Item.shootSpeed = 19;
		Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(1.7f).WithVolume(0.6f);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.crit += 4;
		Item.width = 60;
		Item.height = 26;
		Item.shoot = ProjectileID.Bullet;
		Item.useAmmo = AmmoID.Bullet;
		Item.noMelee = true;
		Item.knockBack = 2.5f;
	}
	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		Vector2 offset = Vector2.Normalize(velocity);
		offset = offset * 24 + offset.RotatedBy(-MathHelper.PiOver2 * player.direction) * 8;
		position += offset;
		velocity = velocity.RotatedByRandom(0.08f);
	}
	public override bool CanUseItem(Player player) => ammo > 0;
	public override void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) => player.OriginPlayer().ammoConsumptionChance *= 0.9f;
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		if (arm.itemAnimation != 0) {
			//SoundEngine.PlaySound(SoundID.Item61.WithPitch(2f), player.Center);
			reloadTime = 0;
			if (arm.itemTime == arm.itemTimeMax && usedFakeAmmo) ammo--;
		} else if (ammo < AmmoMax && reloadTime.Warmup(ReloadLength)) {
			SoundEngine.PlaySound(SoundID.Item53.WithPitch(0.5f), player.Center);
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
			usedFakeAmmo = CombinedHooks.CanConsumeAmmo(player, Item, new(ItemID.MusketBall, ammo))
				&& (!player.ammoPotion || Main.rand.NextBool(4, 5))
				&& (!player.huntressAmmoCost90 || Main.rand.NextBool(9, 10))
				&& (!player.chloroAmmoCost80 || Main.rand.NextBool(4, 5))
				&& (!player.ammoCost80 || Main.rand.NextBool(4, 5))
				&& (!player.ammoCost75 || Main.rand.NextBool(3, 4));
		}
		return false;
	}
	static bool needsAmmoChecking = false;
	public override void OnConsumeAmmo(Item ammo, Player player) {
		SoundEngine.PlaySound(SoundID.Item149.WithPitch(-1f), player.Center);
		this.ammo--;
	}
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
		int ammoFactor = (ammo * width) / AmmoMax;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position + halfWidth * Vector2.UnitX,
			new Rectangle(0, 0, ammoFactor, 4),
			Color.OrangeRed,
			0,
			new Vector2(ammoFactor, 2),
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
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position + halfWidth * Vector2.UnitX,
			new Rectangle(0, 0, ammoFactor, 4),
			Color.OrangeRed,
			0,
			new Vector2(ammoFactor, 2),
			new Vector2(1, 0.5f),
			SpriteEffects.None,
		0);
		position.Y += 8;
	}
}
public class Star_Soldier_Laser : Star_Soldier_Weapon {
	static float AmmoMax => 100;
	static float ChargeSpeed => 0.75f;
	static float RechargeSpeed => 0.6f;
	float ammo = AmmoMax;
	bool recharging = false;
	public override void SetStaticDefaults() {
		Origins.AddGlowMask(this);
	}
	public override void SetDefaults() {
		Item.damage = 72;
		Item.DamageType = DamageClass.Magic;
		Item.useAnimation = 30;
		Item.useTime = 30;
		Item.shootSpeed = 19;
		//Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(2f).WithVolume(0.6f);
		Item.mana = 100;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.width = 60;
		Item.height = 26;
		Item.shoot = ModContent.ProjectileType<Star_Soldier_Laser_Beam>();
		Item.noMelee = true;
		Item.knockBack = 2.5f;
	}
	public override bool CanUseItem(Player player) => !recharging;
	public override void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) => player.GetAttackSpeed(DamageClass.Magic) *= 1.1f;
	public override void ModifyManaCost(Player player, ref float reduce, ref float mult) => mult *= player.GetWeaponAttackSpeed(Item);
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		if (arm.itemAnimation != 0 && control) {
			const float precision = 100;
			int manaRedirect = (int)(ammo * precision);
			using (new ScopedRedirect<int>(ref player.statMana, ref manaRedirect)) {
				using ScopedOverride<int> _ = player.statManaMax2.ScopedOverride((int)(AmmoMax * precision));
				if (!player.CheckMana(Item, pay: true, blockQuickMana: true)) recharging = true;
			}
			ammo = manaRedirect / precision;
			if (!recharging) {
				SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(2.7f).WithVolume(0.3f), player.Center);
				SoundEngine.SoundPlayer.Play(SoundID.Item72.WithVolume(0.3f), player.Center);
				arm.itemTime = arm.itemTimeMax;
				arm.itemAnimation = arm.itemAnimationMax;
			} else {
				SoundEngine.SoundPlayer.Play(SoundID.NPCHit43.WithPitch(0.6f).WithVolume(0.3f), player.Center);
				arm.itemTime = 0;
				arm.itemAnimation = 0;
			}
		} else {
			SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(0.1f + ammo / 20).WithVolume(0.2f - ammo / 400), player.Center);
			arm.itemTime = 0;
			arm.itemAnimation = 0;
			if (ammo.Warmup(AmmoMax, recharging ? RechargeSpeed : ChargeSpeed)) {
				recharging = false;
				SoundEngine.SoundPlayer.Play(SoundID.Item75.WithPitch(0.6f).WithVolume(0.3f), player.Center);
				SoundEngine.SoundPlayer.Play(SoundID.Item84.WithPitch(0.8f).WithVolume(0.9f), player.Center);
			}
		}
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
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position,
			new Rectangle(0, 0, (int)((ammo * width) / AmmoMax), 4),
			recharging ? Color.Purple : Color.DodgerBlue,
			0,
			new Vector2(halfWidth, 2),
			1,
			SpriteEffects.None,
		0);
		position.Y += 8;
	}
	public class Star_Soldier_Laser_Beam : ModProjectile {
		static readonly AdvancedMiscShaderData beamShader = new(ModContent.Request<Effect>("Origins/Effects/Beam"), "StarSoldierLaser", [
			new("uColorMatrix0", Matrix.Identity with { M44 = 0 })
		]);
		static readonly AdvancedMiscShaderData hitAOEShader = new(ModContent.Request<Effect>("Origins/Effects/Radial"), "StarSoldierLaserHit", [
			new("uOffset", new Vector2(0.5f)),
			new("uScale", float.Sqrt(0.5f)),
			new("uColorMatrix0", Matrix.Identity with { M44 = 0 })
		]);
		static class BeamParams {
			public static Parameter uColorMatrix1;
			public static Parameter uFinalColorMatrix;
			public static Parameter uOverbrightMatrix;
			public static Parameter uOverbrightMax;
			public static Parameter uShaderSpecificData;
		}
		static class AOEParams {
			public static Parameter uImageOffset1;
			public static Parameter uColorMatrix1;
			public static Parameter uFinalColorMatrix;
			public static Parameter uOverbrightMatrix;
			public static Parameter uOverbrightMax;
		}
		Star_Soldier.WeaponColors? colors = default(Star_Soldier.WeaponColors);
		Star_Soldier.WeaponColors Colors => colors ?? (OriginsModIntegrations.CheckAprilFools() ? Star_Soldier.RGB : Star_Soldier.DefaultColors);
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3200 + 64;
			hitAOEShader.UseSamplerState(SamplerState.PointWrap)
			.UseImage1(TextureAssets.MagicPixel);
			GameShaders.Misc["Origins:StarSoldierLaser"] = beamShader;
			GameShaders.Misc["Origins:StarSoldierLaserHit"] = hitAOEShader;
			beamShader.LoadThen(() => {
				beamShader.CreateParameter(ref BeamParams.uColorMatrix1, nameof(BeamParams.uColorMatrix1), Matrix.Identity with { M44 = 0 });
				beamShader.CreateParameter(ref BeamParams.uFinalColorMatrix, nameof(BeamParams.uFinalColorMatrix), Matrix.Identity);
				beamShader.CreateParameter(ref BeamParams.uOverbrightMatrix, nameof(BeamParams.uOverbrightMatrix), Matrix.Identity);
				beamShader.CreateParameter(ref BeamParams.uOverbrightMax, nameof(BeamParams.uOverbrightMax), new Vector4(float.PositiveInfinity));
				beamShader.CreateParameter(ref BeamParams.uShaderSpecificData, nameof(BeamParams.uShaderSpecificData), Vector4.Zero);
			});
			hitAOEShader.LoadThen(() => {
				hitAOEShader.CreateParameter(ref AOEParams.uImageOffset1, nameof(AOEParams.uImageOffset1), Vector2.Zero);
				hitAOEShader.CreateParameter(ref AOEParams.uColorMatrix1, nameof(AOEParams.uColorMatrix1), Matrix.Identity with { M44 = 0 });
				hitAOEShader.CreateParameter(ref AOEParams.uFinalColorMatrix, nameof(AOEParams.uFinalColorMatrix), Matrix.Identity);
				hitAOEShader.CreateParameter(ref AOEParams.uOverbrightMatrix, nameof(AOEParams.uOverbrightMatrix), Matrix.Identity);
				hitAOEShader.CreateParameter(ref AOEParams.uOverbrightMax, nameof(AOEParams.uOverbrightMax), new Vector4(float.PositiveInfinity));
			});
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
		}
		public override bool ShouldUpdatePosition() => false;
		public Vector2 TargetPos {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set => (Projectile.ai[0], Projectile.ai[1]) = value;
		}
		public override void OnSpawn(IEntitySource source) {
			string[] contextArgs = source?.Context?.Split(';') ?? [];
			for (int i = 0; i < contextArgs.Length; i++) {
				if (contextArgs[i].StartsWith(nameof(Star_Soldier))) {
					_ = float.TryParse(contextArgs[i][nameof(Star_Soldier).Length..], out Projectile.ai[2]);
					break;
				}
			}
		}
		public override void AI() {
			if (!Projectile.TryGetOwner(out Player owner) || Star_Soldier.GetHandler(owner) is not Star_Soldier.MountHandler handler) {
				Projectile.Kill();
				return;
			}
			Star_Soldier.MountHandler.Arm arm = Projectile.ai[2] == 1 ? handler.altItem : handler.chosenItem;
			if (arm.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}
			Star_Soldier.InitializeColor(owner, ref colors);
			Projectile.velocity = arm.gunRotation.ToRotationVector2();
			arm.GetPositions(owner.MountedCenter, owner.fullRotation, owner.Directions, out _, out _, out Projectile.position);
			Projectile.position += Projectile.velocity * 24;
			Vector2 targetPos = Projectile.position + Projectile.velocity * Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64);

			//SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(2.7f).WithVolume(0.5f), Projectile.Center);
			//SoundEngine.SoundPlayer.Play(SoundID.Item72.WithVolume(0.5f), Projectile.Center);
			Dust.NewDust(targetPos - Vector2.One * 2, 4, 4, DustID.PortalBoltTrail, newColor: new(Colors.GetDustColor()));
			Projectile.localAI[2] += 1f / 60;
			TargetPos = targetPos;
			//SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(++owner.ai[3] / 10).WithVolume(0.5f), Projectile.Center);
			//SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(owner.ai[3] / 20).WithVolume(0.5f), Projectile.Center);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (targetHitbox.IsWithin(TargetPos, 16 * 5)) return true;
			return targetHitbox.Contains(targetHitbox.Center().SnapToLine(Projectile.position, TargetPos, radius: 12));
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (!Projectile.TryGetOwner(out Player owner) || Star_Soldier.GetHandler(owner) is not Star_Soldier.MountHandler handler) return;
			Star_Soldier.MountHandler.Arm arm = Projectile.ai[2] == 1 ? handler.altItem : handler.chosenItem;
			modifiers.FinalDamage *= owner.GetWeaponAttackSpeed(arm.item);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			if (!Projectile.TryGetOwner(out Player owner) || Star_Soldier.GetHandler(owner) is not Star_Soldier.MountHandler handler) return;
			Star_Soldier.MountHandler.Arm arm = Projectile.ai[2] == 1 ? handler.altItem : handler.chosenItem;
			modifiers.FinalDamage *= owner.GetWeaponAttackSpeed(arm.item);
			if (!target.immune) modifiers = modifiers with { CooldownCounter = -2 };
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (info.CooldownCounter == -2) {
				target.immune = true;
				target.immuneTime = target.longInvince ? 16 : 8;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!TargetPos.IsWithin(TargetPos.Clamp(Main.screenPosition, Main.screenPosition + Main.ScreenSize.ToVector2()), 64) && !Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;
			using GraphicsExt.SpritebatchOverride _ = Main.spriteBatch.OverrideState(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap);
			hitAOEShader.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]).Apply(null,
				AOEParams.uImageOffset1 with { Value = new Vector2(Projectile.localAI[2], Projectile.localAI[2] * -0.5f) },
				AOEParams.uColorMatrix1 with { Value = Colors.InitialMult },
				AOEParams.uFinalColorMatrix with { Value = Colors.FinalColor },
				AOEParams.uOverbrightMatrix with { Value = Colors.OverbrightColor },
				AOEParams.uOverbrightMax with { Value = Colors.OverbrightMax }
			);
			Main.spriteBatch.Draw(
				TextureAssets.Projectile[Type].Value,
				TargetPos - Main.screenPosition,
				null,
				default,
				Projectile.localAI[2],
				Vector2.One * 128,
				Vector2.One * 5 * Colors.AOEScale,
				0,
			0);
			Vector2 diff = TargetPos - Projectile.position;
			Vector2 position = Projectile.position;
			position -= Main.screenPosition;
			/*Main.spriteBatch.Draw(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,
				position,
				default
			);*/
			float rotation = diff.ToRotation();
			float dist = diff.Length();
			const float scale = 1f / 256f;
			DrawData data = new(
				TextureAssets.Extra[ExtrasID.MagicMissileTrailShape].Value,//TextureAssets.MagicPixel.Value,
				position,
				new(256 - (int)((Projectile.localAI[2] * 600) % 256), 0, (int)dist, 256),
				default,
				rotation,
				Vector2.UnitY * 128,
				new Vector2(1, 48 * scale * Colors.BeamScale),
				0
			);
			beamShader.Apply(null,
				BeamParams.uColorMatrix1 with { Value = Colors.InitialMult },
				BeamParams.uFinalColorMatrix with { Value = Colors.FinalColor },
				BeamParams.uOverbrightMatrix with { Value = Colors.OverbrightColor },
				BeamParams.uOverbrightMax with { Value = Colors.OverbrightMax },
				BeamParams.uShaderSpecificData with { Value = new Vector4(Colors.ExtraBeamData, 0) }
			);
			data.Draw(Main.spriteBatch);
			beamShader.Apply(null,
				BeamParams.uColorMatrix1 with { Value = Colors.InitialMult },
				BeamParams.uFinalColorMatrix with { Value = Colors.FinalColor },
				BeamParams.uOverbrightMatrix with { Value = Colors.OverbrightColor },
				BeamParams.uOverbrightMax with { Value = Colors.OverbrightMax },
				BeamParams.uShaderSpecificData with { Value = Vector4.Zero }
			);
			/*Main.spriteBatch.Draw(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,
				position,
				default
			);*/
			return false;
		}
		float Raymarch(Vector2 position, Vector2 direction, float maxLength = float.PositiveInfinity) {
			float dist = CollisionExt.Raymarch(position, direction, maxLength);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (dist < 16) return dist;
				if (npc.friendly) continue;
				if (position.Clamp(npc.Hitbox).DistanceSQ(position) >= dist * dist) continue;
				float collisionPoint = 1;
				if (Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, position + direction * dist, 1, ref collisionPoint)) {
					Min(ref dist, collisionPoint);
				}
			}
			if (Main.player[Projectile.owner] is Player { hostile: true } owner) {
				foreach (Player player in Main.ActivePlayers) {
					if (!player.InOpposingTeam(owner)) continue;
					if (dist < 16) return dist;
					if (position.Clamp(player.Hitbox).DistanceSQ(position) >= dist * dist) continue;
					float collisionPoint = 1;
					if (Collision.CheckAABBvLineCollision(player.position, player.Size, position, position + direction * dist, 1, ref collisionPoint)) {
						Min(ref dist, collisionPoint);
					}
				}
			}
			return dist;
		}
	}
}
public class Star_Soldier_Droner : Star_Soldier_Weapon {
	static int AmmoMax => 1;
	static int ReloadLength => 15 * 60;
	int ammo = AmmoMax;
	int reloadTime = 0;
	public override void SetStaticDefaults() {
		Origins.AddGlowMask(this);
	}
	public override void SetDefaults() {
		Item.damage = 54;
		Item.DamageType = DamageClass.Summon;
		Item.useAnimation = 12;
		Item.useTime = 12;
		Item.shootSpeed = 15;
		Item.knockBack = 4f;
		Item.useAmmo = AmmoID.Rocket;
		Item.shoot = ProjectileID.RocketI;
		Item.UseSound = Origins.Sounds.ThrusterChargeUp.WithPitch(3f).WithVolume(0.6f);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
	}
	public override bool CanUseItem(Player player) => ammo > 0;
	public override void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) { }
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		if (arm.itemAnimation != 0) {
			reloadTime = 0;
			if (arm.itemTime == arm.itemTimeMax) ammo--;
		} else if (ammo < AmmoMax) {
			if (reloadTime.CycleUp(ReloadLength)) {
				SoundEngine.PlaySound(SoundID.Item53.WithPitch(0.5f), player.Center);
				ammo.Warmup(AmmoMax);
			}
		}
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale) {
		int width = 64;
		int segment = width / AmmoMax;
		for (int i = 0; i < AmmoMax; i++) {
			Color color = Color.OrangeRed;
			if (i >= ammo) color = Color.Black;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, segment, 4),
				color,
				0,
				new Vector2(segment * 0.5f, 2),
				1,
				SpriteEffects.None,
			0);
		}
		position.Y += 8;
	}
}
public class Star_Soldier_Pod : Star_Soldier_Weapon {
	static int AmmoMax => 4;
	static int ReloadLength => 100 / AmmoMax;
	int ammo = AmmoMax;
	int reloadTime = 0;
	bool reloading = false;
	public override void SetStaticDefaults() {
		AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Type] = ItemID.RocketLauncher;
	}
	public override void SetDefaults() {
		Item.damage = 222;
		Item.DamageType = DamageClasses.Explosive;
		Item.useAnimation = 48;
		Item.useTime = 12;
		Item.shootSpeed = 24;
		Item.knockBack = 4f;
		Item.useAmmo = AmmoID.Rocket;
		Item.shoot = ProjectileID.RocketI;
		Item.UseSound = Origins.Sounds.ThrusterChargeUp.WithPitch(3f).WithVolume(0.6f);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
	}
	public override bool CanUseItem(Player player) => !reloading && ammo > 0;
	public override void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) => player.OriginPlayer().projectileSpeedBoost *= 1.1f;
	public override void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) {
		drawData.sourceRect = drawData.texture.Frame(verticalFrames: 2, frameY: reloading.ToInt());
	}
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		if (arm.itemAnimation != 0) {
			reloadTime = 0;
			if (arm.itemTime == arm.itemTimeMax) ammo--;
		} else if (reloading) {
			if (reloadTime.CycleUp(ReloadLength)) {
				SoundEngine.PlaySound(SoundID.Item53.WithPitch(0.5f).WithVolume(0.35f), player.MountedCenter);
				if (ammo.Warmup(AmmoMax)) {
					SoundEngine.PlaySound(SoundID.Item108.WithPitch(-0.5f).WithVolume(0.5f), player.MountedCenter);
					SoundEngine.PlaySound(SoundID.Item103.WithPitch(1.5f).WithVolume(0.5f), player.MountedCenter);
					reloading = false;
				}
			}
		} else if (ammo < AmmoMax) {
			reloading = true;
		}
	}
	public override void PlaySound(Player player) {
		SoundEngine.PlaySound(SoundID.Item108.WithPitch(-1f), player.MountedCenter);
		SoundEngine.PlaySound(SoundID.Item113.WithPitch(1.2f), player.MountedCenter);
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale) {
		int width = 64;
		int segment = (int)((width * 0.75f) / AmmoMax);
		for (int i = 0; i < AmmoMax; i++) {
			Color color = reloading ? Color.White : Color.OrangeRed;
			if (i >= ammo) color = Color.Black;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, segment, 4),
				color,
				0,
				new Vector2(segment * ((i - 0.5f * (AmmoMax - 1)) * 1.1f + i / (float)(AmmoMax - 1)), 2),
				1,
				SpriteEffects.None,
			0);
		}
		position.Y += 8;
	}
}
public class Star_Soldier_Proper_Buff : ModBuff {
	public override string Texture => "Origins/Buffs/Star_Soldier_Buff";
	protected virtual int MountID => ModContent.MountType<Star_Soldier>();
	public override void SetStaticDefaults() {
		BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
			mountID = MountID
		};
		Star_Soldier_UI.Sets.CustomBuffIndicator[Type] = _ => { };
	}
	public override void Update(Player player, ref int buffIndex) {
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.changeSize = true;
		originPlayer.targetWidth = 48;
		originPlayer.targetHeight = player.mount._data.heightBoost + Player.defaultHeight;
	}
}
public class Star_Soldier_Wagon : ModMount, IModifyTriggers {
	public class MountHandler {
		int time;
		int hoverIndex = 0;
		Star_Soldier_Weapon leftClickSelection;
		Star_Soldier_Weapon rightClickSelection;
		public void Update(Player player) {
			if (leftClickSelection is null || rightClickSelection is null) return;
			if (++time > 60) {
				player.mount.SetMount(ModContent.MountType<Star_Soldier>(), player, player.direction == -1);
				new Star_Soldier.Star_Soldier_Set_Weapons(player, leftClickSelection.Type, rightClickSelection.Type).Perform();
			}
		}
		public bool ModifyTriggers(Player player, TriggersSet triggersSet) {
			IReadOnlyList<Star_Soldier_Weapon> options = Star_Soldier_Weapon.Weapons;
			if (PlayerInput.ScrollWheelDelta.Abs(out int dir) >= 120) {
				if (dir < 0) hoverIndex.CycleUp(options.Count);
				else hoverIndex.CycleDownWithZero(options.Count);
			}
			PlayerInput.ScrollWheelDelta = 0;
			for (int i = 1; i <= 10; i++) {
				if (triggersSet.KeyStatus["Hotbar" + i] && i <= options.Count) {
					hoverIndex = i - 1;
				}
				triggersSet.KeyStatus["Hotbar" + i] = false;
			}
			if (PlayerInput.Triggers.Current.HotbarScrollCD == 0 && (triggersSet.HotbarPlus ^ triggersSet.HotbarMinus)) {
				if (triggersSet.HotbarPlus) hoverIndex.CycleUp(options.Count);
				if (triggersSet.HotbarMinus) hoverIndex.CycleDownWithZero(options.Count);
				PlayerInput.Triggers.Current.HotbarScrollCD = 8;
			} else if (!triggersSet.HotbarPlus && !triggersSet.HotbarMinus) PlayerInput.Triggers.Current.HotbarScrollCD = 0;
			if (PlayerInput.Triggers.JustPressed.MouseLeft) {
				if (leftClickSelection is null) leftClickSelection = options[hoverIndex];
				else leftClickSelection = null;
			}
			if (PlayerInput.Triggers.JustPressed.MouseRight) {
				if (rightClickSelection is null) rightClickSelection = options[hoverIndex];
				else rightClickSelection = null;
			}
			triggersSet.HotbarPlus = false;
			triggersSet.HotbarMinus = false;
			player.controlUseItem = false;
			player.controlUseTile = false;
			return false;
		}
		public void DrawUI(SpriteBatch spriteBatch) {
			IReadOnlyList<Star_Soldier_Weapon> options = Star_Soldier_Weapon.Weapons;
			Player player = Main.LocalPlayer;
			Vector2 pos = player.MountedCenter - Main.screenPosition;

			Main.UIScaleMatrix.Decompose(out Vector3 scale, out _, out _);
			pos.X = ((int)pos.X) / scale.X;
			pos.Y = ((int)pos.Y) / scale.Y;
			Vector2 iconsPos = pos - Vector2.UnitX * (player.width * 0.5f + 40);
			DrawData data = new(
				options[0].Icon.Value,
				iconsPos,
				Color.White
			) {
				origin = options[0].Icon.Value.Size() * 0.5f,
				scale = new(0.85f)
			};
			if (leftClickSelection is null) {
				for (int i = -1; i <= 1; i++) {
					data.texture = options[(hoverIndex + i + options.Count) % options.Count].Icon.Value;
					data.position = iconsPos + i * 50 * Vector2.UnitY;
					data.color = Color.White * (i == 0 ? 1 : 0.5f);
					data.Draw(spriteBatch);
				}
			} else {
				data.texture = leftClickSelection.Icon.Value;
				data.position = iconsPos;
				data.color = Color.White;
				data.Draw(spriteBatch);
			}
			iconsPos = pos + Vector2.UnitX * (player.width * 0.5f + 40);
			if (rightClickSelection is null) {
				for (int i = -1; i <= 1; i++) {
					data.texture = options[(hoverIndex + i + options.Count) % options.Count].Icon.Value;
					data.position = iconsPos + i * 50 * Vector2.UnitY;
					data.color = Color.White * (i == 0 ? 1 : 0.5f);
					data.Draw(spriteBatch);
				}
			} else {
				data.texture = rightClickSelection.Icon.Value;
				data.position = iconsPos;
				data.color = Color.White;
				data.Draw(spriteBatch);
			}
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
	public override void UpdateEffects(Player player) => GetHandler(player)?.Update(player);

	static MountHandler GetHandler(Player player) {
		if (!Main.LocalPlayer.mount.IsMount<Star_Soldier_Wagon>()) return null;
		if (player.mount._mountSpecificData is not MountHandler data) player.mount._mountSpecificData = data = new MountHandler();
		return data;
	}
	public bool ModifyTriggers(Player player, TriggersSet triggersSet) => GetHandler(player)?.ModifyTriggers(player, triggersSet) ?? true;
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
	public class Star_Soldier_UI : SwitchableUIState {
		public override void AddToList() => OriginSystem.Instance.MountHUD.AddState(this);
		public override bool IsActive() => !Main.LocalPlayer.dead && Main.LocalPlayer.mount.IsMount<Star_Soldier_Wagon>();
		public Star_Soldier_UI() : base() {
			OverrideSamplerState = SamplerState.PointClamp;
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) => GetHandler(Main.LocalPlayer)?.DrawUI(spriteBatch);
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
public class Star_Soldier_UI : SwitchableUIState {
	[ReinitializeDuringResizeArrays]
	public static class Sets {
		public static Action<SpriteBatch>[] CustomBuffIndicator = BuffID.Sets.Factory.CreateNamedSet($"{nameof(Star_Soldier)}_{nameof(CustomBuffIndicator)}")
		.Description("Replaces the normal buff icon drawing when the player is using the Star Soldier mount")
		.RegisterCustomSet<Action<SpriteBatch>>(null);
		public static Action<SpriteBatch>[] InfoDisplayOverride = new Action<SpriteBatch>[InfoDisplayLoader.InfoDisplayCount];
		static Sets() {
			InfoDisplayOverride[InfoDisplay.DepthMeter.Type] = InfoAccessoryHUD.DrawDepthMeter;
		}
	}
	static readonly AdvancedMiscShaderData healthBarShader = new(ModContent.Request<Effect>("Origins/Effects/HUD"), "HealthBar");
	public override InterfaceScaleType ScaleType => InterfaceScaleType.None;
	public override void AddToList() {
		OriginSystem.Instance.MountHUD.AddState(this);
		healthBarShader.LoadThen(() => {
			uColor.Bind(healthBarShader);
			uSecondaryColor.Bind(healthBarShader);
			uSaturation.Bind(healthBarShader);
		});
	}
	static readonly HashSet<string> hideLayers = [
		"Vanilla: Resource Bars",
		"Vanilla: Info Accessories Bar",
		"Vanilla: Hotbar",
		"ThoriumMod: Combat Icon"
	];
	static readonly HashSet<string> hideLayersInventory = [
		"Vanilla: Resource Bars",
		"Vanilla: Hotbar",
		"ThoriumMod: Combat Icon"
	];
	public static Parameter uColor = Parameter.uColor with { Value = Color.OrangeRed.ToVector3() };
	public static Parameter uSecondaryColor = Parameter.uSecondaryColor with { Value = Vector3.Zero };
	public static Parameter uSaturation = Parameter.uSaturation;
	public override bool IsActive() {
		if (Main.LocalPlayer.dead || !Main.LocalPlayer.mount.IsMount<Star_Soldier>()) return false;
		OriginSystem.hideInterfaceLayers = Main.playerInventory ? hideLayersInventory : hideLayers;
		return true;
	}
	// We could go full NieR and let the player interact with the UI components as items
	readonly IUISegment[] uiSegments = [
		new WeaponHUD(),
		new HPHUD(),
		new O2HUD(),
		new TargetingHUD(),
		new InfoAccessoryHUD(),
	];
	public Star_Soldier_UI() : base() {
		OverrideSamplerState = SamplerState.PointClamp;
	}
	protected override void DrawSelf(SpriteBatch spriteBatch) {
		Player player = Main.LocalPlayer;
		if (Star_Soldier.GetHandler(player) is not Star_Soldier.MountHandler handler) return;
		Main.UIScaleMatrix.Decompose(out Vector3 _uiScale, out _, out _);
		Vector2 uiScale = _uiScale.XY();

		for (int i = 0; i < uiSegments.Length; i++) uiSegments[i].Draw(spriteBatch, handler, uiScale);
	}
	interface IUISegment {
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale);
	}
	public static void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, Vector2 scale) {
		if (OriginAccessibilityConfig.ItemSpecificOptions.StarSoldier_HUDOutlines) {
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				Star_Soldier.Font,
				text,
				position,
				color,
				color.MultiplyRGB(new(100, 100, 100)),
				0,
				default,
				scale,
				spread: 1
			);
		} else {
			ChatManager.DrawColorCodedString(
				spriteBatch,
				Star_Soldier.Font,
				text,
				position,
				color,
				0,
				default,
				scale
			);
		}
	}
	public class WeaponHUD : IUISegment {
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			Player player = Main.LocalPlayer;
			Vector2 scale = Main.GameViewMatrix.Zoom * uiScale;

			Vector2 pos = player.Center - Main.screenPosition;
			pos.Y += player.height * 0.5f + 8;
			pos = pos.Floor();

			//pos = pos.Transform(Main.UIScaleMatrix);
			(handler.chosenItem.item?.ModItem as Star_Soldier_Weapon)?.DrawHud(spriteBatch, ref pos, scale);
			(handler.altItem.item?.ModItem as Star_Soldier_Weapon)?.DrawHud(spriteBatch, ref pos, scale);
		}
	}
	public class HPHUD : IUISegment {
		static readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[6] {
			new(default, Color.White, new(0, 0)),
			new(default, Color.White, new(0.05f, 0)),
			new(default, Color.White, new(0.05f, 1)),
			new(default, Color.White, new(0.95f, 0)),
			new(default, Color.White, new(0.95f, 1)),
			new(default, Color.White, new(1f, 0))
		};
		static readonly short[] dices = [
			0, 1, 2,
			1, 3, 2,
			2, 3, 4,
			4, 3, 5,
		];
		static readonly Vector2[] positions = new Vector2[6];
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			bool doOutlines = OriginAccessibilityConfig.ItemSpecificOptions.StarSoldier_HUDOutlines;
			Player player = Main.LocalPlayer;
			Vector2 hpSize = new Vector2(400, 16) * uiScale;
			{ //HP bar
				Vector2 position = new((Main.screenWidth - hpSize.X) * 0.5f, 8);
				Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
				Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
				Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				for (int i = 0; i < vertices.Length; i++) {
					positions[i] = position + hpSize * vertices[i].TextureCoordinate;
					if (!doOutlines) continue;
					vertices[i].Position = new Vector3(positions[i], 0);
					if (vertices[i].TextureCoordinate.Y == 0) vertices[i].Position.Y -= 4;
					else vertices[i].Position.Y += 2;
					switch (vertices[i].TextureCoordinate.X) {
						case 0:
						vertices[i].Position.X -= 8;
						break;
						case 0.05f:
						vertices[i].Position.X -= 2;
						break;
						case 0.95f:
						vertices[i].Position.X += 2;
						break;
						case 1:
						vertices[i].Position.X += 8;
						break;
					}
					vertices[i].Color = Color.OrangeRed.MultiplyRGB(new(100, 100, 100));
				}
				if (doOutlines) Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, 4);
				for (int i = 0; i < vertices.Length; i++) {
					vertices[i].Position = new Vector3(positions[i], 0);
					vertices[i].Color = Color.White;
				}

				healthBarShader.Apply(null,
					uColor,
					uSecondaryColor,
					uSaturation with { Value = handler.life / (float)Star_Soldier.MountHandler.MaxLife }
				);
				Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, 4);
			}
			{ //buff icons
				Point position = new((int)((Main.screenWidth - hpSize.X) * 0.5f + hpSize.X * 0.075f), 28);
				Point currentPosition = position;
				int hovered = -1;
				for (int i = 0; i < Player.MaxBuffs; i++) {
					if (player.buffType[i] > 0) {
						if (Sets.CustomBuffIndicator[player.buffType[i]] is Action<SpriteBatch> customIcon) {
							customIcon(spriteBatch);
							continue;
						}
						hovered = Main.DrawBuffIcon(hovered, i, currentPosition.X, currentPosition.Y);
						currentPosition.X += 38;
						if (currentPosition.X >= position.X + 9 * 38) {
							currentPosition.X = position.X;
							currentPosition.Y += 50;
						}
					} else {
						Main.buffAlpha[i] = 0.4f;
					}
				}
				if (hovered >= 0) {
					int buffType = player.buffType[hovered];
					string buffName = Lang.GetBuffName(buffType);
					string buffTooltip = Main.GetBuffTooltip(player, buffType);

					if (player.buffType[hovered] == BuffID.MonsterBanner) Main.bannerMouseOver = true;
					int rare = Main.meleeBuff[buffType] ? -10 : 0;

					BuffLoader.ModifyBuffText(buffType, ref buffName, ref buffTooltip, ref rare);
					Main.instance.MouseTextHackZoom(buffName, rare, 0, buffTooltip);
				}
			}
		}
	}
	public class O2HUD : IUISegment {
		static readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4] {
			new(default, Color.White, new(0, 0)),
			new(default, Color.White, new(1, 0)),
			new(default, Color.White, new(0, 1)),
			new(default, Color.White, new(1, 1))
		};
		static readonly short[] dices = [
			0, 1, 2,
			1, 3, 2
		];
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			bool doOutlines = OriginAccessibilityConfig.ItemSpecificOptions.StarSoldier_HUDOutlines;
			Player player = Main.LocalPlayer;
			if (player.breath != player.breathMax) { //O2 bar
				Vector2 position = new(8, Main.screenHeight - 8);
				Vector2 width = new(8, 0);
				Vector2 height = new(0, -128);
				Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
				Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
				Main.instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
				if (doOutlines) {
					for (int i = 0; i < vertices.Length; i++) {
						Vector2 pos = position;
						if (i % 2 != 0) pos += height - Vector2.UnitY * 2;
						else pos.Y += 2;
						if (i >= 2) pos += width + Vector2.UnitX * 2;
						else pos.X -= 2;
						vertices[i].Position = new(pos, 0);
						vertices[i].Color = Color.OrangeRed.MultiplyRGB(new(100, 100, 100));
					}

					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, 4);
				}

				for (int i = 0; i < vertices.Length; i++) {
					Vector2 pos = position;
					if (i % 2 != 0) pos += height;
					if (i >= 2) pos += width;
					vertices[i].Position = new(pos, 0);
					vertices[i].Color = Color.White;
				}

				healthBarShader.Apply(null,
					uColor,
					uSecondaryColor,
					uSaturation with { Value = player.breath / (float)player.breathMax }
				);
				Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, dices, 0, 4);
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				DrawText(spriteBatch,
					"O2",
					position + width + height + Vector2.UnitX * 4,
					Color.OrangeRed,
					uiScale * 0.85f
				);
			}
		}
	}
	public class TargetingHUD : IUISegment {
		readonly (Vector2 pos, Vector4 dimensions)[] lines = [
			(Vector2.Zero, new(1, 0, 0, 2)),
			(Vector2.Zero, new(0, 1, 2, 0)),
			(Vector2.UnitX, new(1, 0, 0, 2)),
			(Vector2.UnitX, new(0, 1, 2, 0)),
			(Vector2.UnitY, new(1, 0, 0, 2)),
			(Vector2.UnitY, new(0, 1, 2, 0)),
			(Vector2.One, new(1, 0, 0, 2)),
			(Vector2.One, new(0, 1, 2, 0)),
			(Vector2.One * 0.5f, new(0, 0, 4, 4)),
		];
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			Player player = Main.LocalPlayer;
			float nearestDist = float.PositiveInfinity;
			Vector4 nearest = default;
			Entity nearestEntity = default;
			foreach (NPC npc in Main.ActiveNPCs) {
				Rectangle hitbox = npc.Hitbox;
				if (hitbox.Width == 0 || hitbox.Height == 0) continue;

				Color color = (npc.friendly || NPCID.Sets.CountsAsCritter[npc.type]) ? Color.Lime : Color.OrangeRed;
				if (npc == handler.LockOnTarget) color = Color.Red;
				hitbox.X -= (int)Main.screenPosition.X;
				hitbox.Y -= (int)Main.screenPosition.Y;

				Vector2 hitboxPos = hitbox.TopLeft().Transform(Main.GameViewMatrix.TransformationMatrix);
				Vector2 hitboxSize = hitbox.BottomRight().Transform(Main.GameViewMatrix.TransformationMatrix) - hitboxPos;

				if (!OriginExtensions.Intersects(hitboxPos, hitboxSize, Vector2.Zero, Main.ScreenSize.ToVector2())) continue;
				float dist = Main.MouseScreen.Clamp(hitboxPos, hitboxPos + hitboxSize).DistanceSQ(Main.MouseScreen);

				Rectangle rect = new(0, 0, 1, 1);
				Vector2 offset = uiScale * 8;
				Vector2 size = hitboxSize + offset * 2;
				for (int i = 0; i < lines.Length; i++) {
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						hitboxPos + lines[i].pos * size - offset,
						rect,
						color,
						0,
						lines[i].pos,
						size * lines[i].dimensions.XY() / 3 + uiScale * lines[i].dimensions.ZW(),
						SpriteEffects.None,
					0);
				}
				if (NPCID.Sets.ProjectileNPC[npc.type]) continue;
				if (npc.realLife != -1 && npc.realLife != npc.whoAmI) continue;
				if (Minimize(ref nearestDist, dist)) {
					nearest = new(hitboxPos - offset, size.X, size.Y);
					nearestEntity = npc;
				}
			}
			foreach (Player playerTarget in Main.ActivePlayers) {
				if (playerTarget == player) continue;
				Rectangle hitbox = playerTarget.Hitbox;
				if (hitbox.Width == 0 || hitbox.Height == 0) continue;

				Color color = playerTarget.InOpposingTeam(player) ? Color.OrangeRed : Color.Lime;
				if (playerTarget == handler.LockOnTarget) color = Color.Red;
				hitbox.X -= (int)Main.screenPosition.X;
				hitbox.Y -= (int)Main.screenPosition.Y;

				Vector2 hitboxPos = hitbox.TopLeft().Transform(Main.GameViewMatrix.TransformationMatrix);
				Vector2 hitboxSize = hitbox.BottomRight().Transform(Main.GameViewMatrix.TransformationMatrix) - hitboxPos;

				if (!OriginExtensions.Intersects(hitboxPos, hitboxSize, Vector2.Zero, Main.ScreenSize.ToVector2())) continue;
				float dist = Main.MouseScreen.Clamp(hitboxPos, hitboxPos + hitboxSize).DistanceSQ(Main.MouseScreen);

				Rectangle rect = new(0, 0, 1, 1);
				Vector2 offset = uiScale * 8;
				Vector2 size = hitboxSize + offset * 2;
				for (int i = 0; i < lines.Length; i++) {
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						hitboxPos + lines[i].pos * size - offset,
						rect,
						color,
						0,
						lines[i].pos,
						size * lines[i].dimensions.XY() / 3 + uiScale * lines[i].dimensions.ZW(),
						SpriteEffects.None,
					0);
				}
				if (Minimize(ref nearestDist, dist)) {
					nearest = new(hitboxPos - offset, size.X, size.Y);
					nearestEntity = playerTarget;
				}
			}
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if (projectile.ModProjectile is not IArtifactMinion) continue;
				Rectangle hitbox = projectile.Hitbox;
				if (hitbox.Width == 0 || hitbox.Height == 0) continue;

				Color color = (projectile.hostile || Main.player[projectile.owner].InOpposingTeam(player)) ? Color.OrangeRed : Color.Lime;
				if (projectile == handler.LockOnTarget) color = Color.Red;
				hitbox.X -= (int)Main.screenPosition.X;
				hitbox.Y -= (int)Main.screenPosition.Y;

				Vector2 hitboxPos = hitbox.TopLeft().Transform(Main.GameViewMatrix.TransformationMatrix);
				Vector2 hitboxSize = hitbox.BottomRight().Transform(Main.GameViewMatrix.TransformationMatrix) - hitboxPos;

				if (!OriginExtensions.Intersects(hitboxPos, hitboxSize, Vector2.Zero, Main.ScreenSize.ToVector2())) continue;
				float dist = Main.MouseScreen.Clamp(hitboxPos, hitboxPos + hitboxSize).DistanceSQ(Main.MouseScreen);

				Rectangle rect = new(0, 0, 1, 1);
				Vector2 offset = uiScale * 8;
				Vector2 size = hitboxSize + offset * 2;
				for (int i = 0; i < lines.Length; i++) {
					spriteBatch.Draw(
						TextureAssets.MagicPixel.Value,
						hitboxPos + lines[i].pos * size - offset,
						rect,
						color,
						0,
						lines[i].pos,
						size * lines[i].dimensions.XY() / 3 + uiScale * lines[i].dimensions.ZW(),
						SpriteEffects.None,
					0);
				}
				if (Minimize(ref nearestDist, dist)) {
					nearest = new(hitboxPos - offset, size.X, size.Y);
					nearestEntity = projectile;
				}
			}
			Entity displayEntity = handler.LockOnTarget ?? nearestEntity;
			if (displayEntity is not null) {
				if (Keybindings.StarSoldierLockOn.JustPressed) {
					if (nearestEntity is null || nearestEntity == handler.LockOnTarget || nearestDist > 16 * 16 * 15 * 15) {
						handler.LockOnTarget = null;
					} else {
						handler.LockOnTarget = nearestEntity;
						SoundEngine.PlaySound(SoundID.Chat.WithPitch(1.8f).WithVolume(0.6f), player.Center);
						SoundEngine.PlaySound(SoundID.Item91.WithPitch(1.6f).WithVolume(0.6f), player.Center);
					}
				}

				StringBuilder builder = new();
				Color color = Color.OrangeRed;
				switch (displayEntity) {
					case NPC npc:
					builder.AppendLine(npc.GivenOrTypeName);
					if (!npc.dontTakeDamage) builder.Append($"{npc.GetLifePercent():P0}");
					if (npc.friendly || NPCID.Sets.CountsAsCritter[npc.type]) color = Color.Lime;
					break;

					case Player playerTarget:
					builder.AppendLine(playerTarget.name);
					builder.Append($"{playerTarget.statLife / (float)playerTarget.statLifeMax2:P0}");
					if (!player.InOpposingTeam(playerTarget)) color = Color.Lime;
					break;

					case Projectile projectile:
					if (projectile.ModProjectile is not IArtifactMinion artifact) break;
					builder.AppendLine(projectile.Name);
					builder.Append($"{artifact.Life / artifact.MaxLife:P0}");
					if (!projectile.hostile && !Main.player[projectile.owner].InOpposingTeam(player)) color = Color.Lime;
					break;
				}
				if (handler.LockOnTarget is not null) color = Color.Red;
				string text = builder.ToString().Trim();
				DrawText(spriteBatch,
					text,
					nearest.XY() + new Vector2(nearest.Z, 0),
					color,
					uiScale
				);
			}
		}
	}
	public class InfoAccessoryHUD : IUISegment {
		static readonly FastStaticFieldInfo<List<InfoDisplay>> InfoDisplays = new(typeof(InfoDisplayLoader), nameof(InfoDisplays));
		public void Draw(SpriteBatch spriteBatch, Star_Soldier.MountHandler handler, Vector2 uiScale) {
			if (Main.playerInventory) return;
			DrawText(spriteBatch,
				"""
				ABCDEFGHIJKLMNOPQRSTUVWXYZ
				abcdefghijklmnopqrstuvwxyz
				 !$#"%&'()*+,-./0123456789
				:;<=>?@[\]^_`{|}~
				""",
				new Vector2(8, 8),
				Color.OrangeRed,
				Vector2.One
			);
			Player player = Main.LocalPlayer;

			int y = 8;
			int lineSpacing = 22;
			if (Main.screenHeight < 650) lineSpacing = 20;
			if (Main.mapStyle == 1 && Main.mapEnabled) {
				y = Main.screenHeight - 8 - 29;
				lineSpacing *= -1;
			}

			for (int i = 0; i < InfoDisplayLoader.InfoDisplayCount; i++) {
				InfoDisplay display = InfoDisplays.Value[i];
				if (!InfoDisplayLoader.Active(display) || player.hideInfo[i]) {
					continue;
				}
				if (Sets.InfoDisplayOverride[i] is Action<SpriteBatch> customDisplay) {
					customDisplay(spriteBatch);
					continue;
				}
				Color infoTextColor = Main.MouseTextColorReal.MultiplyRGBA(Color.OrangeRed);
				Color infoTextShadowColor = Color.OrangeRed.MultiplyRGB(new(100, 100, 100));
				string text = display.DisplayValue(ref infoTextColor, ref infoTextShadowColor);
				string name = "";
				switch (display) {
					case WatchesInfoDisplay: {
						double time = Main.time;
						if (!Main.dayTime) {
							// if it's night add this number
							time += 54000.0;
						}

						// Divide by seconds in a day * 24
						time = (time / 86400.0) * 24.0;
						// Dunno why we're taking 19.5. Something about hour formatting
						time = time - 7.5 - 12.0;
						// Format in readable time
						if (time < 0.0) {
							time += 24.0;
						}

						int intTime = (int)time;
						// Get the decimal points of time.
						double deltaTime = time - intTime;
						int seconds = ((int)(deltaTime * 60.0 * 60.0) % 60);
						// multiply them by 60. Minutes, probably
						deltaTime = (int)(deltaTime * 60.0);
						text = $"{intTime:0#}:{(int)deltaTime:0#}:{seconds:0#}";
						break;
					}
					case MetalDetectorInfoDisplay: {
						if (Main.SceneMetrics.bestOre <= 0) {
							continue;
						} else {
							int baseOption = 0;
							int tileType = Main.SceneMetrics.bestOre;
							if (Main.SceneMetrics.ClosestOrePosition.HasValue) {
								Point pos = Main.SceneMetrics.ClosestOrePosition.Value;
								Tile tile = Framing.GetTileSafely(pos);
								if (tile.HasTile) {
									MapHelper.GetTileBaseOption(pos.X, pos.Y, tile.TileType, tile, ref baseOption);
									tileType = tile.TileType;
									if (TileID.Sets.BasicChest[tileType] || TileID.Sets.BasicChestFake[tileType]) {
										baseOption = 0;
									}
								}
							}
							text = Language.GetTextValue("GameUI.OreDetected", Lang.GetMapObjectName(MapHelper.TileToLookup(tileType, baseOption)));
						}
						break;
					}
					case LifeformAnalyzerInfoDisplay: {
						const float maxDist = 1300;
						int bestRarity = 0;
						int index = -1;
						if (player.accCritterGuideCounter <= 0) {
							player.accCritterGuideCounter = 15;
							foreach (NPC npc in Main.ActiveNPCs) {
								if (npc.rarity > bestRarity && npc.Center.WithinRange(player.Center, maxDist)) {
									index = npc.whoAmI;
									bestRarity = npc.rarity;
								}
							}
							player.accCritterGuideNumber = (byte)index;
						} else {
							player.accCritterGuideCounter--;
							index = player.accCritterGuideNumber;
						}
						NPC rarestNPC = Main.npc.GetIfInRange(index);
						if ((rarestNPC?.active ?? false) && rarestNPC.rarity > 0) text = Main.npc[index].GivenOrTypeName;
						break;
					}
					case RadarInfoDisplay: {
						const float maxDist = 2000;
						if (player.accThirdEyeCounter <= 0) {
							player.accThirdEyeNumber = 0;
							player.accThirdEyeCounter = 15;
							foreach (NPC npc in Main.ActiveNPCs) {
								if (!npc.friendly && npc.damage > 0 && npc.lifeMax > 5 && !npc.dontCountMe && npc.Center.WithinRange(player.Center, maxDist)) {
									player.accThirdEyeNumber++;
								}
							}
						} else {
							player.accThirdEyeCounter--;
						}
						if (player.accThirdEyeNumber != 0) {
							text = player.accThirdEyeNumber != 1 ? Language.GetTextValue("GameUI.EnemiesNearby", player.accThirdEyeNumber) : Language.GetTextValue("GameUI.OneEnemyNearby");
						}
						break;
					}
					case TallyCounterInfoDisplay: {
						int lastCreatureHit = player.lastCreatureHit;
						if (lastCreatureHit > 0) {
							text = Lang.GetNPCNameValue(Item.BannerToNPC(lastCreatureHit)) + ": " + NPC.killCount[lastCreatureHit];
						}
						break;
					}
					case DPSMeterInfoDisplay: {
						player.checkDPSTime();
						if (player.getDPS() != 0) {
							text = Language.GetTextValue("GameUI.DPS", player.getDPS());
						}
						break;
					}
					case CompassInfoDisplay: {
						int xPos = (int)((player.position.X + (float)(player.width / 2)) * 2f / 16f - (float)Main.maxTilesX);
						text = (xPos > 0) ? Language.GetTextValue("GameUI.CompassEast", xPos) : ((xPos >= 0) ? Language.GetTextValue("GameUI.CompassCenter") : Language.GetTextValue("GameUI.CompassWest", -xPos));
						break;
					}
				}
#pragma warning disable CS0618 // Type or member is obsolete
				InfoDisplayLoader.ModifyDisplayValue(display, ref text);
				InfoDisplayLoader.ModifyDisplayName(display, ref name);
				InfoDisplayLoader.ModifyDisplayColor(display, ref infoTextColor, ref infoTextShadowColor);
#pragma warning restore CS0618 // Type or member is obsolete
				InfoDisplayLoader.ModifyDisplayParameters(display, ref text, ref name, ref infoTextColor, ref infoTextShadowColor);
				if (string.IsNullOrEmpty(text)) continue;

				Vector2 size = Star_Soldier.Font.MeasureString(text);
				infoTextColor = infoTextShadowColor * 2.55f;

				DrawText(spriteBatch, 
					text,
					new Vector2(Main.screenWidth - size.X - 8, y),
					infoTextColor,
					Vector2.One
				);
				y += lineSpacing;
			}
		}
		public static void DrawDepthMeter(SpriteBatch spriteBatch) {
			Player player = Main.LocalPlayer;
			int depth = -(int)(((player.position.Y + player.height) * 2f / 16f) - Main.worldSurface * 2.0);

			Vector2 size = Star_Soldier.Font.MeasureString(depth.ToString());

			DrawText(spriteBatch,
				$"{depth}'",
				new Vector2(Main.screenWidth - size.X - 8 - 52, Main.screenHeight * 0.5f),
				Color.OrangeRed,
				Vector2.One
			);

			Vector2 position = new(Main.screenWidth - 4, Main.screenHeight * 0.5f + 12);
			DrawData smallTick = new(TextureAssets.MagicPixel.Value, position, new Rectangle(0, 0, 24, 2), Color.OrangeRed) {
				origin = new(24, 1)
			};
			DrawData largeTick = smallTick with { scale = new Vector2(2, 1) };
			for (int i = depth - 200; i < depth + 200; i++) {
				if (i % 20 != 0) continue;
				DrawData data = i % 100 == 0 ? largeTick : smallTick;
				data.color *= 1 - float.Pow((i - depth) / 200f, 4);
				data.position.Y -= i - depth;
				data.Draw(spriteBatch);
			}
		}
	}
}