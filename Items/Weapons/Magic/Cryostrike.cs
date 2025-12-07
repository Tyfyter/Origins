using Microsoft.Xna.Framework;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Magic {
	public class Cryostrike : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Wand"
        ];
        public override void SetStaticDefaults() {
			Item.staff[Item.type] = true;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 16;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.mana = 10;
			Item.shoot = ModContent.ProjectileType<Cryostrike_P>();
			Item.value = Item.sellPrice(silver: 48);
			Item.rare = ItemRarityID.Blue;
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
