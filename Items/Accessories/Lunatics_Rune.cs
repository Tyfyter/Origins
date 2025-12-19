using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Projectiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Items.Accessories {
	public class Lunatics_Rune : ModItem {
		static readonly List<Option> options = [];
		public static int ChargeThreshold => 2 * 60;
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationSwitching(OriginsModIntegrations.CheckAprilFools, NoDrawAnimation.AtAll, new DrawAnimationRandom(3, 20)));
			AprilFoolsTextures.AddItem(this);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.DamageType = DamageClass.Magic;
			Item.damage = 100;
			Item.mana = 120;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = tooltips.Count - 1; i >= 0; i--) {
				
			}
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) => LunaticsRuneAttack.ModifyWeaponDamage(player, ref damage);
		public static bool CheckMana(Player player, Item item, float multiplier, bool pay = true) {
			float reduce = player.manaCost;
			float mult = 1;

			CombinedHooks.ModifyManaCost(player, item, ref reduce, ref mult);
			int mana = Main.rand.RandomRound(item.mana * reduce * mult * multiplier);
			if (!pay) return player.statMana >= mana;

			if (player.statMana < mana) {
				CombinedHooks.OnMissingMana(player, item, mana);
				if (player.statMana < mana && player.manaFlower)
					player.QuickMana();
			}

			if (player.statMana < mana) return false;
			CombinedHooks.OnConsumeMana(player, item, mana);
			player.manaRegenDelay = player.maxRegenDelay;
			player.statMana -= mana;
			return true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.lunaticsRune = Item;
			ref int charge = ref originPlayer.lunaticsRuneCharge;
			if (player.SyncedKeybinds().LunaticsRune.Current && (charge >= ChargeThreshold || CheckMana(player, Item, 1f / ChargeThreshold))) {
				originPlayer.lunaticsRuneRotation += 0.02f;
				charge.Warmup(ChargeThreshold);
				float moveMult = 1 - float.Pow(charge / (float)ChargeThreshold, 2);
				player.velocity *= moveMult * moveMult;
				player.gravity *= moveMult;
				originPlayer.moveSpeedMult *= moveMult * moveMult;
				if (player.velocity.Y == 0) player.velocity.Y = float.Epsilon;
			} else {
				if (charge >= ChargeThreshold && player.whoAmI == Main.myPlayer) {
					for (int i = 0; i < player.buffType.Length; i++) {
						Max(ref player.buffTime[i], BuffOption.RitualRefreshTime[player.buffType[i]]);
					}
					RangeRandom random = new(Main.rand, 0, options.Count);
					for (int i = 0; i < options.Count; i++) {
						random.Multiply(i, i + 1, options[i].GetWeight(player));
					}
					if (random.AnyWeight) {
						options[random.Get()].Trigger(player);
					}
				}
				charge = 0;
			}
		}
		public class Attacks : BuffOption {
			public override int BuffType => ModContent.BuffType<Lunatics_Rune_Attacks_Buff>();
			public override int BuffTime => 20 * 60;
		}
		public class Duplicates : BuffOption {
			public override int BuffType => ModContent.BuffType<Lunatics_Rune_Duplicates_Buff>();
			public override int BuffTime => 16 * 60;
		}
		public class Healing : BuffOption {
			public override int BuffType => BuffID.RapidHealing;
			public override int BuffTime => 16 * 60;
		}
		[ReinitializeDuringResizeArrays]
		public abstract class BuffOption : Option {
			public static int[] RitualRefreshTime = BuffID.Sets.Factory.CreateIntSet();
			public abstract int BuffType { get; }
			public abstract int BuffTime { get; }
			public override void SetStaticDefaults() {
				RitualRefreshTime[BuffType] = BuffTime;
			}
			public override double GetWeight(Player player) => player.HasBuff(BuffType) ? 0 : 1;
			public override void Trigger(Player player) {
				player.AddBuff(BuffType, BuffTime);
			}
		}
		public abstract class Option : ModType {
			protected sealed override void Register() {
				options.Add(this);
			}
			public sealed override void SetupContent() {
				SetStaticDefaults();
			}
			public virtual double GetWeight(Player player) => 1;
			public abstract void Trigger(Player player);
		}
	}

	public class Lunatics_Rune_Attacks_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Item_" + ItemID.NebulaPickup1;
		public override void Update(Player player, ref int buffIndex) { }
	}
	public abstract class LunaticsRuneAttack : ModTexturedType {
		protected static bool shadowAttacking;
		static readonly List<LunaticsRuneAttack> options = [];
		public static void ModifyWeaponDamage(Player player, ref StatModifier damage) => options[player.OriginPlayer().lunaticsRuneSelectedAttack].ModifyAttackDamage(player, ref damage);
		public static bool ValidateSelection(ref int selectedAttack, bool scroll = false) {
			if (scroll) {
				while (selectedAttack < 0) selectedAttack += options.Count;
				while (selectedAttack >= options.Count) selectedAttack -= options.Count;
				return true;
			} else {
				return options.IndexInRange(selectedAttack);
			}
		}
		protected sealed override void Register() => options.Add(this);
		public Asset<Texture2D> texture;
		public virtual Rectangle Frame => texture.Frame();
		public sealed override void SetupContent() {
			if (!Main.dedServ) {
				texture = ModContent.Request<Texture2D>(Texture);
			}
			SetStaticDefaults();
		}
		public virtual bool CanStartAttack(Player player, bool justChecking = false) => true;
		public abstract void StartAttack(Player player);
		public abstract void ProcessAttack(Player player);
		public virtual void ModifyAttackDamage(Player player, ref StatModifier damage) { }
		protected static void ForPlayerAndShadows(Player player, Action<Player> action) {
			shadowAttacking = false;
			action(player);
			OriginPlayer originPlayer = player.OriginPlayer();
			if (originPlayer.lunaticDuplicates) {
				Vector2 position = player.position;
				Vector2 itemLocation = player.itemLocation;
				try {
					shadowAttacking = true;
					player.position = position + Vector2.UnitX * Lunatic_Shadow.Offset;
					player.itemLocation = itemLocation + Vector2.UnitX * Lunatic_Shadow.Offset;
					action(player);
					player.position = position - Vector2.UnitX * Lunatic_Shadow.Offset;
					player.itemLocation = itemLocation - Vector2.UnitX * Lunatic_Shadow.Offset;
					action(player);
				} finally {
					player.position = position;
					player.itemLocation = itemLocation;
					shadowAttacking = false;
				}
			}
		}
		public static void ItemCheck(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			Max(ref player.itemAnimation, 0);
			Max(ref player.itemTime, 0);
			player.itemLocation.X = 0;
			player.itemLocation.Y = 0;
			originPlayer.lunaticsRuneSelectedAttack %= options.Count;
			player.compositeBackArm.enabled = false;
			player.compositeFrontArm.enabled = false;
			player.itemTime.Cooldown();
			bool shouldAttack = (player.controlUseItem && player.releaseUseItem) || originPlayer.lunaticsRuneSelectedAttack != 0;
			if (player.ItemAnimationActive) {
				player.manaRegenDelay = player.maxRegenDelay;
				player.itemAnimation--;
				options[originPlayer.lunaticsRuneSelectedAttack].ProcessAttack(player);
			} else if (shouldAttack && options[originPlayer.lunaticsRuneSelectedAttack].CanStartAttack(player)) {
				options[originPlayer.lunaticsRuneSelectedAttack].StartAttack(player);
			}
			player.releaseUseItem = !player.controlUseItem;
			if (!player.ItemAnimationActive && player.ItemTimeIsZero) {
				originPlayer.lunaticsRuneSelectedAttack = 0;
			}

			int buffIndex = player.FindBuffIndex(ModContent.BuffType<Lunatics_Rune_Attacks_Buff>());
			if (buffIndex == -1) {
				player.itemTime = 0;
				player.itemAnimation = 0;
			} else if (player.buffTime[buffIndex] <= 2 && (player.ItemAnimationActive || !player.ItemTimeIsZero)) {
				player.buffTime[buffIndex] = 2;
			}
		}
		public static bool DrawSlots() {
			Player player = Main.LocalPlayer;
			OriginPlayer originPlayer = player.OriginPlayer();
			Texture2D backTexture;
			int posX = 20;
			for (int i = 1; i < 10 && i < options.Count; i++) {
				LunaticsRuneAttack attack = options[i];
				Color tint = Color.White;
				switch ((i == originPlayer.lunaticsRuneSelectedAttack, attack.CanStartAttack(player, true))) {
					case (true, true):
					case (true, false):
					backTexture = TextureAssets.InventoryBack14.Value;
					break;

					case (false, false):
					backTexture = TextureAssets.InventoryBack12.Value;
					tint = Color.DarkGray;
					break;

					default:
					backTexture = TextureAssets.InventoryBack.Value;
					break;
				}
				MathUtils.LinearSmoothing(ref Main.hotbarScale[i], 0.75f, 0.05f);
				float hotbarScale = Main.hotbarScale[i];
				int posY = (int)(20f + 22f * (1f - hotbarScale));

				if (!player.hbLocked && !PlayerInput.IgnoreMouseInterface && Main.mouseX >= posX && Main.mouseX <= posX + backTexture.Width * Main.hotbarScale[i] && Main.mouseY >= posY && Main.mouseY <= posY + backTexture.Height * Main.hotbarScale[i] && !player.channel) {
					player.mouseInterface = true;
					if (Main.mouseLeft && !player.hbLocked && !Main.blockMouse) {
						originPlayer.lunaticsRuneSelectedAttack = i;
					}
				}
				Main.spriteBatch.Draw(
					backTexture,
					new Vector2(posX, posY),
					null,
					Color.White,
					0f,
					Vector2.Zero,
					hotbarScale,
					SpriteEffects.None,
				0f);
				float offset = 26 * hotbarScale;
				Rectangle frame = attack.Frame;
				float scale = 1.15f;
				Min(ref scale, 48f / frame.Width);
				Min(ref scale, 48f / frame.Height);
				Main.spriteBatch.Draw(
					attack.texture.Value,
					new Vector2(posX + offset, posY + offset),
					frame,
					tint,
					0f,
					frame.Size() * 0.5f,
					hotbarScale * scale,
					SpriteEffects.None,
				0f);
				string text = i.ToString();
				if (text == "10") text = "0";
				ChatManager.DrawColorCodedStringWithShadow(
					Main.spriteBatch,
					FontAssets.ItemStack.Value,
					text,
					new Vector2(posX, posY) + new Vector2(8f, 4f) * hotbarScale,
					Color.White,
					0f,
					Vector2.Zero,
					new Vector2(hotbarScale),
					-1f,
					hotbarScale
				);
				posX += (int)(backTexture.Width * Main.hotbarScale[i]) + 4;
			}
			return true;
		}
	}
	public class FireballAttack : LunaticsRuneAttack {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossFireBall}";
		int frameCounter;
		int frame;
		public override Rectangle Frame {
			get {
				if (frameCounter.CycleUp(6)) frame.CycleUp(4);
				return texture.Frame(verticalFrames: 4, frameY: frame);
			}
		}
		public override void ModifyAttackDamage(Player player, ref StatModifier damage) {
			if (shadowAttacking) damage *= 0.5f;
		}
		public override void ProcessAttack(Player player) {
			if (player.ItemTimeIsZero) {
				player.itemTime = player.itemTimeMax;
				ForPlayerAndShadows(player, player => {
					Item item = player.OriginPlayer().lunaticsRune;
					Vector2 dir = (Main.MouseWorld - player.MountedCenter).Normalized(out _) * 8;
					if (shadowAttacking) {
						player.SpawnProjectile(player.GetSource_Accessory(item),
							player.MountedCenter,
							(Main.MouseWorld - player.MountedCenter).Normalized(out _) * 8,
							ModContent.ProjectileType<CultistFireballShadow>(),
							player.GetWeaponDamage(item),
							player.GetWeaponKnockback(item)
						);
					} else {
						player.SpawnProjectile(player.GetSource_Accessory(item),
							player.MountedCenter,
							(Main.MouseWorld - player.MountedCenter).Normalized(out _) * 8,
							ModContent.ProjectileType<CultistFireball>(),
							player.GetWeaponDamage(item),
							player.GetWeaponKnockback(item)
						);
						player.itemRotation = dir.ToRotation();
					}
				});
			}
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - MathHelper.PiOver2);
			player.direction = Math.Sign(float.Cos(player.itemRotation));
		}
		public override bool CanStartAttack(Player player, bool justChecking = false) {
			return Lunatics_Rune.CheckMana(player, player.OriginPlayer().lunaticsRune, 0.10f, !justChecking);
		}
		public override void StartAttack(Player player) {
			player.SetItemAnimation(30);
			player.itemTimeMax = player.itemAnimation / 3;
		}
		public class CultistFireball : ModProjectile {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossFireBall}";
			public virtual int FireDustType => DustID.Torch;
			public virtual Vector3 LightColor => new(1.1f, 0.9f, 0.4f);
			public override void SetDefaults() {
				Main.projFrames[Type] = 4;
				Projectile.width = 40;
				Projectile.height = 40;
				Projectile.friendly = true;
				Projectile.alpha = 255;
				Projectile.ignoreWater = true;
				Projectile.extraUpdates = 1;
			}
			public override void AI() {
				if (Projectile.soundDelay == 0) {
					Projectile.soundDelay = -1;
					SoundEngine.PlaySound(SoundID.Item34, Projectile.Center);
				}
				int currentTarget = (int)Projectile.ai[1];
				switch (Projectile.ai[0]) {
					case -1:
					break;

					case 1:
					float moveDir = Projectile.velocity.ToRotation();
					Vector2 targetPos;
					if (currentTarget >= 300) {
						targetPos = Main.npc[currentTarget - 300].Center;
					} else {
						targetPos = Main.player[currentTarget].Center;
					}
					Vector2 vector = targetPos - Projectile.Center;
					if (vector.Length() < 20f) {
						Projectile.Kill();
						return;
					}

					Projectile.velocity = moveDir.AngleLerp(vector.ToRotation(), 0.08f).ToRotationVector2() * Projectile.velocity.Length();
					break;

					default:
					float targetDist = 16 * 20;
					targetDist *= targetDist;
					Projectile.ai[0] = Main.player[Projectile.owner].DoHoming(target => {
						int targetIndex = target.whoAmI + (target is NPC).Mul(300);
						Vector2 currentPos = target.Center;
						if (targetIndex == currentTarget) currentPos = (currentPos + Projectile.Center) * 0.5f;
						float dist = Projectile.Center.DistanceSQ(currentPos);
						if (dist < targetDist && Collision.CanHit(Projectile.position, 1, 1, target.position, 1, 1)) {
							targetDist = dist;
							Projectile.ai[1] = targetIndex;
							Projectile.ai[0] = 1;
							return true;
						}
						return false;
					}).ToInt();
					break;
				}
				Projectile.alpha -= 40;
				if (Projectile.alpha < 0)
					Projectile.alpha = 0;

				Projectile.spriteDirection = Projectile.direction;
				if (Projectile.frameCounter.CycleUp(6)) Projectile.frame.CycleUp(4);

				Lighting.AddLight(Projectile.Center, LightColor);
				Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0.6f);
				Projectile.localAI[0] += 1f;
				if (Projectile.localAI[0] == 12f) {
					Projectile.localAI[0] = 0f;
					for (int i = 0; i < 12; i++) {
						Vector2 offset = Vector2.UnitX * -Projectile.width / 2f;
						offset += -Vector2.UnitY.RotatedBy(i * float.Pi / 6f) * new Vector2(8f, 16f);
						offset = offset.RotatedBy(Projectile.rotation - float.Pi / 2f);
						Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, FireDustType, Alpha: 160);
						dust.scale = 1.1f;
						dust.noGravity = true;
						dust.position = Projectile.Center + offset;
						dust.velocity = Projectile.velocity * 0.1f;
						dust.velocity = Vector2.Normalize(Projectile.Center - Projectile.velocity * 3f - dust.position) * 1.25f;
					}
				}

				if (Main.rand.NextBool(4)) {
					Vector2 offset = -Vector2.UnitX.RotatedByRandom(float.Pi / 16f).RotatedBy(Projectile.velocity.ToRotation());
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 100);
					dust.velocity *= 0.1f;
					dust.position = Projectile.Center + offset * Projectile.width / 2f;
					dust.fadeIn = 0.9f;
				}

				if (Main.rand.NextBool(32)) {
					Vector2 offset = -Vector2.UnitX.RotatedByRandom(float.Pi / 8f).RotatedBy(Projectile.velocity.ToRotation());
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Alpha: 155, Scale: 0.8f);
					dust.velocity *= 0.3f;
					dust.position = Projectile.Center + offset * Projectile.width / 2f;
					if (Main.rand.NextBool(2))
						dust.fadeIn = 1.4f;
				}

				if (Main.rand.NextBool(2)) {
					for (int i = 0; i < 2; i++) {
						Vector2 offset = -Vector2.UnitX.RotatedByRandom(MathHelper.PiOver4).RotatedBy(Projectile.velocity.ToRotation());
						Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, FireDustType, Scale: 1.2f);
						dust.velocity *= 0.3f;
						dust.noGravity = true;
						dust.position = Projectile.Center + offset * Projectile.width / 2f;
						if (Main.rand.NextBool(2))
							dust.fadeIn = 1.4f;
					}
				}
			}
			public override bool PreDraw(ref Color lightColor) {
				return base.PreDraw(ref lightColor);
			}
			public override void OnKill(int timeLeft) {
				ExplosiveGlobalProjectile.DoExplosion(Projectile, 176, false, SoundID.Item14, fireDustType: FireDustType, smokeDustAmount: 0, smokeGoreAmount: 0);
			}
		}
		public class CultistFireballShadow : CultistFireball {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossFireBallClone}";
			public override int FireDustType => DustID.Shadowflame;
			public override Vector3 LightColor => new(0.2f, 0.1f, 0.6f);
		}
	}
	public class IceMistAttack : LunaticsRuneAttack {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossIceMist}";
		public override void ProcessAttack(Player player) {
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - MathHelper.PiOver2);
			player.direction = Math.Sign(float.Cos(player.itemRotation));
		}
		public override bool CanStartAttack(Player player, bool justChecking = false) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<IceMist>()] > 0) return false;
			return Lunatics_Rune.CheckMana(player, player.OriginPlayer().lunaticsRune, 0.30f, !justChecking);
		}
		public override void StartAttack(Player player) {
			Item item = player.OriginPlayer().lunaticsRune;
			Vector2 dir = (Main.MouseWorld - player.MountedCenter).Normalized(out _);
			player.SpawnProjectile(player.GetSource_Accessory(item),
				player.MountedCenter,
				dir * 4,
				ModContent.ProjectileType<IceMist>(),
				player.GetWeaponDamage(item),
				player.GetWeaponKnockback(item)
			);
			player.itemRotation = dir.ToRotation();
			player.SetItemAnimation(20);
		}
		public class IceMist : ModProjectile {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossIceMist}";
			public override void SetDefaults() {
				Projectile.width = 60;
				Projectile.height = 60;
				Projectile.friendly = true;
				Projectile.tileCollide = false;
				Projectile.penetrate = -1;
				Projectile.alpha = 255;
				Projectile.ignoreWater = true;
			}
			public override void AI() {
				if (Projectile.localAI[1] == 0f) {
					Projectile.localAI[1] = 1f;
					SoundEngine.PlaySound(SoundID.Item120, Projectile.position);
				}

				Projectile.ai[0]++;
				if (Projectile.ai[0] >= 130f) Projectile.alpha += 10;
				else Projectile.alpha -= 10;
				Max(ref Projectile.alpha, 0);
				Min(ref Projectile.alpha, 255);

				if (Projectile.ai[0] >= 150f) {
					Projectile.Kill();
					return;
				}

				if (Projectile.ai[0] % 30f == 0f && Projectile.IsLocallyOwned()) {
					Vector2 vector82 = Projectile.rotation.ToRotationVector2();
					//Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center.X, Projectile.Center.Y, vector82.X, vector82.Y, Type, Projectile.damage, Projectile.knockBack, Projectile.owner);
				}
				const int HalfSpriteWidth = 92 / 2;
				const int HalfSpriteHeight = 102 / 2;

				int HalfProjWidth = Projectile.width / 2;
				int HalfProjHeight = Projectile.height / 2;

				// Vanilla configuration for "hitbox in middle of sprite"
				DrawOriginOffsetX = 0;
				DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
				DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);

				Projectile.rotation += (float)Math.PI / 30f;
				Lighting.AddLight(Projectile.Center, 0.3f, 0.75f, 0.9f);
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				Vector2 vector12 = new Vector2(0f, -720f).RotatedBy(Projectile.velocity.ToRotation());
				float num25 = Projectile.ai[0] % 45f / 45f;
				Vector2 spinningpoint = vector12 * num25;
				for (int num26 = 0; num26 < 6; num26++) {
					float num27 = num26 * ((float)Math.PI * 2f) / 6f;
					if (Utils.CenteredRectangle(Projectile.Center + spinningpoint.RotatedBy(num27), new Vector2(30f, 30f)).Intersects(targetHitbox))
						return true;
				}
				return null;
			}
			public override bool PreDraw(ref Color lightColor) {
				lightColor = Color.White * Projectile.Opacity;
				Texture2D value83 = TextureAssets.Extra[35].Value;
				Microsoft.Xna.Framework.Rectangle rectangle21 = value83.Frame(1, 3);
				Vector2 origin21 = rectangle21.Size() / 2f;
				Vector2 vector80 = new Vector2(0f, -720f).RotatedBy(Projectile.velocity.ToRotation());
				float num324 = Projectile.ai[0] % 45f / 45f;
				Vector2 spinningpoint7 = vector80 * num324;
				for (int num325 = 0; num325 < 6; num325++) {
					float num326 = num325 * ((float)Math.PI * 2f) / 6f;
					Vector2 vector81 = Projectile.Center + spinningpoint7.RotatedBy(num326);
					Main.EntitySpriteDraw(value83, vector81 - Main.screenPosition, rectangle21, lightColor, num326 + Projectile.velocity.ToRotation() + (float)Math.PI, origin21, Projectile.scale, SpriteEffects.None);
					rectangle21.Y += rectangle21.Height;
					if (rectangle21.Y >= value83.Height)
						rectangle21.Y = 0;
				}
				return base.PreDraw(ref lightColor);
			}
		}
	}

	public class Lunatics_Rune_Duplicates_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Item_" + ItemID.NebulaPickup3;
		public override void Update(Player player, ref int buffIndex) {
			player.EnableShadow<Lunatic_Shadow>();
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.lunaticDuplicates = true;
			originPlayer.lunaticDuplicateOpacity++;
			Min(ref originPlayer.lunaticDuplicateOpacity, player.buffTime[buffIndex]);
		}
	}
	public class Lunatic_Shadow : ShadowType {
		public static float Offset => 64;
		public override IEnumerable<ShadowType> SortAbove() => [PartialEffects];
		public override IEnumerable<ShadowData> GetShadowData(Player player, ShadowData from) {
			Vector2 position = from.Position;
			//from.Shadow = 0.5f;
			from.Position = position + Vector2.UnitX * Offset;
			yield return from;
			from.Position = position - Vector2.UnitX * Offset;
			yield return from;
		}
		public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
			float opacity = Math.Min(drawInfo.drawPlayer.OriginPlayer().lunaticDuplicateOpacity / 30f, 1) * 0.5f;

			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData drawData = drawInfo.DrawDataCache[i];
				drawData.color *= opacity;
				drawInfo.DrawDataCache[i] = drawData;
			}
		}
	}
}
