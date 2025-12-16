using AltLibrary.Common.AltBiomes;
using AltLibrary.Core;
using Origins.Items.Weapons.Ammo;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	public abstract class TORenewals : ModItem {
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Fargowiltas");
		protected override bool CloneNewInstances => true;
		protected internal TORenewal_P Proj;
		public virtual bool Supreme => false;
		public abstract ModItem Material { get; }
		public abstract AltBiome Biome { get; }

		public override void Load() {
			Mod.AddContent(Proj = new TORenewal_P(this));
		}
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
			if (Proj?.Projectile is not null) Item.shoot = Proj.Type;
			Item.shootSpeed = 5f;
		}
		public override void AddRecipes() {
			int amt;
			Recipe recipe = Recipe.Create(Type);
			if (Supreme) {
				amt = 10;
				recipe.AddIngredient(ItemID.ChlorophyteBar, 5);
				recipe.AddTile(TileID.AlchemyTable);
			} else {
				amt = 100;
				recipe.AddIngredient(ItemID.Bottle);
				recipe.AddTile(TileID.Bottles);
			}
			recipe.AddIngredient(Material, amt);
			recipe.Register();
		}
	}
	[Autoload(false)]
	public class TORenewal_P(TORenewals item) : ModProjectile {
		protected override bool CloneNewInstances => true;
		readonly TORenewals item = item;
		public override string Name => item.Name + "_P";
		public override LocalizedText DisplayName => item.DisplayName;
		public override string Texture => item.Texture;

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
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
			int dst = 150;
			Vector2[] vel = [new(0, 5), new(0, -5), new(5, 0), new(5), new(5, -5), new(-5, 0), new(-5, 5), new(-5)];

			if (NetmodeActive.SinglePlayer) {
				for (int i = 0; i < vel.Length; i++) {
					int proj = item.Material.Item.shoot;
					if (item.Material is TORenewals renewal) proj = renewal.Material.Item.shoot;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel[i], proj, 0, 0f, Projectile.owner);
				}
			}

			int x = dst;
			int y = dst;
			if (item.Supreme) {
				x = Main.maxTilesX;
				y = Main.maxTilesY;
			}

			for (int i = -x; i <= x; i++) {
				for (int j = -y; j <= y; j++) {
					int i3 = (int)(i + Projectile.Center.X / 16f);
					int j3 = (int)(j + Projectile.Center.Y / 16f);
					if (item.Supreme || Math.Sqrt(i * i + j * j) <= dst + 0.5) ALConvert.Convert(item.Biome, i3, j3, 1);
				}
			}
		}
	}

	public class AshenRenewal : TORenewals {
		public override ModItem Material => ModContent.GetInstance<Orange_Solution>();
		public override AltBiome Biome => ModContent.GetInstance<Ashen_Alt_Biome>();
	}
	public class AshenSupremeRenewal : TORenewals {
		public override bool Supreme => true;
		public override ModItem Material => ModContent.GetInstance<AshenRenewal>();
		public override AltBiome Biome => ModContent.GetInstance<Ashen_Alt_Biome>();
	}

	public class DefiledRenewal : TORenewals {
		public override ModItem Material => ModContent.GetInstance<Gray_Solution>();
		public override AltBiome Biome => ModContent.GetInstance<Defiled_Wastelands_Alt_Biome>();
	}
	public class DefiledSupremeRenewal : TORenewals {
		public override bool Supreme => true;
		public override ModItem Material => ModContent.GetInstance<DefiledRenewal>();
		public override AltBiome Biome => ModContent.GetInstance<Defiled_Wastelands_Alt_Biome>();
	}

	public class RivenRenewal : TORenewals {
		public override ModItem Material => ModContent.GetInstance<Teal_Solution>();
		public override AltBiome Biome => ModContent.GetInstance<Riven_Hive_Alt_Biome>();
	}
	public class RivenSupremeRenewal : TORenewals {
		public override bool Supreme => true;
		public override ModItem Material => ModContent.GetInstance<RivenRenewal>();
		public override AltBiome Biome => ModContent.GetInstance<Riven_Hive_Alt_Biome>();
	}
}