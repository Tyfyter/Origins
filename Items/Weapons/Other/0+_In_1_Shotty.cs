using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Origins.Items.Weapons.Other {
    public class Shotty_x2 : ModItem {
        public override string Texture => "Origins/Items/Weapons/Other/2_In_1_Shotty";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("2 in 1 Shotty");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Boomstick);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 speed = new Vector2(speedX, speedY);
            for(int i = Main.rand.Next(5,8); i-->0;) {
                Projectile.NewProjectile(position, speed.RotatedByRandom(0.5f), type, damage, knockBack, player.whoAmI);
            }
            return true;
        }
    }
    public class Shotty_x3 : ModItem {
        public override string Texture => "Origins/Items/Weapons/Other/3_In_1_Shotty";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("3 in 1 Shotty");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Boomstick);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 speed = new Vector2(speedX, speedY);
            for(int i = Main.rand.Next(8,12); i-->0;) {
                Projectile.NewProjectile(position, speed.RotatedByRandom(0.5f), type, damage, knockBack, player.whoAmI);
            }
            return true;
        }
    }
}
