#define EditAnimation
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Origins.Journal;
using Origins.Layers;
using Origins.NPCs.Ashen;
using Origins.UI;
using PegasusLib.Networking;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using AnimationOffsets = Origins.Items.Mounts.Star_Soldier.Landing_Animation.AnimationOffsets;

namespace Origins.Items.Mounts.Star_Soldier;
public class Star_Soldier_Summon_Item : ModItem, ICustomWikiStat, IJournalEntrySource {
	public string EntryName => "Origins/" + typeof(Scrap_Wagon_Entry).Name;
	public class Scrap_Wagon_Entry : JournalEntry {
		public override string TextKey => "Scrap_Wagon";
		public override JournalSortIndex SortIndex => new("The_Ashen", 5);
	}
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
		Item.rare = ItemRarityID.Yellow;
		Item.value = Item.sellPrice(gold: 20);
	}
}
public class Star_Soldier : ModMount, IModifyTriggers {
	public static int BodyTextureFrames => 5;
	public static AutoLoadingTexture bodyTexture = typeof(Star_Soldier).GetDefaultTMLName();
	public static AutoLoadingTexture bodyGlowTexture = typeof(Star_Soldier).GetDefaultTMLName("_Glow");
	public static int LegTextureFrames => 19;
	public static AutoLoadingTexture frontLegTexture = typeof(Star_Soldier).GetDefaultTMLName("_Front_Leg");
	public static AutoLoadingTexture backLegTexture = typeof(Star_Soldier).GetDefaultTMLName("_Back_Leg");
	public static AutoLoadingTexture shoulderTexture = typeof(Star_Soldier).GetDefaultTMLName("_Shoulder");
	public static AutoLoadingTexture forearmTexture = typeof(Star_Soldier).GetDefaultTMLName("_Forearm");
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

		public int fallAnimationTime = -1;
		public static int MaxLife => 750;
		public float LifePercent => life / (float)MaxLife;
		public int life = MaxLife;
		public int dotCount = 0;

		public int bodyFrame;
		public float bodyFrameCounter;
		public int walkFrame;
		public float walkFrameCounter;

		public int maxDashes;
		public int availableDashes;
		public int dashCooldownMax;
		public int dashCooldown;
		public int dashLength;
		public int dashTime;
		public int dashSpeed;
		public float dashAngle;

		public int maxDrones;
		public int availableDrones = int.MaxValue;
		public int droneCooldown;

