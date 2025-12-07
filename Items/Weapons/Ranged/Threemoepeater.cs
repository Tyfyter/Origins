using Origins.Dev;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Threemoepeater : ModItem, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => [
            "Repeater"
        ];
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.HallowedRepeater);
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 5;
			Item.noMelee = true;
			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.width = 50;
			Item.height = 10;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 12)
			.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 0; i < 2; i++) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.9f, 1.1f),
					Threemoepeater_P.ID,
					damage / 2,
					knockback,
					player.whoAmI
				);
			}
			return true;
		}
	}
	public class Threemoepeater_P : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Amoeba_Ball";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SpikyBall);
			Projectile.timeLeft = 180;
			Projectile.ArmorPenetration += 15;
		}
		public override Color? GetAlpha(Color lightColor) {
			const float add = 510;
			const float max = add + 255;
			return new Color((lightColor.R + add) / max, (lightColor.G + add) / max, (lightColor.B + add) / max, 0.5f) * Math.Min(Projectile.timeLeft / 20f, 1);
		}
	}
}
