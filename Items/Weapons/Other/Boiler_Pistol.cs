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
    public class Boiler_Pistol : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Boiler Pistol");
            Tooltip.SetDefault("Uses fireblossoms as ammo");
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Gatligator);
            Item.damage = 53;
            Item.useAnimation = 18;
            Item.useTime = 12;
            Item.width = 48;
            Item.height = 26;
            Item.useAmmo = ItemID.Fireblossom;
            Item.shoot = ModContent.ProjectileType<Lava_Shot>();
            Item.shootSpeed*=1.75f;
            Item.UseSound = null;
            Item.scale = 0.8f;
            Item.glowMask = glowmask;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 velocity = new Vector2(speedX, speedY);
            Vector2 offset = Vector2.Normalize(velocity);
            offset = offset*24+offset.RotatedBy(-MathHelper.PiOver2*player.direction)*8;
            SoundEngine.PlaySound(SoundID.Item, position+offset, 41);
            position+=offset;
            Item.reuseDelay = 36;
            Lava_Shot.damageType = 2;
            return true;
            //Projectile projectile = Projectile.NewProjectileDirect(position+offset, velocity, type, damage, knockBack, player.whoAmI);
            //projectile.extraUpdates+=2;
            //projectile.aiStyle = -1;
        }
    }
}
