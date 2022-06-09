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
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Acid {
    public class Splashid : ModItem, IElementalItem {
		static short glowmask;
		public ushort Element => Elements.Acid;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acid Splash");
			Tooltip.SetDefault("");
			glowmask = Origins.AddGlowMask(this);
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 52;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
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
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    int a = Main.rand.Next(5,7);
			//++i < a: increments i and returns true if i < a/2
			//a = Main.rand.Next(5,7): randomize a every loop so ((i-a/2f)/a) returns a random value but maintains a mostly constant spread
            for(int i = 0; ++i < a; a = Main.rand.Next(5,7)) {
				//((i-a/2f)/a): returns a value based on i between -0.5 and 0.5
                Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(((i-a/2f)/a)*0.75), type, damage, knockback, player.whoAmI, 0, 12f).timeLeft+=i;
            }
            return false;
        }
    }
}
