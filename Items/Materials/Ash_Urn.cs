﻿using Microsoft.Xna.Framework;
using Origins.Dusts;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Tiles.Ashen;

namespace Origins.Items.Materials
{
    public class Ash_Urn : ModItem {
        public string[] Categories => [
            "ExpendableTool"
        ];
        public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.PurificationPowder;
			Item.ResearchUnlockCount = CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[ItemID.VilePowder];
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.VilePowder);
			Item.shoot = ModContent.ProjectileType<Ash_Urn_P>();
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddIngredient(ModContent.ItemType<Fungarust_Item>())
			.AddTile(TileID.Bottles)
			.Register();

            Recipe.Create(ItemID.PoisonedKnife, 50)
            .AddIngredient(ItemID.ThrowingKnife, 50)
            .AddIngredient(this)
            .Register();
        }
	}
	public class Ash_Urn_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VilePowder;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.VilePowder);
			Projectile.aiStyle = 0;
		}
		public override void AI() {
			Projectile.velocity *= 0.95f;
			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] == 180f) {
				Projectile.Kill();
			}
			if (Projectile.ai[1] == 0f) {
				Projectile.ai[1] = 1f;
				for (int i = 0; i < 30; i++) {
					Dust.NewDust(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						ModContent.DustType<Generic_Powder_Dust>(),
						Projectile.velocity.X,
						Projectile.velocity.Y,
						50,
						new Color(0.15f, 0.15f, 0.15f, 0.1f)
					);
				}
			}
			int minX = (int)(Projectile.position.X / 16f) - 1;
			int maxX = (int)((Projectile.position.X + Projectile.width) / 16f) + 2;
			int minY = (int)(Projectile.position.Y / 16f) - 1;
			int maxY = (int)((Projectile.position.Y + Projectile.height) / 16f) + 2;
			if (minX < 0) {
				minX = 0;
			}
			if (maxX > Main.maxTilesX) {
				maxX = Main.maxTilesX;
			}
			if (minY < 0) {
				minY = 0;
			}
			if (maxY > Main.maxTilesY) {
				maxY = Main.maxTilesY;
			}
			Vector2 comparePos = default;
			for (int x = minX; x < maxX; x++) {
				for (int y = minY; y < maxY; y++) {
					comparePos.X = x * 16;
					comparePos.Y = y * 16;
					if ((Projectile.position.X + Projectile.width > comparePos.X) &&
						(Projectile.position.X < comparePos.X + 16f) &&
						(Projectile.position.Y + Projectile.height > comparePos.Y) &&
						(Projectile.position.Y < comparePos.Y + 16f) &&
						Main.myPlayer == Projectile.owner || Main.tile[x, y].HasTile) {
						//AltLibrary.Core.ALConvert.Convert<Ashen_Metalworks_Alt_Biome>(x, y, 1);
						//WorldGen.Convert(x, y, OriginSystem.origin_conversion_type, 1);
					}
				}
			}
		}
	}
}
