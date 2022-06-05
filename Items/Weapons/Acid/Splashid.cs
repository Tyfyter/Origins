using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Projectiles.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Acid {
    public class Splashid : ModItem, IElementalItem {
		static short glowmask;
		public ushort Element => Elements.Acid;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acid Splash");
			Tooltip.SetDefault("");
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 52;
			Item.magic = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
            Item.useStyle = 1;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 24;
			Item.useAnimation = 24;
            Item.reuseDelay = 8;
			Item.mana = 18;
			Item.value = 5000;
            Item.shoot = ModContent.ProjectileType<Acid_Shot>();
			Item.rare = ItemRarityID.Lime;
			Item.glowMask = glowmask;
		}
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            int a = Main.rand.Next(5,7);
			//++i < a: increments i and returns true if i < a/2
			//a = Main.rand.Next(5,7): randomize a every loop so ((i-a/2f)/a) returns a random value but maintains a mostly constant spread
            for(int i = 0; ++i < a; a = Main.rand.Next(5,7)) {
				//((i-a/2f)/a): returns a value based on i between -0.5 and 0.5
                Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY).RotatedBy(((i-a/2f)/a)*0.75), type, damage, knockBack, player.whoAmI, 0, 12f).timeLeft+=i;
            }
            return false;
        }
    }
}
