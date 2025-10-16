using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using PegasusLib;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Armor.Riptide {
	[AutoloadEquip(EquipType.Head)]
	public class Riptide_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string[] Categories => [
            WikiCategories.ArmorSet,
            WikiCategories.MagicBoostGear
        ];
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Magic) += 0.05f;
			player.AddMaxBreath(63);
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Riptide_Breastplate>() && legs.type == ModContent.ItemType<Riptide_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Riptide");
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.riptideSet = true;
			if (player.wet || originPlayer.timeSinceRainedOn < 60) {
				player.GetDamage(DamageClass.Magic) += 0.06f;
			}
		}
		public override void ArmorSetShadows(Player player) {
			if (Main.rand.NextBool(2)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.WaterCandle, 0, 1, 255, new Color(255, 150, 30), 1f);
				dust.noGravity = true;
				dust.velocity *= 1.2f;
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.FallenStar, 11)
            .AddIngredient(ItemID.ShellPileBlock, 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Riptide_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Riptide_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Riptide_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Riptide_Breastplate : ModItem, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Item.defense = 5;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.statManaMax2 += 40;
			Lighting.AddLight(player.Center, new Vector3(0, 1, 1));
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.FallenStar, 24)
            .AddIngredient(ItemID.ShellPileBlock, 17)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Riptide_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Magic) += 0.05f;
			player.accFlipper = true;
			player.GetModPlayer<OriginPlayer>().riptideLegs = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.FallenStar, 20)
            .AddIngredient(ItemID.ShellPileBlock, 13)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Riptide_Dash_P : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
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
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
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
