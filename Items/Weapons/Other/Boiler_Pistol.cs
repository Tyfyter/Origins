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
    public class Boiler_Pistol : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Boiler Pistol");
            Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Gatligator);
            item.damage = 53;
            item.useAnimation = 18;
            item.useTime = 9;
            item.width = 48;
            item.height = 26;
            item.useAmmo = ItemID.Fireblossom;
            item.shoot = ProjectileID.Flamelash;
            item.shootSpeed*=2;
            item.UseSound = null;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 velocity = new Vector2(speedX, speedY);
            Vector2 offset = Vector2.Normalize(velocity);
            offset = offset*24+offset.RotatedBy(-MathHelper.PiOver2*player.direction)*8;
            Main.PlaySound(SoundID.Item, position+offset, 41);
            Projectile projectile = Projectile.NewProjectileDirect(position+offset, velocity, type, damage, knockBack, player.whoAmI);
            projectile.extraUpdates+=2;
            item.reuseDelay = 9;
            return false;
        }
    }
}
