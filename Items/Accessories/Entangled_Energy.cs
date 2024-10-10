using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Entangled_Energy : ModItem, ICustomWikiStat {
		public string[] Categories => [
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 30);
			Item.rare = ItemRarityID.Blue;
			Item.expert = true;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().entangledEnergy = true;
		}
	}
	public class Entangled_Energy_Lifesteal : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetDefaults() {
			Projectile.timeLeft = 300;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) Projectile.Kill();
			if (player.Hitbox.Contains(Projectile.Center.ToPoint())) {
				player.Heal(Projectile.damage);
				Projectile.Kill();
				return;
			}
			Vector2 unit = (player.Center - Projectile.Center).WithMaxLength(12);
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, unit, 0.1f);
			Dust.NewDustPerfect(Projectile.Center, DustID.Glass, Vector2.Zero);
		}
	}
}
