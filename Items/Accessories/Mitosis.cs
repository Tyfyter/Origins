using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
	public class Mitosis : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"Shouldntexist"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 26);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);

			Item.damage = 0;
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.shootSpeed = 5;
			Item.shoot = ModContent.ProjectileType<Amoeba_Bubble>();
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.mitosis = true;
			originPlayer.mitosisItem = Item;
		}
		public override bool RangedPrefix() => false;
	}
	public class Amoeba_Bubble : ModProjectile {
		public override string Texture => "Origins/Items/Accessories/Mitosis_P";
		public override string GlowTexture => Texture;
		
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 900;
			Projectile.alpha = 150;
		}
		public override void AI() {
			Projectile.velocity *= 0.95f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (i == Projectile.whoAmI) continue;
				Projectile other = Main.projectile[i];
				if (other.active && !ProjectileID.Sets.IsAWhip[other.type] && other.Colliding(other.Hitbox, Projectile.Hitbox)) {
					OriginGlobalProj globalProj = other.GetGlobalProjectile<OriginGlobalProj>();
					if (other.type == 1075) {

					}
					if (!globalProj.isFromMitosis && !globalProj.hasUsedMitosis) {
						Projectile duplicated = Projectile.NewProjectileDirect(
							Projectile.GetSource_FromThis(),
							other.Center,
							other.velocity.RotatedBy(0.1f),
							other.type,
							other.damage,
							other.knockBack,
							other.owner,
							other.ai[0],
							other.ai[1]
						);
						duplicated.rotation += 0.25f;

						other.velocity = other.velocity.RotatedBy(-0.25f);
						other.rotation -= 0.25f;
						globalProj.hasUsedMitosis = true;
						if (other.minion) {
							globalProj.mitosisTimeLeft = 300;
						}
					}
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			//Projectile.Kill();
			return false;
		}
	}
}