		public Arm chosenItem = new() { item = new(ModContent.ItemType<Star_Soldier_Laser>()), Index = 0 };
		public Arm altItem = new() { item = new(ModContent.ItemType<Star_Soldier_Gun>()), Index = 1 };
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
			player.AddBuff(
				ModContent.BuffType<Star_Soldier_Cooldown>(),
				(int)float.Lerp(Star_Soldier_Cooldown.DamagedCooldown, Star_Soldier_Cooldown.IntactCooldown, Utils.GetLerpValue(0.25f, 1, LifePercent, true))
			);
			OriginPlayer originPlayer = player.OriginPlayer();
			if (LockOnTarget is NPC target) {
				if (!target.active) LockOnTarget = null;
				else if (Star_Soldier_UI.Sets.OverrideDontDakeDamageTargeting[target.type] ?? target.dontTakeDamage) LockOnTarget = null;
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

			if (player.bleed.TrySet(false)) originPlayer.oiled = true;

			player.buffImmune[BuffID.Poisoned] = true;
			player.buffImmune[BuffID.Weak] = true;
			player.buffImmune[BuffID.WindPushed] = true;
			if (!originPlayer.oiled) {
				player.fireWalk = true;
				player.buffImmune[BuffID.OnFire] = true;
				player.buffImmune[BuffID.OnFire3] = true;
			}

			player.lifeRegenTime = 0;
			if (player.lifeRegenCount < 0) {
				dotCount -= player.lifeRegenCount;
				player.lifeRegenCount = 0;
			}
			int lifeLost = 0;
			while (dotCount.CycleUp(120, 0)) {
				life--;
				lifeLost++;
			}
			if (lifeLost > 0) CombatText.NewText(player.Hitbox, CombatText.LifeRegen, lifeLost, dramatic: false, dot: true);

			if (life <= 0) {
				player.statLife -= player.statLifeMax2 / 4 - 1;
				player.lifeRegenCount = -120;
				ForceDismount(player);
				SpawnGores(player.GetSource_Misc("DOT"), player.Center, player.velocity, player.direction);
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
					float speed = player.velocity.X.Abs(out int moveDir);
					if (speed < 0.5f && (player.controlRight.ToInt() - player.controlLeft.ToInt()) == 0) {
						walkFrame = 17;
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
				walkFrame = 18;
				walkFrameCounter = 0;
				jumpCounter = 0;
			}

			if ((player.controlJump && jumpCounter == 0) || dashTime > 0) {
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
		public static void SpawnGores(IEntitySource source, Vector2 center, Vector2 velocity, int direction) {
			SoundEngine.PlaySound(SoundID.NPCDeath44.WithPitch(-0.5f), center);
			SoundEngine.PlaySound(Origins.Sounds.PackhunterDeath.WithPitch(0.6f), center);
			SoundEngine.PlaySound(Origins.Sounds.PowerStomp.WithPitch(1.3f), center);
			SoundEngine.PlaySound(Origins.Sounds.VV13Idle.WithPitch(1.2f), center);
			Gore.NewGore(
				source,
				center + new Vector2(12 * direction, 13),
				velocity,
				Origins.instance.GetGoreSlot("Gores/Star_Soldier_Gore5")
			);
			Gore.NewGore(
				source,
				center + new Vector2(8 * direction, 27),
				velocity,
				Origins.instance.GetGoreSlot("Gores/Star_Soldier_Gore6")
			);
			Gore.NewGore(
				source,
				center + new Vector2(-6 * direction, -26),
				velocity,
				Origins.instance.GetGoreSlot("Gores/Star_Soldier_Gore3")
			);
			Gore.NewGore(
				source,
				center + new Vector2(25 * direction, -41),
				velocity,
				Origins.instance.GetGoreSlot("Gores/Star_Soldier_Gore2")
			);
			Gore.NewGore(
				source,
				center + new Vector2(-20 * direction, -38),
				velocity,
				Origins.instance.GetGoreSlot("Gores/Star_Soldier_Gore1")
			);
			Gore.NewGore(
				source,
				center + new Vector2(26 * direction, -2),
				velocity,
				Origins.instance.GetGoreSlot("Gores/Star_Soldier_Gore4E")
			);
			Gore.NewGore(
				source,
				center + new Vector2(-6 * direction, -29),
				velocity,
				Origins.instance.GetGoreSlot("Gores/Star_Soldier_Gore7")
			);
		}
		struct TODO_DamageRedirection : IBroken {
			public static string BrokenReason => "Use DamageRedirection";
		}
		public void HandleHurt(Player player, in Player.HurtInfo info) {
			SoundEngine.PlaySound(SoundID.Item37.WithPitch(-1.1f).WithVolume(1f), player.Center);
			SoundEngine.PlaySound(SoundID.NPCHit4.WithPitch(-0.5f).WithVolume(1f), player.Center);
			life -= info.Damage;
			if (life > 0) return;
			SpawnGores(player.GetSource_OnHurt(info.DamageSource), player.Center, player.velocity, player.direction);
			ForceDismount(player);
			player.Hurt(info with { Damage = player.statLifeMax2 / 4 });
		}
		public static void ForceDismount(Player player) {
			player.mount.Dismount(player);
			player.AddBuff(ModContent.BuffType<Star_Soldier_Cooldown>(), Star_Soldier_Cooldown.ForceDismountCooldown);
		}
		public void ItemCheck(Player player) {
			/// these are default stats, change stat changes in <see cref="Star_Soldier_Blade.UpdateEquipped"/> to change boosts
			dashCooldownMax = 90;
			maxDashes = 1;
			dashLength = 19;
			dashSpeed = 5;

			maxDrones = 0;
			droneCooldown.Cooldown();
			using ScopedOverride<bool> _ = player.controlUseTile.ScopedOverride(player.controlUseTile && !player.tileInteractionHappened && !player.mouseInterface);
			GetArm(0).Weapon.PreItemCheck(player, this, ref GetArm(0));
			GetArm(1).Weapon.PreItemCheck(player, this, ref GetArm(1));
			GetArm(0).ItemCheck(player, ref player.controlUseItem);
			GetArm(1).ItemCheck(player, ref player.controlUseTile);

			int remainingDrones = maxDrones - player.ownedProjectileCounts[ModContent.ProjectileType<Star_Soldier_Droner.Star_Soldier_Drone>()];
			if (availableDrones < remainingDrones) {
				if (droneCooldown == 0) {
					availableDrones++;
					droneCooldown = Star_Soldier_Droner.Cooldown;
				}
			} else Min(ref availableDrones, remainingDrones);
		}
		public void HandleDash(Player player) {
			if (dashTime > 0) {
				player.velocity.Y -= player.gravity;
				player.velocity = player.velocity.RotatedBy(-dashAngle);
				player.velocity.Y *= 0.75f;
				Max(ref player.velocity.X, 25);
				player.velocity = player.velocity.RotatedBy(dashAngle);
				if (dashTime.Cooldown()) {
					dashCooldown = dashCooldownMax;
					player.velocity *= 0.65f;
					availableDashes--;
				}
				return;
			}
			if (availableDashes < maxDashes && dashCooldown.CycleDown(dashCooldownMax)) availableDashes++;
			if (availableDashes <= 0) return;
			OriginPlayer originPlayer = player.OriginPlayer();
			if (originPlayer.dashDirection != 0 || originPlayer.dashDirectionY != 0) {
				player.timeSinceLastDashStarted = 0;
				Vector2 dir = new((player.controlRight ? 1 : 0) - (player.controlLeft ? 1 : 0), (player.controlDown ? 1 : 0) - (player.controlUp ? 1 : 0));
				if (dir == default) dir = Vector2.UnitX * -player.direction;
				dashAngle = dir.ToRotation();
				dashTime = dashLength;
				SoundEngine.PlaySound(Origins.Sounds.StarDash.WithVolume(0.7f), player.Center);
				/*SoundEngine.PlaySound(Origins.Sounds.EnergyRipple.WithPitchRange(0.5f, 0.7f).WithVolume(0.8f), player.Center);
				SoundEngine.PlaySound(SoundID.Item33.WithPitchRange(1.6f, 1.9f).WithVolume(0.8f), player.Center);
				SoundEngine.PlaySound(SoundID.Item73, player.Center);*/
			}
		}
		public struct Arm {
			public int Index { get; init; }
			public Item item;
			public int itemAnimation;
			public int itemAnimationMax;
			public int itemTime;
			public int itemTimeMax;
			static Player player;
			public float shoulderRotation;
			public float forearmRotation;
			public float gunRotation;
			public float rotationSpeedMult;
			public readonly bool IsFront(Player player) => (Index == 1) == (player.direction == 1);
			public readonly bool IsBack(Player player) => (Index == 1) != (player.direction == 1);
			public readonly Star_Soldier_Weapon Weapon => (Star_Soldier_Weapon)item.ModItem;
			public static void GetPositions(Vector2 basePosition, float baseRotation, float shoulderRotation, float forearmRotation, Vector2 directions, out Vector2 shoulderPos, out Vector2 forearmPos, out Vector2 gunPos) {
				shoulderPos = basePosition - (new Vector2(4, 20) * directions).RotatedBy(baseRotation);
				if (directions.Sum() == 0) baseRotation += MathHelper.Pi;
				forearmPos = shoulderPos + (new Vector2(-22, 24) * directions).RotatedBy(baseRotation + shoulderRotation);
				gunPos = forearmPos + (new Vector2(20, 16) * directions).RotatedBy(baseRotation + forearmRotation);
			}
			public readonly void GetPositions(Vector2 basePosition, float baseRotation, Vector2 directions, out Vector2 shoulderPos, out Vector2 forearmPos, out Vector2 gunPos) {
				GetPositions(basePosition, baseRotation, shoulderRotation, forearmRotation, directions, out shoulderPos, out forearmPos, out gunPos);
			}
			public void UpdateRotations(Player player) {
				rotationSpeedMult = 1;
				Weapon.UpdateRotations(player, ref this);
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
				if (item.UseSound is not null) new Star_Soldier_Weapon_Sound(player, item.type).Perform();
				Knockback = player.GetWeaponKnockback(item, Knockback);
				EntitySource_ItemUse_WithAmmo projectileSource = new(player, item, usedAmmoItemId, nameof(Star_Soldier) + currentArm);
				player.ApplyItemTime(item, callUseItem: false);

				GetPositions(player.MountedCenter, player.fullRotation, player.Directions, out _, out _, out Vector2 gunPos);

				Vector2 vector = gunRotation.ToRotationVector2();
				Vector2 velocity = vector * item.shootSpeed;
				CombinedHooks.ModifyShootStats(player, item, ref gunPos, ref velocity, ref projToShoot, ref Damage, ref Knockback);
				if (CombinedHooks.Shoot(player, item, projectileSource, gunPos, velocity, projToShoot, Damage, Knockback)) {
					Projectile.NewProjectile(projectileSource, gunPos, velocity, projToShoot, Damage, Knockback);
				}
			}
			public readonly void DrawArm(List<DrawData> playerDrawData, Color drawColor, float rotation, SpriteEffects spriteEffects, float drawScale, MountHandler handler, Vector2 bodyCenter, Player player = null) {
				if (item is null) return;
				player ??= Arm.player;
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
		OriginsSets.Mounts.DoNotOffsetDust[Type] = true;

		RainbowDyes = [
			GameShaders.Armor.GetShaderIdFromItemId(ItemID.RainbowDye),
			GameShaders.Armor.GetShaderIdFromItemId(ItemID.IntenseRainbowDye),
			GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye),
			GameShaders.Armor.GetShaderIdFromItemId(ItemID.MidnightRainbowDye)
		];
		On_Player.ScrollHotbar += On_Player_ScrollHotbar;
		if (!Main.dedServ) {
			Mount_Icon_Layer.IconTexture[Type] = ModContent.Request<Texture2D>(typeof(Star_Soldier).GetDefaultTMLName("_Icon"));
			Asset<Texture2D> fontTexture = ModContent.Request<Texture2D>("Origins/UI/Ashen_Font");
			Task.Run(FontAssets.ItemStack.Wait).ContinueWith(__ => {
				fontTexture.Wait();
				Rectangle glyph = new(0, 0, 10, 16);
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
				Font = FontGenerator.Monospace(FontAssets.ItemStack.Value, 0, 16, '*',
					range,
					Range('a'..'z'),
					Range(' '..'9', 17),
					Range(':'..'@', 34),
					Range('['..'`', 34, 77),
					Range('{'..'~', 34, 143),
					Single('©', 34, 187),
					Range('‘'..'‟', 34, 198)
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
	public override void SetMount(Player player, ref bool skipDust) {
		player.mount._mountSpecificData = new MountHandler();
	}
	struct HideItemHUD : IBroken {
		static string IBroken.BrokenReason => "Hide item HUD";
	}
	public override void UpdateEffects(Player player) {
		//SwitchableUIState.SharedInterfaces.ItemUseHUD.Hidden = true;
		player.statDefense += 48 - player.armor[0].defense - player.armor[1].defense - player.armor[2].defense;
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.knockbackTaken.Base -= 4.5f;
		player.AddMaxBreath(200);
		GetHandler(player)?.Update(player);
		originPlayer.mountOnly = true;
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
	internal static Vector2 GetBodyCenter(Player player, Vector2 hitboxCenter) => hitboxCenter - Vector2.UnitY * (player.height - 24);
	public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle _, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
		if (drawType == 3 && GetHandler(drawPlayer) is MountHandler handler) {
			DrawStarSoldier(
				playerDrawData,
				GetBodyCenter(drawPlayer, drawPosition),
				drawPlayer.cMount,
				drawColor,
				rotation,
				spriteEffects,
				drawScale,
				handler
			);
		}
		return false;
	}

	public static void DrawStarSoldier(List<DrawData> playerDrawData, Vector2 bodyCenter, int cMount, Color drawColor, float rotation, SpriteEffects spriteEffects, float drawScale, MountHandler handler, Player player = null) {
		Vector2 directions = spriteEffects.ApplyToOrigin(Vector2.One, default);
		float rotFlip = directions.X * directions.Y;
		AnimationOffsets backLegOffset = default, bodyOffset = default, frontLegOffset = default;
		int backLegFrame = handler.walkFrame, frontLegFrame = handler.walkFrame;
		#region drawing
		/*if (handler.fallAnimationTime >= 1) {
			//SoundEngine.PlaySound(Origins.Sounds.RivenBass.WithPitch(-handler.fallAnimationTime));
			SoundEngine.PlaySound(SoundID.NPCDeath44.WithPitch(-0.5f * handler.fallAnimationTime / 10).WithVolume(0.2f));
			SoundEngine.PlaySound(Origins.Sounds.PackhunterDeath.WithPitch(0.6f * handler.fallAnimationTime / 20).WithVolume(0.1f));
			SoundEngine.PlaySound(Origins.Sounds.PowerStomp.WithPitch(1.3f * handler.fallAnimationTime / 5).WithVolume(0.2f));
			SoundEngine.PlaySound(Origins.Sounds.VV13Idle.WithPitch(1.2f * handler.fallAnimationTime / 10).WithVolume(0.2f));
		}*/
		if (handler.fallAnimationTime >= 0) {
			AnimationOffsets wholeOffset = Landing_Animation.instance.wholeOffsetAnimation.GetCurrentValue(handler.fallAnimationTime);
			AnimationOffsets bothLegsOffset = Landing_Animation.instance.bothLegsAnimation.GetCurrentValue(handler.fallAnimationTime);
			backLegOffset = Landing_Animation.instance.backLegOffsetAnimation.GetCurrentValue(handler.fallAnimationTime);
			frontLegOffset = Landing_Animation.instance.frontLegOffsetAnimation.GetCurrentValue(handler.fallAnimationTime);
			backLegFrame = Landing_Animation.instance.backLegFrameAnimation.GetCurrentValue(handler.fallAnimationTime);
			frontLegFrame = Landing_Animation.instance.frontLegFrameAnimation.GetCurrentValue(handler.fallAnimationTime);
			backLegOffset += bothLegsOffset;
			frontLegOffset += bothLegsOffset;
			bodyCenter += wholeOffset.Position * directions;
			rotation += wholeOffset.Rotation * rotFlip;
		}
		Matrix rotationMatrix = Matrix.CreateRotationZ(rotation);
		Rectangle frame = backLegTexture.Frame(verticalFrames: LegTextureFrames, frameY: backLegFrame);
		Vector2 hips = bodyCenter + new Vector2(directions.X * -16, 32).Transform(rotationMatrix);

		(rotFlip < 0 ? handler.altItem : handler.chosenItem).DrawArm(playerDrawData, drawColor.MultiplyRGBA(Color.Gray), rotation, spriteEffects, drawScale, handler, bodyCenter, player);
		playerDrawData.Add(new(
			backLegTexture,
			hips + backLegOffset.Position * directions,
			frame,
			drawColor,
			rotation + backLegOffset.Rotation * rotFlip,
			spriteEffects.ApplyToOrigin(new(15, 23), frame),
			drawScale,
			spriteEffects
		) {
			shader = cMount
		});

		frame = bodyTexture.Frame(verticalFrames: BodyTextureFrames, frameY: handler.bodyFrame);
		DrawData bodyData = new(
			bodyTexture,
			bodyCenter + bodyOffset.Position * directions,
			frame,
			drawColor,
			rotation + bodyOffset.Rotation * rotFlip,
			spriteEffects.ApplyToOrigin(new(59, 37), frame),
			drawScale,
			spriteEffects
		) {
			shader = cMount
		};
		playerDrawData.Add(bodyData);
		bodyData.texture = bodyGlowTexture;
		bodyData.color = Color.White;
		playerDrawData.Add(bodyData);

		frame = frontLegTexture.Frame(verticalFrames: LegTextureFrames, frameY: frontLegFrame);
		playerDrawData.Add(new(
			frontLegTexture,
			hips + frontLegOffset.Position * directions,
			frame,
			drawColor,
			rotation + frontLegOffset.Rotation * rotFlip,
			spriteEffects.ApplyToOrigin(new(15, 23), frame),
			drawScale,
			spriteEffects
		) {
			shader = cMount
		});
		(rotFlip < 0 ? handler.chosenItem : handler.altItem).DrawArm(playerDrawData, drawColor, rotation, spriteEffects, drawScale, handler, bodyCenter, player);
		#endregion
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
public class Star_Soldier_Proper_Buff : ModBuff {
	public override string Texture => "Origins/Buffs/Star_Soldier_Buff";
	protected virtual int MountID => ModContent.MountType<Star_Soldier>();
	public override void SetStaticDefaults() {
		BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
			mountID = MountID
		};
		Star_Soldier_UI.Sets.CustomBuffIndicator[Type] = (_, _) => false;
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
		static float GroundOffset => 56;
		float fadeIconsOut = 1;
		int hoverIndex = 0;
		Star_Soldier_Weapon leftClickSelection;
		Star_Soldier_Weapon rightClickSelection;
		float fallPosition;
		public Star_Soldier.MountHandler fallingStarSoldier;
		public bool startedLanding;
		public bool hidePlayer;
		static KeyframeAnimation.PlayingState animationControls;
		public void Update(Player player) {
			if (leftClickSelection is null || rightClickSelection is null) return;
			MathUtils.LinearSmoothing(ref fadeIconsOut, 0, 1f / 60);
			if (fallingStarSoldier is null) {
				//SoundEngine.PlaySound(Origins.Sounds.PackhunterDeath.WithPitch(-1.5f), player.Center);
				SoundEngine.PlaySound(Origins.Sounds.SonarPing.WithPitch(-0.2f), player.Center);
				SoundEngine.PlaySound(Origins.Sounds.StarLaunch.WithPitch(-0.9f), player.Center);
				fallingStarSoldier ??= new() {
					chosenItem = new() { item = new(leftClickSelection.Type), Index = 0 },
					altItem = new() { item = new(rightClickSelection.Type), Index = 1 },
					fallAnimationTime = 0
				};
			}
			fallPosition += 48;
			int lastAnimationTime = fallingStarSoldier.fallAnimationTime;
			if (startedLanding || fallPosition > player.Bottom.Y - GroundOffset) {
				startedLanding = true;
				fallPosition = player.Bottom.Y - GroundOffset;
				animationControls.Play(ref fallingStarSoldier.fallAnimationTime, Landing_Animation.instance);
				Landing_Animation.instance.exampleEventTimeline.Update(new(player, this), lastAnimationTime, fallingStarSoldier.fallAnimationTime);
			}
			if (hidePlayer) player.OriginPlayer().mountOnly = true;
			if (fallingStarSoldier.fallAnimationTime > Landing_Animation.instance.totalLength) {
				player.mount.SetMount(ModContent.MountType<Star_Soldier>(), player, player.direction == -1);
				new Star_Soldier.Star_Soldier_Set_Weapons(player, leftClickSelection.Type, rightClickSelection.Type).Perform();
			}
		}
		public bool ModifyTriggers(Player player, TriggersSet triggersSet) {
			if (leftClickSelection is not null && rightClickSelection is not null) {
#if EditAnimation
				if (DebugConfig.Instance.AnimatorMode && PlayerInput.ScrollWheelDelta.Abs(out int _dir) >= 120) {
					fallingStarSoldier.fallAnimationTime -= _dir;
					Max(ref fallingStarSoldier.fallAnimationTime, 0);
				}
#endif
				PlayerInput.ScrollWheelDelta = 0;
				for (int i = 1; i <= 10; i++) triggersSet.KeyStatus["Hotbar" + i] = false;
				triggersSet.HotbarPlus = false;
				triggersSet.HotbarMinus = false;
				player.controlUseItem = false;
				player.controlUseTile = false;
				return false;
			}
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
#if EditAnimation
			if (DebugConfig.Instance.AnimatorMode && fallPosition >= Main.LocalPlayer.Bottom.Y - GroundOffset) {
				Landing_Animation.instance.gizmoBasePosition = new(Main.LocalPlayer.MountedCenter.X - Main.LocalPlayer.direction * 24, fallPosition - 28);
				Landing_Animation.instance.gizmoBasePosition -= Main.screenPosition;
				Landing_Animation.instance.DrawEditorUI(spriteBatch, ref fallingStarSoldier.fallAnimationTime, ref animationControls);
			}
#endif
			IReadOnlyList<Star_Soldier_Weapon> options = Star_Soldier_Weapon.Weapons;
			for (int i = 0; i < options.Count; i++) {
				if (options[i].Item.ToolTip is null) options[i].Item.SetDefaults(options[i].Item.type);
			}
			Player player = Main.LocalPlayer;
			Vector2 pos = player.MountedCenter - Main.screenPosition;

			Main.UIScaleMatrix.Decompose(out Vector3 scale, out _, out _);
			pos.X = ((int)pos.X) / scale.X;
			pos.Y = ((int)pos.Y) / scale.Y;
			Vector2 iconsPos = pos - Vector2.UnitX * (player.width * 0.5f + 40);
			Color baseColor = Color.White * fadeIconsOut;
			DrawData data = new(
				options[0].Icon.Value,
				iconsPos,
				baseColor
			) {
				origin = options[0].Icon.Value.Size() * 0.5f,
				scale = new(0.85f)
			};
			if (leftClickSelection is null) {
				for (int i = -1; i <= 1; i++) {
					Star_Soldier_Weapon weapon = options[(hoverIndex + i + options.Count) % options.Count];
					data.texture = weapon.Icon.Value;
					data.position = iconsPos + i * 50 * Vector2.UnitY;
					data.color = baseColor * (i == 0 ? 1 : 0.5f);
					data.Draw(spriteBatch);
					TryHover(data, weapon);
				}
			} else {
				data.texture = leftClickSelection.Icon.Value;
				data.position = iconsPos;
				data.color = baseColor;
				data.Draw(spriteBatch);
				TryHover(data, leftClickSelection);
			}
			iconsPos = pos + Vector2.UnitX * (player.width * 0.5f + 40);
			if (rightClickSelection is null) {
				for (int i = -1; i <= 1; i++) {
					Star_Soldier_Weapon weapon = options[(hoverIndex + i + options.Count) % options.Count];
					data.texture = weapon.Icon.Value;
					data.position = iconsPos + i * 50 * Vector2.UnitY;
					data.color = baseColor * (i == 0 ? 1 : 0.5f);
					data.Draw(spriteBatch);
					TryHover(data, weapon);
				}
			} else {
				data.texture = rightClickSelection.Icon.Value;
				data.position = iconsPos;
				data.color = baseColor;
				data.Draw(spriteBatch);
				TryHover(data, rightClickSelection);
			}
		}
		static void TryHover(in DrawData data, Star_Soldier_Weapon selection) {
			if (data.texture.Bounds.Recentered(data.position).Contains(Main.MouseScreen)) {
				Main.hoverItemName = selection.Item.Name;
				Main.HoverItem = selection.Item;
				Main.instance.MouseText(Main.hoverItemName, selection.Item.rare, 0);
			}
		}
		public void DrawFallingStar(List<DrawData> playerDrawData, Player drawPlayer, Vector2 drawPosition, Color drawColor, float rotation, SpriteEffects spriteEffects, float drawScale) {
			if (fallingStarSoldier is Star_Soldier.MountHandler fallingHandler) {
				drawPosition.X -= drawPlayer.direction * 24;
				drawPosition.Y = fallPosition - 12 - Main.screenPosition.Y;
				float shoulderRotation = Landing_Animation.instance.shoulderRotation.GetCurrentValue(fallingHandler.fallAnimationTime);
				float forearmRotation = Landing_Animation.instance.forearmRotation.GetCurrentValue(fallingHandler.fallAnimationTime);
				float gunRotation = Landing_Animation.instance.gunRotation.GetCurrentValue(fallingHandler.fallAnimationTime);
				if (drawPlayer.direction < 0) {
					shoulderRotation = MathHelper.Pi - shoulderRotation;
					forearmRotation = MathHelper.Pi - forearmRotation;
					gunRotation = MathHelper.Pi - gunRotation;
				}
				fallingHandler.chosenItem.shoulderRotation = shoulderRotation;
				fallingHandler.chosenItem.forearmRotation = forearmRotation;
				fallingHandler.chosenItem.gunRotation = gunRotation;
				fallingHandler.altItem.shoulderRotation = shoulderRotation;
				fallingHandler.altItem.forearmRotation = forearmRotation;
				fallingHandler.altItem.gunRotation = gunRotation;
				Star_Soldier.DrawStarSoldier(
					playerDrawData,
					Star_Soldier.GetBodyCenter(drawPlayer, drawPosition),
					drawPlayer.cMount,
					drawColor,
					rotation,
					spriteEffects,
					drawScale,
					fallingHandler,
					drawPlayer
				);
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
		OriginsSets.Mounts.MountCooldownBuff[Type] = ModContent.BuffType<Star_Soldier_Cooldown>();
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
		MountHandler handler = GetHandler(drawPlayer);
		if (drawType == 2 && !(handler?.hidePlayer ?? false)) {
			drawOrigin = texture.Size() * 0.5f;
			drawOrigin.X -= drawPlayer.direction * 4;
			frame = texture.Bounds;
			drawPosition.Y += 9;
			return true;
		}
		if (drawType == 3) handler?.DrawFallingStar(playerDrawData, drawPlayer, drawPosition, drawColor, rotation, spriteEffects, drawScale);
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
public class Star_Soldier_Cooldown : ModBuff {
	public static int IntactCooldown => 60 * 15;
	public static int DamagedCooldown => 60 * 140;
	public static int ForceDismountCooldown => 60 * 60 * 3;
	public override string Texture => "Origins/Buffs/Star_Soldier_Cooldown";
	public override void SetStaticDefaults() {
		Main.debuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		Star_Soldier_UI.Sets.CustomBuffIndicator[Type] = (_, _) => false;
	}
}
public class Surf_Music : ModSceneEffect {
	public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
	// Just have to put the track in a wrapper that ignores requests to stop early
	public override int Music => Origins.Music.SurfsUp;
	public override bool IsSceneEffectActive(Player _) {
		if (Music == -1) return false;
		Rectangle bounds = new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
		bounds.Inflate(Main.screenWidth, Main.screenHeight);
		foreach (Player player in Main.ActivePlayers) {
			if (player.Hitbox.Intersects(bounds) && player.mount.IsMount<Star_Soldier>() && player.OriginPlayer().standingOnPlatformNPC?.ModNPC is HA_24_Flying) return true;
		}
		return false;
	}
}
