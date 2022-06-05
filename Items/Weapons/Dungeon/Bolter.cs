using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Dungeon {
    public class Bolter : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bolt Gun");
            Tooltip.SetDefault("Get boned");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Handgun);
            Item.damage = 26;
			Item.knockBack = 5;
			Item.crit = 4;
            Item.useTime = Item.useAnimation = 17;
			Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 8;
            Item.width = 38;
            Item.height = 18;
            Item.autoReuse = true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Projectile p = Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
            if(p.penetrate>0) {
                p.penetrate++;
                p.localNPCHitCooldown = 10;
                p.usesLocalNPCImmunity = true;
            }
            return false;
        }
    }
}
