using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Felnum_Sword : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Sword
		];
		public override void SetStaticDefaults() {
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Rapier;
			Item.knockBack = 9;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.useTurn = false;
			Item.shoot = ModContent.ProjectileType<Felnum_Sword_Slash>();
			Item.shootSpeed = 12;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool AltFunctionUse(Player player) => true;
		public override float UseSpeedMultiplier(Player player) => player.altFunctionUse == 0 ? 1f : 1.25f;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 0) velocity = new(Math.Sign(velocity.X) * velocity.Length(), 0);
			else velocity = velocity.RotatedByRandom(0.05f);
		}
	}
	public class Felnum_Sword_Slash : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Felnum_Sword";
		float HitboxRotation => Projectile.rotation + MathHelper.PiOver4 * -0.5f * Projectile.ai[1] * Main.player[Projectile.owner].gravDir;
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.noEnchantmentVisuals = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Main.player[Projectile.owner].altFunctionUse == 0) Projectile.ai[1] = Main.player[Projectile.owner].direction;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (Projectile.ai[1] != 0) {
				float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
				Projectile.rotation = MathHelper.Lerp(-2.25f, 1f, swingFactor) * Projectile.ai[1] * player.gravDir;
				float realRotation = Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation();
				Projectile.Center = player.MountedCenter - Projectile.velocity;
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
				Projectile.Center = player.GetCompositeArmPosition(false);
			} else {
				float swingFactor = Math.Abs((player.itemTime / (float)player.itemTimeMax) * 1.5f - 0.5f);
				Projectile.Center = player.MountedCenter + Projectile.velocity * (1 - swingFactor * swingFactor * swingFactor - 0.5f);
			}
			player.heldProj = Projectile.whoAmI;
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates;

			Vector2 vel = (Projectile.velocity.RotatedBy(HitboxRotation) / 12f) * Projectile.width * 0.95f;
			for (int j = 0; j <= 1; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = (Projectile.velocity.RotatedBy(HitboxRotation) / 12f) * Projectile.width * 0.65f * Projectile.scale;
			for (int j = 0; j <= 1; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * (j + 0.5f);
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(180, 241));
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(HitboxRotation).SafeNormalize(Vector2.UnitX) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			float rotation;
			Vector2 origin;
			if (Projectile.ai[1] == 0) {
				rotation = Projectile.rotation + Projectile.velocity.ToRotation() + MathHelper.PiOver4 * 1.5f;
				origin = new Vector2(6, 31 + 25);
			} else {
				rotation = Projectile.rotation + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * Projectile.ai[1]);
				origin = new Vector2(6, 31 + 25 * Projectile.ai[1]);
				if (Main.player[Projectile.owner].gravDir < 0) rotation += MathHelper.PiOver4 * Projectile.ai[1];
			}
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				origin,// origin point in the sprite, 'round which the whole sword rotates
				Projectile.scale,
				Projectile.ai[1] < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None,
				0
			);
			return false;
		}
	}
}
