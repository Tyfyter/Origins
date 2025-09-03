using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Weapons.Demolitionist;
using Origins.NPCs;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class The_Bird : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Sword",
			"DeveloperItem"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenSword);
			Item.useStyle = ItemUseStyleID.Swing;
			Item.damage = 99;
			Item.crit = 21;
			Item.useAnimation = Item.useTime = 20;
			Item.rare = ItemRarityID.Cyan;
			Item.knockBack = 16;
			Item.shoot = ModContent.ProjectileType<The_Bird_Swing>();
			Item.shootSpeed = 1;
			Item.channel = true;
			Item.UseSound = SoundID.Item82.WithPitchRange(0.8f, 1f);
			Item.scale = 1f;
		}
		public override bool? CanHitNPC(Player player, NPC target) => player.altFunctionUse == 2 ? null : false;
		public override bool CanShoot(Player player) => player.altFunctionUse != 2;
		public override bool AltFunctionUse(Player player) => true;
		public override void UseItemFrame(Player player) {
			int frame = player.bodyFrame.Y / player.bodyFrame.Height;
			float progress = player.itemAnimation / (float)player.itemAnimationMax;
			if (player.altFunctionUse == 2) {
				progress *= progress * progress;
				int oldFrame = frame;
				frame = 2 + (int)(progress * 4f);
				if (frame == 5) frame = 0;
				switch (frame) {
					case 0:
					player.itemLocation = player.MountedCenter + new Vector2(-4 * player.direction, 4);
					break;
					case 4:
					if (oldFrame != frame) SoundEngine.PlaySound(player.HeldItem.UseSound, player.MountedCenter);
					player.itemLocation = player.MountedCenter + new Vector2(4 * player.direction, 8);
					break;
					case 3:
					player.itemLocation = player.MountedCenter + new Vector2(6 * player.direction, 8);
					break;
					case 2:
					player.itemLocation = player.MountedCenter + new Vector2(6 * player.direction, -8);
					break;
				}
				player.itemRotation = (progress * 3 - 0.5f) * player.direction;
			}
			player.bodyFrame.Y = player.bodyFrame.Height * frame;
		}
		public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox) {
			if (player.altFunctionUse == 2) {
				switch (player.bodyFrame.Y / player.bodyFrame.Height) {
					case 0:
					hitbox.Y += 48;
					break;
					case 2 or 3 or 4:
					hitbox = player.Hitbox;
					hitbox.X += player.direction * 24;
					hitbox.Inflate(8, (80 - player.height) / 2);
					break;
				}
			}
		}
		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
			PreHitNPC(target, ref modifiers);
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			hit.Knockback = GetBirdKnockback(target, hit, Item.knockBack);
			if (hit.Knockback != 0) {
				Vector2 knockback = hit.GetKnockbackFromHit(false, false, yMult: 1);
				target.velocity = new(knockback.Y * player.direction * 0.25f, -knockback.X);
				BirdUp(target, hit.SourceDamage);
			}
			target.SyncCustomKnockback();
		}
		public static void PreHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
			target.GetGlobalNPC<OriginGlobalNPC>().birdedTime = 1;
		}
		public static float GetBirdKnockback(NPC target, NPC.HitInfo hit, float baseKnockback) {
			baseKnockback *= 0.5f;
			return Math.Min((!target.GetGlobalNPC<OriginGlobalNPC>().deadBird || hit.Knockback > baseKnockback) ? hit.Knockback : baseKnockback, 16);
		}
		public static void BirdUp(NPC target, int sourceDamage) {
			OriginGlobalNPC global = target.GetGlobalNPC<OriginGlobalNPC>();
			global.birdedTime = (target.boss || NPCID.Sets.ShouldBeCountedAsBoss[target.type]) ? 30 : 90;
			global.birdedDamage = sourceDamage;
		}
	}
	public class The_Bird_Swing : ModProjectile, IDrawOverArmProjectile, ILoadExtraTextures {
		public static int PerfectFrames => 15;
		static AutoLoadingAsset<Texture2D> frontTexture = typeof(The_Bird_Swing).GetDefaultTMLName() + "_Front";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
			Main.projFrames[Type] = 5;
		}
		public static List<int> reflectors = [];
		public static List<int> nextReflectors = [];
		public override void Unload() {
			reflectors = null;
			nextReflectors = null;
		}
		public override void SetDefaults() {
			Projectile.friendly = false;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.timeLeft = 18;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
		}
		public override bool ShouldUpdatePosition() => true;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) {
				Projectile.Kill();
				return;
			}
			Projectile.Center = player.MountedCenter;
			if (player.channel) {
				if (Projectile.owner == Main.myPlayer) {
					Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(default);
				}
				Projectile.timeLeft = player.itemAnimationMax;
				Projectile.hide = true;
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, Projectile.direction * (MathHelper.PiOver2 + Projectile.velocity.Y * 0.5f - 0.2f));
				player.itemLocation = player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation);
				player.itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver4 * Projectile.direction * 3;
				if (++Projectile.localAI[0] == player.itemAnimationMax) {
					SoundEngine.PlaySound(SoundID.Item29.WithVolume(0.5f).WithPitchRange(1f, 1.2f), Projectile.Center);
					ParticleOrchestrator.RequestParticleSpawn(false,
						ParticleOrchestraType.SilverBulletSparkle,
						new() {
							PositionInWorld = player.itemLocation + Vector2.UnitY.RotatedBy(player.compositeFrontArm.rotation) * 28,
							MovementVector = player.velocity,
							IndexOfPlayerWhoInvokedThis = (byte)Projectile.owner
						}
					);
				}
			} else {
				if (Projectile.hide) SoundEngine.PlaySound(player.HeldItem.UseSound, Projectile.Center);
				Projectile.hide = false;
				Projectile.friendly = true;
				float rotation = Projectile.velocity.ToRotation();
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, rotation - MathHelper.PiOver2);
				player.itemLocation = default;
				player.itemRotation = rotation + MathHelper.PiOver4 - MathHelper.PiOver4 * (Projectile.direction - 1);
				Projectile.rotation = rotation;
				Projectile.frame = (++Projectile.frameCounter) / 3;
				if (Projectile.frameCounter < 3) {
					player.itemLocation = player.GetFrontHandPosition(player.compositeFrontArm.stretch, player.compositeFrontArm.rotation);
				} else {
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2);
					Projectile.friendly = false;
				}
				Projectile.direction = Math.Sign(Projectile.velocity.X);
				player.OriginPlayer().heldProjOverArm = this;
				if (Projectile.friendly) {
					nextReflectors.Add(Projectile.whoAmI);
				}
			}
			player.SetDummyItemTime(2);
			player.direction = Projectile.direction;
			Projectile.Center += Projectile.velocity * 32;
			if (Projectile.direction == -1) {
				Projectile.spriteDirection = -1;
				//Projectile.rotation += MathHelper.Pi;
			} else {
				Projectile.spriteDirection = 1;
			}
		}
		public bool forcedCrit = false;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			The_Bird.PreHitNPC(target, ref modifiers);
			Player player = Main.player[Projectile.owner];
			forcedCrit = false;
			if (Projectile.localAI[0] >= player.itemAnimationMax && Projectile.localAI[0] < player.itemAnimationMax + PerfectFrames) {
				modifiers.SetCrit();
				if (!CritType.ModEnabled) modifiers.CritDamage *= 1 + (Projectile.CritChance / 100f);
				modifiers.HideCombatText();
				forcedCrit = true;
			}
		}
		public override bool? CanCutTiles() => Projectile.friendly;
		public override bool CanHitPvp(Player target) => Projectile.frameCounter > 0;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			float knockback = The_Bird.GetBirdKnockback(target, hit, Projectile.knockBack);
			if (knockback != 0) {
				target.velocity = knockback * Projectile.velocity * 1.25f * (hit.Crit ? 1.4f : 1f);
				The_Bird.BirdUp(target, hit.SourceDamage);
			}
			if (forcedCrit) OriginExtensions.DoCustomCombatText(target.Hitbox, Color.Silver, damageDone, true);
			target.SyncCustomKnockback();
		}
		public override bool PreDraw(ref Color lightColor) {
			DrawData data = GetDrawData();
			data.texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(data);
			return false;
		}
		public DrawData GetDrawData() => new(
			frontTexture,
			Projectile.Center - Main.screenPosition,
			frontTexture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
			Color.Lerp(Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Color.Blue, 0.5f),
			Projectile.rotation,
			new(62, 17 + Projectile.spriteDirection * 6),
			Projectile.scale,
			Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically
		);

		public void LoadTextures() {
			frontTexture.LoadAsset();
		}
	}
	public class The_Bird_Crit_Type : CritType<The_Bird> {
		public override LocalizedText Description => Language.GetOrRegister($"Mods.Origins.CritType.PerfectTiming");
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => (projectile?.ModProjectile as The_Bird_Swing)?.forcedCrit ?? false;
		public override float CritMultiplier(Player player, Item item) => 2f;
	}
}
