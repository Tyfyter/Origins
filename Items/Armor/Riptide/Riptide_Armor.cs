using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Armor.Riptide {
	[AutoloadEquip(EquipType.Head)]
	public class Riptide_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riptide Helm");
			Tooltip.SetDefault("5% increased magic damage\nGreatly extends underwater breathing");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.buyPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Magic) += 0.05f;
			player.breathMax += 63;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Riptide_Breastplate>() && legs.type == ModContent.ItemType<Riptide_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "5% increased magic damage when submerged or in rain\nGrants the ability to dash which releases a tidal wave";
			player.GetModPlayer<OriginPlayer>().riptideSet = true;
			if (player.wet) {//TODO: rain
				player.GetDamage(DamageClass.Magic) += 0.05f;
			}
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ShellPileBlock, 9);
			recipe.AddIngredient(ItemID.DivingHelmet);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Riptide_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riptide Breastplate");
			Tooltip.SetDefault("Emit a small aura of light\n+60 mana capacity");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 5;
			Item.value = Item.buyPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.statManaMax2 += 60;
			Lighting.AddLight(player.Center, new Vector3(0, 1, 1));
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ShellPileBlock, 17);
			recipe.AddIngredient(ItemID.BlueJellyfish, 3);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ShellPileBlock, 17);
			recipe.AddIngredient(ItemID.GreenJellyfish, 3);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ShellPileBlock, 17);
			recipe.AddIngredient(ItemID.PinkJellyfish, 3);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Riptide_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riptide Leggings");
			Tooltip.SetDefault("5% increased magic damage\nGrants the ability to swim and provides increased movement speed in water");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.buyPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Magic) += 0.05f;
			player.accFlipper = true;
			player.GetModPlayer<OriginPlayer>().riptideLegs = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ShellPileBlock, 13);
			recipe.AddIngredient(ItemID.Flipper);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class Riptide_Dash_P : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riptide Dash");
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 19;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DD2SquireSonicBoom);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.extraUpdates = 0;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 80;
		}
		public override void AI() {
			if (Projectile.timeLeft >= 40) {
				Projectile.rotation = Projectile.velocity.ToRotation();
				if (Projectile.timeLeft >= 65) {
					Player player = Main.player[Projectile.owner];
					OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
					if (originPlayer.riptideDashTime != 0) {
						Projectile.timeLeft = 66;
						Projectile.Center = player.Center + new Vector2((player.width - 12) * Math.Sign(originPlayer.riptideDashTime), 0);
					}
				}
			}
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (Projectile.penetrate == 1) {
				Projectile.penetrate = 2;
				Projectile.timeLeft = 40;
				Projectile.velocity = Vector2.Zero;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.timeLeft < 40) {
				return false;
			}
			Rectangle hitbox;
			Vector2 offset = Vector2.Zero;
			Vector2 velocity = Vector2.UnitY * 8;
			for (int n = 0; n < 4; n++) {
				offset += velocity;
				hitbox = projHitbox;
				hitbox.Offset(offset.ToPoint());
				if (hitbox.Intersects(targetHitbox)) return true;
				hitbox = projHitbox;
				hitbox.Offset((-offset).ToPoint());
				if (hitbox.Intersects(targetHitbox)) return true;
			}
			return null;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2[] positions = new Vector2[Projectile.oldPos.Length + 1];
			Projectile.oldPos.CopyTo(positions, 1);
			positions[0] = Projectile.position;
			Vector2 halfSize = new Vector2(Projectile.width, Projectile.height) * 0.5f;
			int alpha = 0;
			int min = Math.Max(40 - Projectile.timeLeft, 0);
			for (int i = positions.Length - 1; i >= min; i--) {
				if ((i & 3) != 0) {
					continue;
				}
				alpha = (20 - i) * 10;
				Main.EntitySpriteDraw(
					texture,
					(positions[i] + halfSize) - Main.screenPosition,
					null,
					new Color(alpha, alpha, alpha, alpha),
					Projectile.rotation,
					texture.Size() * 0.5f,
					Projectile.scale,
					Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
					0);
				if (Main.rand.NextBool(3)) Dust.NewDust(positions[i], Projectile.width, Projectile.height, DustID.Water, Projectile.velocity.X * 0.75f);
			}
			Vector2 offset = Vector2.Zero;
			Vector2 velocity = (Vector2)new PolarVec2(8, Projectile.rotation + MathHelper.PiOver2);
			const int backset = 0;
			if (min + backset < positions.Length) {
				for (int n = 0; n < 4; n++) {
					offset += velocity;
					if (Main.rand.NextBool(3)) Dust.NewDust(positions[min + backset] + offset, Projectile.width, Projectile.height, DustID.Water, Projectile.velocity.X);
					if (Main.rand.NextBool(3)) Dust.NewDust(positions[min + backset] - offset, Projectile.width, Projectile.height, DustID.Water, Projectile.velocity.X);
				}
			}
			return false;
		}
	}
}
