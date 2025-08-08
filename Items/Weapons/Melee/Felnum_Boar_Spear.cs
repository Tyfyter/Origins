using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Armor.Felnum;
using Origins.Items.Materials;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Felnum_Boar_Spear : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			ItemID.Sets.Spears[Type] = true;
			Origins.DamageBonusScale[Type] = 1.5f;
			CritType.SetCritType<Felnum_Crit_Type>(Type);
			OriginsSets.Items.FelnumItem[Type] = true;
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 62;
			Item.height = 72;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6;
			Item.autoReuse = true;
			Item.useTurn = false;
			Item.shootSpeed = 3;
			Item.shoot = ModContent.ProjectileType<Felnum_Boar_Spear_P>();
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 13)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool MeleePrefix() => true;
	}
	[LegacyName("Felnum_Boar_Spear_Stab")]
	public class Felnum_Boar_Spear_P : ModProjectile {
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Felnum_Boar_Spear_P).GetDefaultTMLName() + "_Glow";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.timeLeft = 3600;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 1;
			Projectile.scale = 1f;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			Projectile.direction = projOwner.direction;
			projOwner.heldProj = Projectile.whoAmI;
			projOwner.itemTime = projOwner.itemAnimation;
			Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
			if (!projOwner.frozen) {
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2 - 3) {
					movementFactor -= 2.2f;
				} else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1) {
					movementFactor += 2.2f;
				}
			}
			Projectile.position += Projectile.velocity * movementFactor * Projectile.scale;
			if (projOwner.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
			if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.Pi / 2f;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			Static_Shock_Debuff.Inflict(target, 120);
			Player player = Main.player[Projectile.owner];
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			int shockAmount = (int)(originPlayer.felnumShock / (Felnum_Helmet.shock_damage_divisor * 2));
			if (shockAmount > 0) {
				modifiers.SourceDamage.Flat += shockAmount;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 origin = new(105, 5);
			float rotation = Projectile.rotation;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				rotation,
				origin,
				Projectile.scale,
				SpriteEffects.None
			);
			Main.EntitySpriteDraw(
				glowTexture,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White,
				rotation,
				origin,
				Projectile.scale,
				SpriteEffects.None
			);
			return false;
		}
	}
}
