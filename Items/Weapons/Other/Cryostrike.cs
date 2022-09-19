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
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 16;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.mana = 8;
			Item.value = 5000;
            Item.shoot = ModContent.ProjectileType<Cryostrike_P>();
			Item.rare = ItemRarityID.White;
            Item.scale = 0.85f;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 offset = Vector2.Normalize(velocity) * 50;
			if (Collision.CanHitLine(position, 1, 1, position + offset, 1, 1)) position += offset;
		}
		public override Vector2? HoldoutOrigin() {
            return null;
        }
    }
}
