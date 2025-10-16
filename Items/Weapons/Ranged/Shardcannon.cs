using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Shardcannon : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Gun
        ];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Gatligator);
			Item.damage = 6;
			Item.useAnimation = Item.useTime = 20;
			Item.shoot = ModContent.ProjectileType<Shardcannon_P1>();
			Item.shootSpeed = 6.5f;
			Item.width = 44;
			Item.height = 20;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) type = Item.shoot + Main.rand.Next(3);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = Main.rand.Next(2, 4); i-->0;) {
				switch (type - Item.shoot) {
					case 0 or 1:
					type += 1;
					break;
					case 2:
					type -= 2;
					break;
				}
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.8f, 1.2f),
					type,
					damage,
					knockback,
					player.whoAmI
				);
			}
			return true;
		}
	}
	public class Shardcannon_P1 : ModProjectile {
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bullet);
			Projectile.extraUpdates = 2;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 3;
			Projectile.localNPCHitCooldown = 10;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
			if (Projectile.shimmerWet) {
				int num = (int)(Projectile.Center.X / 16f);
				int num2 = (int)(Projectile.position.Y / 16f);
				if (WorldGen.InWorld(num, num2) && Main.tile[num, num2] != null && Main.tile[num, num2].LiquidAmount == byte.MaxValue && Main.tile[num, num2].LiquidType == LiquidID.Shimmer && WorldGen.InWorld(num, num2 - 1) && Main.tile[num, num2 - 1] != null && Main.tile[num, num2 - 1].LiquidAmount > 0 && Main.tile[num, num2 - 1].LiquidType == LiquidID.Shimmer) {
					Projectile.Kill();
				} else if (Projectile.velocity.Y > 0f) {
					Projectile.velocity.Y *= -1f;
					Projectile.netUpdate = true;
					if (Projectile.timeLeft > 600)
						Projectile.timeLeft = 600;

					Projectile.timeLeft -= 60;
					Projectile.shimmerWet = false;
					Projectile.wet = false;
				}
			}
		}
		public override Color? GetAlpha(Color lightColor) {
			if (Projectile.alpha < 200) {
				return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);
			}
			return Color.Transparent;
		}
		public override void OnKill(int timeLeft) {
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
	public class Shardcannon_P2 : Shardcannon_P1 { }
	public class Shardcannon_P3 : Shardcannon_P1 { }
}
