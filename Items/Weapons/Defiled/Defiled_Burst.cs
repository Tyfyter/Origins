using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
    public class Defiled_Burst : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("The Kruncher");
            Tooltip.SetDefault("Very pointy");
        }
        public override void SetDefaults() {
            Item.damage = 15;
            Item.ranged = true;
            Item.noMelee = true;
            Item.crit = -1;
            Item.width = 56;
            Item.height = 18;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = 5;
            Item.knockBack = 5;
            Item.shootSpeed = 20f;
            Item.shoot = ProjectileID.Bullet;
            Item.useAmmo = AmmoID.Bullet;
            Item.value = 15000;
            Item.useTurn = false;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = new LegacySoundStyle(SoundID.Item, Origins.Sounds.Krunch);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            for(int i = 0; i<5; i++)Projectile.NewProjectile(position, new Vector2(speedX, speedY).RotatedByRandom(i/10f), type, damage, knockBack, player.whoAmI);
            return false;
        }
	}
}
