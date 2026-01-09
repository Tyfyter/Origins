using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Core;
using Origins.Layers;
using Origins.Projectiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.Utilities;

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
			Item.rare = ItemRarityID.Red;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) => LunaticsRuneAttack.ModifyWeaponDamage(player, ref damage);
		public static bool CheckMana(Player player, Item item, float multiplier, bool pay = true) {
			if (item is null) return false;
			float reduce = player.manaCost;
			float mult = 1;

			CombinedHooks.ModifyManaCost(player, item, ref reduce, ref mult);
			int mana = Main.rand.RandomRound(item.mana * reduce * mult * multiplier);
			int actualCost = mana;
			Max(ref mana, 1);
			if (!pay) return player.statMana >= mana;

			if (player.statMana < mana) {
				CombinedHooks.OnMissingMana(player, item, mana);
				if (player.statMana < mana && player.manaFlower)
					player.QuickMana();
			}

			if (player.statMana < mana) return false;
			CombinedHooks.OnConsumeMana(player, item, actualCost);
			player.manaRegenDelay = player.maxRegenDelay;
			player.statMana -= actualCost;
			return true;
		}
		public static void DoRitualTeleport(Player player, bool starting, bool duplicates) {
			Vector2 oldLeft = default;
			Vector2 oldRight = default;
			if (duplicates) {
				oldLeft = Lunatic_Shadow.GetPosition(player.position, -1, !starting);
				oldRight = Lunatic_Shadow.GetPosition(player.position, 1, !starting);
			}
			if (starting) {
				player.RemoveAllGrapplingHooks();
				player.Teleport(player.position - Vector2.UnitY * Cultist_Ritual_Layer.Offset, TeleportationStyleID.QueenSlimeHook);
			}
			if (duplicates) {
				Rectangle effectBox = player.Hitbox;
				effectBox.X = (int)oldLeft.X;
				effectBox.Y = (int)oldLeft.Y;
				Main.TeleportEffect(effectBox, TeleportationStyleID.QueenSlimeHook, otherPosition: Lunatic_Shadow.GetPosition(player.position, -1, starting));
				effectBox.X = (int)oldRight.X;
				effectBox.Y = (int)oldRight.Y;
				Main.TeleportEffect(effectBox, TeleportationStyleID.QueenSlimeHook, otherPosition: Lunatic_Shadow.GetPosition(player.position, 1, starting));
			}
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.lunaticsRune = Item;
			ref int charge = ref originPlayer.lunaticsRuneCharge;
			if (player.SyncedKeybinds().LunaticsRune.Current && (charge >= ChargeThreshold || CheckMana(player, Item, 1f / ChargeThreshold))) {
				if (charge == 0) DoRitualTeleport(player, true, originPlayer.lunaticDuplicates);
				originPlayer.lunaticsRuneRotation += 0.02f;
				charge.Warmup(ChargeThreshold);
				float moveMult = 0;// 1 - float.Pow(charge / (float)ChargeThreshold, 2);
				player.velocity *= moveMult * moveMult;
				player.gravity *= moveMult;
				originPlayer.moveSpeedMult *= moveMult * moveMult;
				if (player.velocity.Y == 0) player.velocity.Y = float.Epsilon;
			} else if (charge > 0) {
				DoRitualTeleport(player, false, originPlayer.lunaticDuplicates);
				if (player.whoAmI == Main.myPlayer) {
					if (charge >= ChargeThreshold) {
						for (int i = 0; i < player.buffType.Length; i++) {
							Max(ref player.buffTime[i], BuffOption.RitualRefreshTime[player.buffType[i]]);
						}
						RangeRandom random = new(Main.rand, 0, options.Count);
						for (int i = 0; i < options.Count; i++) {
							random.Multiply(i, i + 1, options[i].GetWeight(player));
						}
						if (random.AnyWeight) {
							options[random.Get()].Trigger(player);
						} else {
							for (int i = 0; i < player.buffType.Length; i++) {
								Max(ref player.buffTime[i], BuffOption.RitualRefreshTime[player.buffType[i]] * 2);
							}
						}
					} else {
						player.wingTime = 0;
						player.AddBuff(ModContent.BuffType<Mana_Buffer_Debuff>(), 5 * 60);
					}
				}
				charge = 0;
			}
		}
		public class Attacks : BuffOption {
			public override int BuffType => ModContent.BuffType<Lunatics_Rune_Attacks_Buff>();
			public override int BuffTime => 30 * 60;
		}
		public class Summon_Dragon : BuffOption {
			public override int BuffType => ModContent.BuffType<Lunatics_Rune_Dragon_Buff>();
			public override int BuffTime => 30 * 60;
			public override void Trigger(Player player) {
				base.Trigger(player);
				Item item = player.OriginPlayer().lunaticsRune;
				int damage = player.GetWeaponDamage(item);
				float knockback = player.GetWeaponKnockback(item);
				bool hasDragon = player.ownedProjectileCounts[ModContent.ProjectileType<Phantasm_Dragon_Head>()] > 0;
				Vector2 position = player.Center + Vector2.UnitY * Cultist_Ritual_Layer.Offset;
				if (!hasDragon) {
					player.SpawnProjectile(null,
						position,
						default,
						ModContent.ProjectileType<Phantasm_Dragon_Head>(),
						damage,
						knockback
					).originalDamage = item.damage;
					for (int i = 0; i < 6; i++) {
						player.SpawnProjectile(null,
							position,
							default,
							ModContent.ProjectileType<Phantasm_Dragon_Body>(),
							damage,
							knockback
						).originalDamage = item.damage;
					}
					player.SpawnProjectile(null,
						position,
						default,
						ModContent.ProjectileType<Phantasm_Dragon_Tail>(),
						damage,
						knockback
					).originalDamage = item.damage;
				} else {
					player.SpawnProjectile(null,
						position,
						default,
						ModContent.ProjectileType<Phantasm_Dragon_Body>(),
						damage,
						knockback
					).originalDamage = item.damage;
				}
			}
		}
		public class Duplicates : BuffOption {
			public override int BuffType => ModContent.BuffType<Lunatics_Rune_Duplicates_Buff>();
			public override int BuffTime => 24 * 60;
		}
		public class Healing : BuffOption {
			public override int BuffType => ModContent.BuffType<Lunatics_Rune_Regen_Buff>();
			public override int BuffTime => 24 * 60;
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
		public override string Texture => "Origins/Buffs/" + Name;
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
	public class Fireball_Attack : LunaticsRuneAttack {
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
							dir,
							ModContent.ProjectileType<Cultist_Fireball_Shadow>(),
							player.GetWeaponDamage(item),
							player.GetWeaponKnockback(item)
						);
					} else {
						player.SpawnProjectile(player.GetSource_Accessory(item),
							player.MountedCenter,
							dir,
							ModContent.ProjectileType<Cultist_Fireball>(),
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
		public class Cultist_Fireball : ModProjectile {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossFireBall}";
			public virtual int FireDustType => DustID.Torch;
			public virtual Vector3 LightColor => new(1.1f, 0.9f, 0.4f);
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 4;
			}
			public override void SetDefaults() {
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
					Entity targetEntity;
					if (currentTarget >= 300) {
						targetEntity = Main.npc[currentTarget - 300];
					} else {
						targetEntity = Main.player[currentTarget];
					}
					if (!targetEntity.active) {
						Projectile.ai[0] = -1;
						break;
					}
					if (Projectile.Center.IsWithin(targetEntity.Center, 20f)) {
						Projectile.Kill();
						return;
					}

					Projectile.velocity = moveDir.AngleLerp((targetEntity.Center - Projectile.Center).ToRotation(), 0.08f).ToRotationVector2() * Projectile.velocity.Length();
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
		public class Cultist_Fireball_Shadow : Cultist_Fireball {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossFireBallClone}";
			public override int FireDustType => DustID.Shadowflame;
			public override Vector3 LightColor => new(0.2f, 0.1f, 0.6f);
		}
	}
	public class Ice_Mist_Attack : LunaticsRuneAttack {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossIceMist}";
		public override void ProcessAttack(Player player) {
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - MathHelper.PiOver2);
			player.direction = Math.Sign(float.Cos(player.itemRotation));
		}
		public override bool CanStartAttack(Player player, bool justChecking = false) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Cultist_Ice_Mist>()] > 0) return false;
			return Lunatics_Rune.CheckMana(player, player.OriginPlayer().lunaticsRune, 0.30f, !justChecking);
		}
		public override void StartAttack(Player player) {
			Item item = player.OriginPlayer().lunaticsRune;
			ForPlayerAndShadows(player, player => {
				Item item = player.OriginPlayer().lunaticsRune;
				Vector2 dir = (Main.MouseWorld - player.MountedCenter).Normalized(out _);
				if (shadowAttacking) {
					List<Vector2> pos = Main.rand.PoissonDiskSampling(new Rectangle(-10, -10, 20, 20), 5f);
					for (int i = 0; i < 6 && i < pos.Count; i++) {
						player.SpawnProjectile(player.GetSource_Accessory(item),
							player.MountedCenter,
							dir * 8 + pos[i] * 0.1f,
							ModContent.ProjectileType<Cultist_Ice_Shard>(),
							player.GetWeaponDamage(item),
							player.GetWeaponKnockback(item)
						);
					}
				} else {
					player.SpawnProjectile(player.GetSource_Accessory(item),
						player.MountedCenter,
						dir * 4,
						ModContent.ProjectileType<Cultist_Ice_Mist>(),
						player.GetWeaponDamage(item),
						player.GetWeaponKnockback(item)
					);
					player.itemRotation = dir.ToRotation();
				}
			});
			player.SetItemAnimation(20);
		}
		public class Cultist_Ice_Mist : ModProjectile {
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
				if (Projectile.localAI[1].TrySet(1)) {
					SoundEngine.PlaySound(SoundID.Item120, Projectile.position);
				}

				Projectile.ai[0]++;
				if (Projectile.ai[0] >= 150f) {
					Projectile.Kill();
					return;
				}
				if (Projectile.ai[0] >= 130f) Projectile.alpha += 10;
				else Projectile.alpha -= 10;
				Max(ref Projectile.alpha, 0);
				Min(ref Projectile.alpha, 255);

				const int HalfSpriteWidth = 92 / 2;
				const int HalfSpriteHeight = 102 / 2;

				int HalfProjWidth = Projectile.width / 2;
				int HalfProjHeight = Projectile.height / 2;

				// Vanilla configuration for "hitbox in middle of sprite"
				DrawOriginOffsetX = 0;
				DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
				DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);

				Projectile.rotation += MathHelper.Pi / 30f;
				Lighting.AddLight(Projectile.Center, 0.3f, 0.75f, 0.9f);
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				Vector2 forward = new Vector2(0f, -720f).RotatedBy(Projectile.velocity.ToRotation());
				float offsetLength = Projectile.ai[0] % 45f / 45f;
				Vector2 offset = forward * offsetLength;
				for (int i = 0; i < 6; i++) {
					float rotation = i * MathHelper.TwoPi / 6f;
					if (Utils.CenteredRectangle(Projectile.Center + offset.RotatedBy(rotation), new Vector2(30f, 30f)).Intersects(targetHitbox))
						return true;
				}
				return null;
			}
			public override bool PreDraw(ref Color lightColor) {
				lightColor = Color.White * Projectile.Opacity;
				Texture2D texture = TextureAssets.Extra[ExtrasID.CultistIceshard].Value;
				Rectangle frame = texture.Frame(1, 3);
				Vector2 origin = frame.Size() / 2f;
				Vector2 forward = new Vector2(0f, -720f).RotatedBy(Projectile.velocity.ToRotation());
				float offsetLength = Projectile.ai[0] % 45f / 45f;
				Vector2 baseOffset = forward * offsetLength;
				for (int i = 0; i < 6; i++) {
					float rotation = i * MathHelper.TwoPi / 6f;
					Vector2 position = Projectile.Center + baseOffset.RotatedBy(rotation);
					Main.EntitySpriteDraw(
						texture,
						position - Main.screenPosition,
						frame,
						lightColor,
						rotation + Projectile.velocity.ToRotation() + MathHelper.Pi,
						origin,
						Projectile.scale,
						SpriteEffects.None
					);
					frame.Y += frame.Height;
					if (frame.Y >= texture.Height)
						frame.Y = 0;
				}
				return base.PreDraw(ref lightColor);
			}
		}
		public class Cultist_Ice_Shard : ModProjectile {
			public override string Texture => $"Terraria/Images/Extra_{ExtrasID.CultistIceshard}";
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 3;
			}
			public override void SetDefaults() {
				Projectile.width = 30;
				Projectile.height = 30;
				Projectile.friendly = true;
				Projectile.tileCollide = false;
				Projectile.penetrate = 1;
				Projectile.extraUpdates = 1;
				Projectile.scale = 0.85f;
				Projectile.ignoreWater = true;
				Projectile.frame = Main.rand.Next(Main.projFrames[Type]);
			}
			public override void AI() {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

				const int HalfSpriteWidth = 24 / 2;
				const int HalfSpriteHeight = 74 / 2;

				int HalfProjWidth = Projectile.width / 2;
				int HalfProjHeight = Projectile.height / 2;

				// Vanilla configuration for "hitbox in middle of sprite"
				DrawOriginOffsetX = 0;
				DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
				DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			}
			public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;
		}
	}
	public class Lightning_Orb_Attack : LunaticsRuneAttack {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossLightningOrb}";
		int frameCounter;
		int frame;
		public override Rectangle Frame {
			get {
				if (frameCounter.CycleUp(6)) frame.CycleUp(4);
				return texture.Frame(verticalFrames: 4, frameY: frame);
			}
		}
		public override void ProcessAttack(Player player) {
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - MathHelper.PiOver2);
			player.direction = Math.Sign(float.Cos(player.itemRotation));
		}
		public override bool CanStartAttack(Player player, bool justChecking = false) {
			return Lunatics_Rune.CheckMana(player, player.OriginPlayer().lunaticsRune, 0.10f, !justChecking);
		}
		public override void StartAttack(Player player) {
			player.SetItemAnimation(30);
			ForPlayerAndShadows(player, player => {
				Item item = player.OriginPlayer().lunaticsRune;
				Vector2 dir = (Main.MouseWorld - player.MountedCenter).Normalized(out _);
				if (shadowAttacking) {
					player.SpawnProjectile(player.GetSource_Accessory(item),
						player.MountedCenter,
						dir,
						ModContent.ProjectileType<Cultist_Lightning>(),
						player.GetWeaponDamage(item),
						player.GetWeaponKnockback(item)
					);
				} else {
					player.SpawnProjectile(player.GetSource_Accessory(item),
						player.Top + new Vector2(float.CopySign(32f, dir.X), -80),
						Vector2.Zero,
						ModContent.ProjectileType<Cultist_Lightning_Orb>(),
						player.GetWeaponDamage(item),
						player.GetWeaponKnockback(item)
					);
					player.itemRotation = float.CopySign(0.1f, dir.X) - MathHelper.PiOver2;
				}
			});
		}
		public class Cultist_Lightning_Orb : ModProjectile {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossLightningOrb}";
			public override void SetStaticDefaults() {
				Main.projFrames[Type] = 4;
			}
			public override void SetDefaults() {
				Projectile.width = 80;
				Projectile.height = 80;
				Projectile.friendly = true;
				Projectile.alpha = 255;
				Projectile.ignoreWater = true;
				Projectile.tileCollide = false;
			}
			public override void AI() {
				bool dieNow = Projectile.ai[0] == 5;
				if (dieNow) Projectile.alpha.Warmup(255, 9);
				else Projectile.alpha.Cooldown(0, 9);
				if (Projectile.ai[1].CycleDown(30) && Projectile.IsLocallyOwned()) {
					if (dieNow) {
						Projectile.Kill();
						return;
					}
					Projectile.ai[0]++;
					Vector2 dir = (Main.MouseWorld - Projectile.Center).Normalized(out _);
					Projectile.SpawnProjectile(null,
						Projectile.Center,
						dir,
						ModContent.ProjectileType<Cultist_Lightning>(),
						Projectile.damage,
						Projectile.knockBack
					);
				}
				if (Projectile.frameCounter.CycleUp(6)) Projectile.frame.CycleUp(Main.projFrames[Type]);

				const int HalfSpriteWidth = 100 / 2;
				const int HalfSpriteHeight = 100 / 2;

				int HalfProjWidth = Projectile.width / 2;
				int HalfProjHeight = Projectile.height / 2;

				// Vanilla configuration for "hitbox in middle of sprite"
				DrawOriginOffsetX = 0;
				DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
				DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			}
		}
		public class Cultist_Lightning : ModProjectile {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CultistBossLightningOrbArc}";

			public override void SetDefaults() {
				Projectile.width = Projectile.height = 0;
				Projectile.alpha = 255;
				Projectile.ignoreWater = true;
				Projectile.tileCollide = false;
				Projectile.DamageType = DamageClass.Magic;
				Projectile.friendly = true;
				Projectile.penetrate = -1;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.timeLeft = 45;
				Projectile.localNPCHitCooldown = 10;
			}
			public override bool ShouldUpdatePosition() => false;
			public override void OnSpawn(IEntitySource source) {
				(Projectile.ai[1], Projectile.ai[2]) = Projectile.position;
				(Projectile.localAI[1], Projectile.localAI[2]) = Projectile.velocity;
			}
			public override void AI() {
				const float step_size = 80;
				if (positionCache.Count <= 0) positionCache.Add(Projectile.position);
				if (!Projectile.ai[0].CycleDown(2)) return;
				if (Projectile.velocity != default) {
					Projectile.localAI[0]++;
					Vector2 targetPos = Projectile.Center;
					bool foundTarget = false;
					Vector2 testPos;
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC target = Main.npc[i];
						if (target.CanBeChasedBy() && Projectile.localNPCImmunity[target.whoAmI] == 0) {
							testPos = Projectile.Center.Clamp(target.Hitbox);
							Vector2 difference = testPos - Projectile.Center;
							if (difference == default) continue;
							float distance = difference.Length();
							bool closest = Vector2.Distance(Projectile.Center, targetPos) > distance;
							bool inRange = distance < 96 && (difference.SafeNormalize(Vector2.Zero) * Projectile.velocity.SafeNormalize(Vector2.Zero)).Length() > 0.1f;//magRange;
							if ((!foundTarget || closest) && inRange) {
								targetPos = testPos;
								foundTarget = true;
							}
						}
					}
					if (!foundTarget) {
						float expectedDist = step_size * Projectile.localAI[0] * 0.85f;
						targetPos = new(Projectile.ai[1] + Projectile.localAI[1] * expectedDist, Projectile.ai[2] + Projectile.localAI[2] * expectedDist);
					}
					Projectile.oldVelocity = Projectile.velocity;
					Projectile.velocity = (targetPos - Projectile.Center).Normalized(out _) * Projectile.velocity.Length();
					Projectile.velocity = Projectile.velocity.RotatedByRandom(0.5f);

					float maxDist = CollisionExt.Raymarch(Projectile.position, Projectile.velocity, step_size);
					Projectile.position += Projectile.velocity * maxDist;
					_ = GetHitbox();
					positionCache.Add(Projectile.position);
					if (maxDist != step_size) Projectile.velocity = default;
				}
				randSeed = Main.rand.Next(ushort.MaxValue);
			}
			readonly List<Vector2> positionCache = [];
			readonly List<(Vector2 start, Vector2 end)> polygonCache = [];
			float lastHitboxUpdate = -1;
			int randSeed;
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				return CollisionExtensions.PolygonIntersectsRect(GetHitbox(), targetHitbox);
			}

			public (Vector2 start, Vector2 end)[] GetHitbox() {
				(Vector2 start, Vector2 end) GetHead(Vector2 pos) {
					Vector2 offset = (Projectile.velocity + Projectile.oldVelocity).Normalized(out _).RotatedBy(MathHelper.PiOver2) * 12;
					return (pos - offset, pos + offset);
				}
				void Connect((Vector2 start, Vector2 end) a, (Vector2 start, Vector2 end) b) {
					if (b == a) return;
					polygonCache.Add((a.start, b.start));
					polygonCache.Add((a.end, b.end));
				}
				if (polygonCache.Count <= 0) {
					lastHitboxUpdate = Projectile.localAI[0];
					(Vector2 start, Vector2 end) a = GetHead(new(Projectile.ai[1], Projectile.ai[2]));
					(Vector2 start, Vector2 end) head = GetHead(Projectile.position);
					polygonCache.Add(a);
					Connect(a, head);
					polygonCache.Add(head);
				}
				if (lastHitboxUpdate.TrySet(Projectile.localAI[0])) {
					(Vector2 start, Vector2 end) a = polygonCache[^1];
					polygonCache.RemoveAt(polygonCache.Count - 1);
					(Vector2 start, Vector2 end) head = GetHead(Projectile.position);
					Connect(a, head);
					polygonCache.Add(head);
				}
				return polygonCache.ToArray();
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
				target.AddBuff(Electrified_Debuff.ID, 240);
			}
			private static readonly VertexStrip _vertexStrip = new();
			public override bool PreDraw(ref Color lightColor) {
				MiscShaderData miscShaderData = GameShaders.Misc["Origins:Framed"];
				float uTime = (float)Main.timeForVisualEffects / 44;
				Vector2[] pos = positionCache.ToArray();
				float[] rot = new float[pos.Length];
				for (int i = 1; i < rot.Length - 1; i++) {
					float diffPrev = (pos[i] - pos[i - 1]).ToRotation();
					float diffNext = (pos[i + 1] - pos[i]).ToRotation();
					rot[i] = Utils.AngleLerp(diffPrev, diffNext, 0.5f);
					if (i == 1) rot[i - 1] = diffPrev;
					if (i == rot.Length - 2) rot[i + 1] = diffNext;
				}

				FastRandom rand = new(randSeed);
				Asset<Texture2D> texture = TextureAssets.Extra[ExtrasID.MagicMissileTrailShape];
				miscShaderData.UseImage0(texture);
				//miscShaderData.UseShaderSpecificData(new Vector4(Main.rand.NextFloat(1), 0, 1, 1));
				miscShaderData.Shader.Parameters["uAlphaMatrix0"]?.SetValue(new Vector4(1, 1, 1, 0));
				miscShaderData.Shader.Parameters["uSourceRect0"]?.SetValue(new Vector4(rand.NextFloat(), 0, 1, 1));
				miscShaderData.Apply();
				_vertexStrip.PrepareStrip(pos, rot, _ => new Color(0.1f, 0.75f, 1f, 0.5f), _ => 32, -Main.screenPosition, pos.Length, includeBacksides: true);
				_vertexStrip.DrawTrail();
				for (int i = 0; i < pos.Length / 2; i++) {
					pos[i] = pos[i] + GeometryUtils.Vec2FromPolar(rand.NextFloat() * 12 - 6, rot[i] + MathHelper.PiOver2);
				}
				_vertexStrip.PrepareStrip(pos, rot, _ => new Color(0.3f, 0.85f, 1f, 0.2f), _ => 24, -Main.screenPosition, pos.Length, includeBacksides: true);
				_vertexStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				return false;
			}
		}
	}
	public class Teleport_Attack : LunaticsRuneAttack {
		public override string Texture => $"Terraria/Images/Extra_{ExtrasID.PotionOfReturnGateIn}";
		int frameCounter;
		int frame;
		public override Rectangle Frame {
			get {
				if (frameCounter.CycleUp(6)) frame.CycleUp(8);
				return texture.Frame(verticalFrames: 8, frameY: frame);
			}
		}
		public override bool CanStartAttack(Player player, bool justChecking = false) {
			Rectangle hitbox = player.Hitbox;
			Vector2 mouseWorld = Main.MouseWorld;
			if (justChecking) {
				mouseWorld = Vector2.Transform(Main.MouseScreen * Main.UIScale, Matrix.Invert(Main.GameViewMatrix.ZoomMatrix)) + Main.screenPosition;
			}
			hitbox.Location = mouseWorld.ToPoint();
			hitbox.X -= player.width / 2;
			hitbox.Y -= player.height / 2;
			if (hitbox.OverlapsAnyTiles()) return false;
			return Lunatics_Rune.CheckMana(player, player.OriginPlayer().lunaticsRune, 0.20f, !justChecking);
		}
		public override void ProcessAttack(Player player) {
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - MathHelper.PiOver2);
			player.direction = Math.Sign(float.Cos(player.itemRotation));
		}
		public override void StartAttack(Player player) {
			Vector2 diff = Main.MouseWorld - player.MountedCenter;
			if (Math.Sign(player.velocity.X) != Math.Sign(diff.X)) player.velocity.X = 0;
			if (Math.Sign(player.velocity.Y) != Math.Sign(diff.Y)) player.velocity.Y = 0;
			player.itemRotation = diff.ToRotation();
			player.SetItemAnimation(15);
			player.Teleport(Main.MouseWorld - player.Size * 0.5f, TeleportationStyleID.QueenSlimeHook);
		}
	}
	public class Lunatics_Rune_Dragon_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/" + Name;
		public override void SetStaticDefaults() => Main.buffNoSave[Type] = true;
		public override void Update(Player player, ref int buffIndex) => player.OriginPlayer().lunaticDragon = true;
	}
	[ReinitializeDuringResizeArrays]
	public abstract class Phantasm_Dragon_Base : WormMinion {
		static readonly bool[] SegmentTypes = ProjectileID.Sets.Factory.CreateBoolSet();
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.LunaticCultistPet}";
		protected abstract int FrameNum { get; }
		public override float ChildDistance => 14;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= 1f;// use this to adjust damage
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			SegmentTypes[Type] = true;
			Main.projFrames[Type] = 4;
			Main.projPet[Projectile.type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.penetrate = -1;
			Projectile.timeLeft *= 5;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			Projectile.ContinuouslyUpdateDamageStats = true;
			Projectile.frame = FrameNum;
		}
		public override bool MinionContactDamage() => true;
		public override ref bool HasBuff(Player player) => ref player.OriginPlayer().lunaticDragon;
		public override bool IsValidParent(Projectile segment) => SegmentTypes[segment.type];
	}
	public class Phantasm_Dragon_Head : Phantasm_Dragon_Base {
		public override BodyPart Part => BodyPart.Head;
		protected override int FrameNum => 0;
		public override bool CanInsert(Projectile parent, Projectile child) => false;
	}
	public class Phantasm_Dragon_Body : Phantasm_Dragon_Base {
		public override BodyPart Part => BodyPart.Body;
		protected override int FrameNum => 1;
		public override bool CanInsert(Projectile parent, Projectile child) => parent.type == ModContent.ProjectileType<Phantasm_Dragon_Head>();
		public override void AI() {
			base.AI();
			Projectile.frame = 1 + ((int)Projectile.localAI[1] % 2);
		}
	}
	public class Phantasm_Dragon_Tail : Phantasm_Dragon_Base {
		public override BodyPart Part => BodyPart.Tail;
		protected override int FrameNum => 3;
		public override bool CanInsert(Projectile parent, Projectile child) => child is null;
	}
	public class Lunatics_Rune_Duplicates_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/" + Name;
		public override void Update(Player player, ref int buffIndex) {
			player.EnableShadow<Lunatic_Shadow>();
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.lunaticDuplicates = true;
			originPlayer.lunaticDuplicateOpacity++;
			Min(ref originPlayer.lunaticDuplicateOpacity, player.buffTime[buffIndex]);
		}
	}
	public class Lunatics_Rune_Regen_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/" + Name;
		public override void Update(Player player, ref int buffIndex) {
			player.lifeRegenCount += 12;
		}
	}
	public class Lunatic_Shadow : ShadowType {
		public static float Offset => 64;
		public override IEnumerable<ShadowType> SortAbove() => [PartialEffects];
		public static Vector2 GetPosition(Vector2 position, int direction, bool ritualActive) {
			if (ritualActive) {
				return position.RotatedBy(MathHelper.TwoPi * direction / 3f, position + Vector2.UnitY * Cultist_Ritual_Layer.Offset);
			} else {
				return position + Vector2.UnitX * Offset * direction;
			}
		}
		public override IEnumerable<ShadowData> GetShadowData(Player player, ShadowData from) {
			Vector2 position = from.Position;
			from.Shadow = 0.25f;
			if (player.OriginPlayer().lunaticsRuneCharge > 0) {
				from.Direction = -1;
				from.Position = GetPosition(position, 1, true);
				yield return from;
				from.Direction = 1;
				from.Position = GetPosition(position, -1, true);
				yield return from;
			} else {
				from.Position = GetPosition(position, 1, false);
				yield return from;
				from.Position = GetPosition(position, -1, false);
				yield return from;
			}
		}
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			drawInfo.colorEyeWhites = Color.Transparent;
			drawInfo.colorEyes = Color.Transparent;
		}
	}
}
