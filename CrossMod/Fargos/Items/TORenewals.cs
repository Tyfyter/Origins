using AltLibrary.Common.AltBiomes;
using AltLibrary.Core;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	public abstract class TORenewals<TMaterial, TSolution>(bool supreme = false) : ModItem where TMaterial : ModItem where TSolution : ModProjectile {
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Fargowiltas");

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 10;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 26;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item1;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.value = Item.buyPrice(0, 0, 3);
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shoot = 1;
			Item.shootSpeed = 5f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(player.GetSource_ItemUse(source.Item), position, velocity, ModContent.ProjectileType<TSolution>(), 0, 0f, Main.myPlayer, 0f, 0f, 0f);
			return false;
		}

		public override void AddRecipes() {
			int amt;
			Recipe recipe = Recipe.Create(Type);
			if (supreme) {
				amt = 10;
				recipe.AddIngredient(ItemID.ChlorophyteBar, 5);
				recipe.AddTile(TileID.AlchemyTable);
			} else {
				amt = 100;
				recipe.AddIngredient(ItemID.Bottle);
				recipe.AddTile(TileID.Bottles);
			}
			recipe.AddIngredient<TMaterial>(amt);
			recipe.Register();
		}
	}
	public abstract class TORenewal_P<TProj, TBiome>(bool supreme = false) : ModProjectile where TProj : ModProjectile where TBiome : AltBiome {

		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Fargoswiltas");

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.aiStyle = 2;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 170;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.Kill();
			return true;
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(in SoundID.Shatter, Projectile.Center);
			int num = 150;
			float[] array = [0f, 0f, 5f, 5f, 5f, -5f, -5f, -5f];
			float[] array2 = [5f, -5f, 0f, 5f, -5f, 0f, 5f, -5f];
			if (Main.netMode == 0) {
				for (int i = 0; i < 8; i++) {
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X, Projectile.Center.Y, array[i], array2[i], ModContent.ProjectileType<TProj>(), 0, 0f, Main.myPlayer);
				}
			}

			if (supreme) {
				for (int j = -Main.maxTilesX; j < Main.maxTilesX; j++) {
					for (int k = -Main.maxTilesY; k < Main.maxTilesY; k++) {
						int i2 = (int)(j + Projectile.Center.X / 16f);
						int j2 = (int)(k + Projectile.Center.Y / 16f);
						ALConvert.Convert<TBiome>(i2, j2, 1);
					}
				}

				return;
			}

			for (int l = -num; l <= num; l++) {
				for (int m = -num; m <= num; m++) {
					int i3 = (int)(l + Projectile.Center.X / 16f);
					int j3 = (int)(m + Projectile.Center.Y / 16f);
					if (Math.Sqrt(l * l + m * m) <= num + 0.5) ALConvert.Convert<TBiome>(i3, j3, 1);
				}
			}
		}
	}
}