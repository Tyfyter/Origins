using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.Projectiles;

namespace Origins.Items.Weapons.Other {
    public class Viper_Rifle : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("HNO-3 \"Viper\"");
            Tooltip.SetDefault("Has a chance to inflict \"Solvent\", increasing critical damage\nDeals critical damage on otherwise afflicted enemies");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Gatligator);
            item.damage = 48;
            item.crit = 5;
            item.knockBack = 6.75f;
            item.useAnimation = item.useTime = 27;
            item.width = 114;
            item.height = 40;
            item.autoReuse = false;
            item.scale = 0.75f;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            OriginGlobalProj.viperEffectNext = true;
            Vector2 unit = Vector2.Normalize(new Vector2(speedX, speedY));
            position+=unit*16;
            float dist = 80 - new Vector2(speedX, speedY).Length();
            Projectile barrelProj = Projectile.NewProjectileDirect(position, unit*(dist/20), type, damage, knockBack, player.whoAmI);
            barrelProj.extraUpdates = 19;
            barrelProj.timeLeft = 20;
            OriginGlobalProj.viperEffectNext = true;
            OriginGlobalProj.killLinkNext = barrelProj.whoAmI;
            position+=unit*dist;
            return true;
        }
    }
}
