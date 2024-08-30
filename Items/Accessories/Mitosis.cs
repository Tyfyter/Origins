using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
	public class Mitosis : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<Plasma_Cutter>()] = Type;
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Plasma_Cutter>();
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 26);
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);

			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.shootSpeed = 5;
			Item.shoot = ModContent.ProjectileType<Mitosis_P>();
		}
	}
	public class Mitosis_P : ModProjectile {
		public override string GlowTexture => Texture;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.DamageType = DamageClass.Default;
			Projectile.aiStyle = ProjAIStyleID.Hook;
			Projectile.penetrate = -1;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 900;
			Projectile.alpha = 150;
		}
		public override void AI() {
			Projectile.aiStyle = 0;
			Projectile.velocity *= 0.95f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (i == Projectile.whoAmI) continue;
				Projectile other = Main.projectile[i];
				if (other.active && !ProjectileID.Sets.IsAWhip[other.type] && other.Colliding(other.Hitbox, Projectile.Hitbox)) {
					OriginGlobalProj globalProj = other.GetGlobalProjectile<OriginGlobalProj>();
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
							other.ai[1],
							other.ai[2]
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
		public override bool? CanUseGrapple(Player player) {
			//if (!player.CheckMana()) return false;
			if (player.ownedProjectileCounts[Type] > 0) {
				foreach (Projectile proj in Main.ActiveProjectiles) {
					if (proj.type == Type && proj.owner == player.whoAmI) {
						proj.Kill();
						break;
					}
				}
			}
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
	}
}
