using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Riven {
    public class Riven_Splitter : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Splitter");
            Tooltip.SetDefault("Uses harpoons as ammo\n87.5% chance not to consume ammo");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.damage = 24;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.useAnimation = 3;
            Item.useTime = 3;
            Item.reuseDelay = 2;
            Item.width = 56;
            Item.height = 26;
            Item.useAmmo = Harpoon.ID;
            Item.shoot = ProjectileID.Harpoon;//ModContent.ProjectileType<Lava_Shot>();
            Item.shootSpeed = 18.75f;
            Item.UseSound = SoundID.Item11;
            Item.rare = ItemRarityID.Blue;
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
			if (!Main.rand.NextBool(8)) {
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
