using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;

namespace Origins.Items.Weapons.Other {
    public class Shotty_x2 : ModItem {
        public override string Texture => "Origins/Items/Weapons/Other/2_In_1_Shotty";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("2 in 1 Shotty");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Boomstick);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    for(int i = Main.rand.Next(5,8); i-->0;) {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f), type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
    public class Shotty_x3 : ModItem {
        public override string Texture => "Origins/Items/Weapons/Other/3_In_1_Shotty";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("3 in 1 Shotty");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Boomstick);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            for(int i = Main.rand.Next(8,12); i-->0;) {
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.5f), type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
}
