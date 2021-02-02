using Microsoft.Xna.Framework;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Cryostrike : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cryostrike");
			Tooltip.SetDefault("Shoots a piercing icicle");
            Item.staff[item.type] = true;
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.RubyStaff);
			item.damage = 16;
			item.magic = true;
			item.noMelee = true;
			item.width = 28;
			item.height = 30;
			item.useTime = 20;
			item.useAnimation = 20;
			item.mana = 8;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Cryostrike_P>();
			item.rare = ItemRarityID.Green;
		}
    }
}
