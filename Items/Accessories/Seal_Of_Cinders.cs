using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Seal_Of_Cinders : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 34);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Blue;

			Item.shoot = ModContent.ProjectileType<Seal_Of_Cinders_Dust>();
			Item.damage = 16;
			Item.ArmorPenetration = 6;
			Item.knockBack = 6;
			Item.useTime = 15;
			Item.useAnimation = 4;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<OriginPlayer>().cinderSealItem = Item;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(
				player.GetSource_Accessory_OnHurt(source.Item, attacker: null),
				position,
				new Vector2(1, 0).RotatedByRandom(MathHelper.Pi),
				type,
				damage,
				knockback,
				player.whoAmI
			);
			return false;
		}
	}
	public class Seal_Of_Cinders_Dust : ModProjectile {
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 10;
			Projectile.width = Projectile.height = 6;
		}
		public override void AI() {
			Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ash, Alpha: 100).noGravity = true;
			Projectile.velocity *= 0.95f;
		}
		public override void OnKill(int timeLeft) {
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item38.WithVolumeScale(0.5f), Projectile.Center);
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);
			if (Projectile.owner == Main.myPlayer) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Seal_Of_Cinders_Explosion>(),
					Projectile.originalDamage,
					Projectile.knockBack,
					Projectile.owner
				);
			}
		}
	}
	public class Seal_Of_Cinders_Explosion : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 19;
		}
		public override void SetDefaults() {
			Projectile.tileCollide = false;
			Projectile.width = Projectile.height = 110;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
		}
		public override void AI() {
			if (++Projectile.frameCounter > 1) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.timeLeft = 0;
			}
			Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.1f, 0f) * MathHelper.Min(Projectile.frame / 8f, 1));
		}
		public override bool PreDraw(ref Color lightColor) {
			lightColor = new(1f, 1f, 1f, 0f);
			return true;
		}
	}
}