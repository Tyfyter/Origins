using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Dash_Ravel : Ravel {
		public static new int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory();
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 6);
			Item.shoot = ModContent.MountType<Dash_Ravel_Mount>();
		}
		public override void UpdateEquip(Player player) {
			base.UpdateEquip(player);
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (originPlayer.dashDirection != 0) {
				if (player.mount.Type != Item.shoot) ToggleRavel(player);
				player.velocity.X = originPlayer.dashDirection * 8;
				originPlayer.dashDelay = 20;
			}
		}
		protected override void UpdateRaveled(Player player) {
			player.blackBelt = true;
			player.noKnockback = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Tabi)
			.AddIngredient(Ravel.ID)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Dash_Ravel_Mount : Ravel_Mount {
		public override string Texture => "Origins/Items/Accessories/Dash_Ravel";
		public static new int ID { get; private set; } = -1;
		protected override void SetID() {
			MountData.buff = ModContent.BuffType<Dash_Ravel_Mount_Buff>();
			ID = Type;
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
            MountData.acceleration = 0.38f;
            MountData.runSpeed = 12f;
            MountData.dashSpeed = 10f;
		}
		public override void UpdateEffects(Player player) {
			base.UpdateEffects(player);
			if (Math.Abs(player.velocity.X) > MountData.dashSpeed) {
				player.runAcceleration += 0.9f;
				if (--player.mount._abilityCooldown < 0) {
					player.mount._abilityCooldown = 15;
					int dashDirection = Math.Sign(player.velocity.X);
					Projectile.NewProjectile(
						player.GetSource_Misc("ravel_dash"),
						player.Center + new Vector2(player.width * dashDirection * 0.5f, 0),
						new Vector2(player.velocity.X, 0),
						Dash_Ravel_P.ID,
						50,
						(player.velocity.X * dashDirection) + 5,
						player.whoAmI
					);
				}
			}
		}
	}
	public class Dash_Ravel_P : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DD2SquireSonicBoom);
			Projectile.damage = 137;
			Projectile.DamageType = DamageClass.Generic;
			Projectile.alpha = 0;
			Projectile.extraUpdates = 0;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 15;
			Projectile.penetrate = -1;
			Projectile.knockBack = 14;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			Projectile.alpha += 14;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Inflate(4, 12);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int alpha = 255 - Projectile.alpha;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				new Color(alpha, alpha, alpha, alpha),
				Projectile.rotation,
				texture.Size() * 0.5f,
				Projectile.scale,
				Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
			0);
			return false;
		}
	}
	public class Dash_Ravel_Mount_Buff : Ravel_Mount_Buff {
		public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
		protected override int MountID => ModContent.MountType<Dash_Ravel_Mount>();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
	}
}
