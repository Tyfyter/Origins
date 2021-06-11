using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Origins.Items.Weapons.Explosives {
    public class Bomboomstick : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bomboomstick");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Boomstick);
            item.useAmmo = ItemID.Grenade;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 speed = new Vector2(speedX, speedY);
            for(int i = Main.rand.Next(3,5); i-->0;) {
                Projectile.NewProjectile(position, speed.RotatedByRandom(0.5f), type, damage, knockBack, player.whoAmI);
            }
            return false;
        }
    }
}
