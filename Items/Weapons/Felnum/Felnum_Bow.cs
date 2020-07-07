using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum {
    public class Felnum_Bow : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Felnum Longbow");
            Tooltip.SetDefault("Recieves 50% higher damage bonuses\nplaceholder sprite, needs better shading.");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.GoldBow);
            item.damage = 19;
            item.width = 18;
            item.height = 58;
            item.useTime = item.useAnimation = 32;
            item.shootSpeed*=2.5f;
            item.autoReuse = false;
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-8f,0);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            damage+=(damage-19)/2;
            return true;
        }
    }
}
