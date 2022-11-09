using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
    public class Harpoon_Gun : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Harpoon Gun");
            Tooltip.SetDefault("Uses harpoons as ammo");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.damage = 17;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.useAnimation = 3;
            Item.useTime = 3;
            Item.reuseDelay = 2;
            Item.width = 58;
            Item.height = 22;
            Item.useAmmo = Harpoon.ID;
            Item.shoot = ProjectileID.Harpoon;//ModContent.ProjectileType<Lava_Shot>();
            Item.shootSpeed = 14.75f;
            Item.UseSound = SoundID.Item11;
            Item.rare = ItemRarityID.Green;
            Item.autoReuse = true;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
        bool consume = false;
		public override bool CanShoot(Player player) {
            consume = true;
            return true;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
            consume = false;
			return true;
		}
		public override void OnConsumeAmmo(Item ammo, Player player) {
			if (!Main.rand.NextBool(7)) {
                ammo.stack++;
            } else {
                consume = true;
            }
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1:consume?1:0);
            return false;
		}
	}
}
