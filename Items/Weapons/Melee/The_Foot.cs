using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Other.Consumables;
using Origins.Projectiles.Weapons;
using System;
using PegasusLib;
using PegasusLib.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Projectiles;
using Origins.CrossMod;
using Terraria.Localization;

namespace Origins.Items.Weapons.Melee {
	public class The_Foot : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Sword"
		];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 217;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.shoot = ModContent.ProjectileType<The_Foot_Smash>();
			Item.shootSpeed = 12;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 12f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item1;
			Item.ArmorPenetration = 0;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool MeleePrefix() => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<The_Foot_Flail>();
			}
		}
		public static void DoHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 600);
		}
		public static void DoSlam(Projectile projectile, Vector2 position, Vector2 direction) {
			Vector2 center = position + projectile.Size * 0.5f;
			Collision.HitTiles(position, direction, projectile.width, projectile.height);
			SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, center);
			SoundEngine.PlaySound(SoundID.Item14, center);

			Main.instance.CameraModifiers.Add(new CameraShakeModifier(
				center, 5f, 3f, 12, 500f, -1f, nameof(Amoebash)
			));
			if (Main.myPlayer == projectile.owner) {
				Projectile.NewProjectile(
					projectile.GetSource_FromAI(),
					center,
					Vector2.Zero,
					ModContent.ProjectileType<The_Foot_Aura>(),
					projectile.damage / 4,
					0
				);
			}
		}
	}
	public class The_Foot_Smash : MeleeSlamProjectile {
		AutoLoadingAsset<Texture2D> glowTexture = typeof(The_Foot).GetDefaultTMLName() + "_Glow";
		public override string Texture => typeof(The_Foot).GetDefaultTMLName();
		public override bool CanHitTiles() => Projectile.rotation * Projectile.ai[1] > -0.85f;
		public override void OnHitTiles(Vector2 position, Vector2 direction) {
			The_Foot.DoSlam(Projectile, position, direction);
		}
		internal static bool forcedCrit = false;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = 0;
			forcedCrit = false;
			if (!target.noTileCollide && float.IsNaN(Projectile.ai[2])) {
				Rectangle hitbox = target.Hitbox;
				Vector2 dir = Projectile.velocity.RotatedBy(Projectile.rotation * Main.player[Projectile.owner].gravDir + Projectile.ai[1] * 2.5f).SafeNormalize(default);
				hitbox.Offset((dir * 8).ToPoint());
				if (hitbox.OverlapsAnyTiles(fallThrough: false)) {
					Collision.HitTiles(hitbox.TopLeft(), dir, hitbox.Width, hitbox.Height);
					modifiers.SetCrit();
					forcedCrit = true;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			The_Foot.DoHitNPC(target, hit, damageDone);
			target.velocity -= target.velocity * target.knockBackResist;
			if (!float.IsNaN(hit.Knockback)) {
				Vector2 dir = Projectile.velocity.RotatedBy(Projectile.rotation);
				if (!forcedCrit) dir += dir.RotatedBy(Projectile.ai[1] * MathHelper.PiOver2);
				target.velocity += dir.SafeNormalize(default) * hit.Knockback;
			}
			target.SyncCustomKnockback();
			forcedCrit = false;
		}
		public override bool PreDraw(ref Color lightColor) {
			DrawData data = GetDrawData(lightColor, new Vector2(10, 39 + 28));
			Main.EntitySpriteDraw(data);
			data.texture = glowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);
			return false;
		}
	}
	public class The_Foot_Flail : ModProjectile {
		public AutoLoadingAsset<Texture2D> ChainTexture = typeof(The_Foot).GetDefaultTMLName() + "_Chain";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.friendly = true;
			Projectile.scale = 0.9f;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.timeLeft = 240;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}
		public override void AI() {
			if (Projectile.timeLeft < 2 || Projectile.ai[1] == 1) {
				Projectile.timeLeft = 2;
				Projectile.tileCollide = false;
				Projectile.ai[1] = 1;
			}
			Player player = Main.player[Projectile.owner];
			Vector2 direction = Projectile.DirectionTo(player.MountedCenter);
			Projectile.rotation += Projectile.ai[0] * 0.3f;
			if (direction.HasNaNs()) return;
			MathUtils.LinearSmoothing(ref Projectile.velocity, direction * (Projectile.ai[1] == 1 ? 8 : 6), Projectile.ai[1] * 0.9f);
			if (Projectile.ai[1] != 1) {
				Projectile.velocity.Y += 0.3f + 0.4f * Projectile.ai[1];
				Projectile.ai[1] = 1 - (1 - Projectile.ai[1]) * 0.98f;
				if (Projectile.ai[1] > 0.5f && Main.myPlayer == Projectile.owner && player.controlUseTile) {
					Projectile.ai[1] = 1;
					Projectile.netUpdate = true;
				}
			}
			if (Projectile.ai[1] > 0.5f) {
				if (Projectile.Hitbox.Intersects(player.Hitbox)) {
					Projectile.penetrate = -1;
					Projectile.Kill();
				}
			}
			player.SetDummyItemTime(2);
			player.itemRotation = (-direction).ToRotation();
			if (Projectile.Center.X < player.MountedCenter.X)
				player.itemRotation += MathHelper.Pi;

			player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			The_Foot.DoHitNPC(target, hit, damageDone);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			//Projectile.velocity = oldVelocity;
			Projectile.velocity *= 0.8f;
			Vector2 slamDir = oldVelocity - Projectile.velocity;
			if (slamDir.LengthSquared() > 4 * 4) {
				The_Foot.DoSlam(Projectile, Projectile.position, slamDir);
			}
			Player player = Main.player[Projectile.owner];
			Vector2 direction = Projectile.DirectionTo(player.MountedCenter);
			float dot = Vector2.Dot(direction, slamDir.SafeNormalize(default));
			if (dot > 0) {
				player.velocity -= dot * slamDir * 0.5f;
			}
			return false;
		}
		public override bool PreDrawExtras() {
			Vector2 chainDrawPosition = Projectile.Center;
			Vector2 vectorFromProjectileToPlayerArms = Main.GetPlayerArmPosition(Projectile).MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
			float rotation = vectorFromProjectileToPlayerArms.ToRotation();
			List<Vector2> chainPositions = GetChainPositions(chainDrawPosition, vectorFromProjectileToPlayerArms);
			for (int i = 0; i < chainPositions.Count; i++) {
				Main.EntitySpriteDraw(
					ChainTexture,
					chainPositions[i] - Main.screenPosition,
					null,
					Lighting.GetColor(chainPositions[i].ToTileCoordinates()),
					rotation,
					new Vector2(6, 3),
					Projectile.scale,
					0,
				0);
			}
			return false;
		}

		List<Vector2> GetChainPositions(Vector2 chainDrawPosition, Vector2 vectorFromProjectileToPlayerArms) {
			const int overlapPixels = 1;
			float chainLength = (12 - (overlapPixels * 2)) * Projectile.scale;
			Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero) * chainLength;
			float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() / chainLength + 1;
			List<Vector2> chainPositions = new();
			while (chainLengthRemainingToDraw > 0f) {
				chainPositions.Add(chainDrawPosition);
				chainDrawPosition += unitVectorFromProjectileToPlayerArms;
				chainLengthRemainingToDraw--;
			}
			return chainPositions;
		}
	}
	public class The_Foot_Aura : ModProjectile {
		public override string Texture => typeof(The_Foot).GetDefaultTMLName() + "_Chain";
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.friendly = true;
			Projectile.scale = 1f;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.width = 120;
			Projectile.height = 120;
			Projectile.timeLeft = 120;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 0;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 20;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 60);
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, 0, 0.4f, 0);
		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteBatchState state = Main.spriteBatch.GetState();
			try {
				Main.spriteBatch.Restart(state, sortMode: SpriteSortMode.Immediate);
				Texture2D texture = TextureAssets.Extra[193].Value;
				DrawData data = new() {
					texture = texture,
					position = Projectile.Center - Main.screenPosition,
					color = Color.Lime * Math.Min(Projectile.timeLeft / 20f, 1),
					rotation = 0f,
					scale = new Vector2(Projectile.scale),
					shader = Crown_Jewel.ShaderID,
					origin = texture.Size() * 0.5f
				};
				GameShaders.Armor.ApplySecondary(Crown_Jewel.ShaderID, null, data);
				data.Draw(Main.spriteBatch);
			} finally {
				Main.spriteBatch.Restart(state);
			}
			return false;
		}
	}
	public class The_Foot_Crit_Type : CritType<The_Foot> {
		public override LocalizedText Description => Language.GetOrRegister($"Mods.Origins.CritType.SlammyHammer");
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => The_Foot_Smash.forcedCrit;
		public override float CritMultiplier(Player player, Item item) => 2f;
	}
}
