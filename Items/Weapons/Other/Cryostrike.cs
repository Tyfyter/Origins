using Microsoft.Xna.Framework;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
//using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Other {
	public class Cryostrike : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cryostrike");
			Tooltip.SetDefault("Shoots a piercing icicle");
            Item.staff[Item.type] = true;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 16;
			Item.magic = true;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.mana = 8;
			Item.value = 5000;
            Item.shoot = ModContent.ProjectileType<Cryostrike_P>();
			Item.rare = ItemRarityID.Green;
            Item.scale = 0.85f;
		}
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 offset = Vector2.Normalize(new Vector2(speedX,speedY))*50;
            if(Collision.CanHitLine(position, 1, 1, position+offset, 1, 1))position+=offset;
            return true;
        }
        public override Vector2? HoldoutOrigin() {
            return new Vector2(6,6);
        }
    }
}
