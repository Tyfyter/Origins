using Origins.Dev;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Explosive_Artery : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 20);
			Item.damage = 31;
			Item.DamageType = DamageClasses.Explosive;
			Item.knockBack = 4;
			Item.useTime = 6;
			Item.useAnimation = 1;//used as the numerator for the chance
			Item.reuseDelay = 20;//used as the denominator for the chance
			Item.shoot = ModContent.ProjectileType<Explosive_Artery_P>();
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveArtery = true;
			originPlayer.explosiveArteryItem = Item;
		}
	}
	public class Explosive_Artery_P : ModProjectile {
		public override string Texture => "Origins/Items/Accessories/Explosive_Artery";
		public override void SetStaticDefaults() {
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.penetrate = -1;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.timeLeft = 0;
		}
		public override void OnKill(int timeLeft) {
			Main.player[Projectile.owner].GetModPlayer<OriginPlayer>().messyLeech = false; // so it doesn't cause an OP chain reaction
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item14.WithVolume(0.66f), Projectile.Center);
			for (int i = 0; i < 15; i++) {
				Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.Blood,
					Scale: 1.8f
				);
			}
			Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), default, Main.rand.Next(61, 64)).velocity += Vector2.One;
			Gore.NewGoreDirect(Projectile.GetSource_FromThis(), new Vector2(Projectile.Center.X, Projectile.Center.Y), default, Main.rand.Next(61, 64)).velocity += Vector2.One;
		}
	}
}
