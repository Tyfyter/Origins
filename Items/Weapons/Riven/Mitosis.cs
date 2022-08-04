using Microsoft.Xna.Framework;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Riven {
    public class Mitosis : ModItem {
        public override string Texture => "Origins/Items/Weapons/Riven/Ameballoon";
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mitosis");
			Tooltip.SetDefault("");
            glowmask = Origins.AddGlowMask(this, "");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 25;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 18;
			Item.useAnimation = 18;
			//item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Mitosis_P>();
            Item.shootSpeed = 8.75f;
            Item.knockBack = 5f;
			Item.value = 5000;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
            Item.glowMask = glowmask;
            Item.consumable = false;
        }
    }
    public class Mitosis_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Riven/Ameballoon";
		public override string GlowTexture => Texture;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Mitosis");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.aiStyle = 0;
            Projectile.penetrate = -1;
			Projectile.width = 22;
			Projectile.height = 22;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
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
                        duplicated.rotation += 0.1f;

                        other.velocity = other.velocity.RotatedBy(-0.1f);
                        other.rotation -= 0.1f;
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
