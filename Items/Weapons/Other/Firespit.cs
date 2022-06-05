using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.Projectiles.Weapons;

namespace Origins.Items.Weapons.Other {
    public class Firespit : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Firespit");
            Tooltip.SetDefault("");
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
            Item.damage = 30;
            Item.magic = true;
            Item.useStyle = 5;
            Item.crit = 1;
            Item.useAnimation = 35;
            Item.useTime = 1;
            Item.mana = 16;
            Item.width = 58;
            Item.height = 22;
            Item.shoot = ModContent.ProjectileType<Lava_Shot>();
            Item.shootSpeed = 6.75f;
            Item.UseSound = null;
            //item.reuseDelay = 9;
            Item.glowMask = glowmask;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(player.itemAnimationMax-player.itemAnimation > 9)return false;
            Vector2 velocity = new Vector2(speedX, speedY);
            Vector2 offset = Vector2.Normalize(velocity);
            offset = offset*24+offset.RotatedBy(-MathHelper.PiOver2*player.direction)*8;
            SoundEngine.PlaySound(SoundID.Item, position+offset, 20);
            position+=offset;
            velocity = velocity.RotatedByRandom(0.5);
            speedX = velocity.X;
            speedY = velocity.Y;
            Lava_Shot.damageType = 3;
            return true;
            //Projectile projectile = Projectile.NewProjectileDirect(, type, damage, knockBack, player.whoAmI);
        }
    }
}
