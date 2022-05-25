using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
    public class Defiled_Dart_Burst : ModItem {
		public override string Texture => "Origins/Items/Weapons/Defiled/Defiled_Burst";
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("The Kruncher but it shoots darts");
            Tooltip.SetDefault("Very pointy");
        }
        public override void SetDefaults() {
            item.damage = 25;
            item.ranged = true;
            item.noMelee = true;
            item.crit = -4;
            item.width = 56;
            item.height = 18;
            item.useTime = 40;
            item.useAnimation = 40;
            item.useStyle = 5;
            item.knockBack = 5;
            item.shootSpeed = 20f;
            item.shoot = ProjectileID.PurificationPowder;
            item.useAmmo = AmmoID.Dart;
            item.value = 80000;
            item.useTurn = false;
            item.rare = ItemRarityID.Pink;
            item.autoReuse = true;
            item.UseSound = new LegacySoundStyle(SoundID.Item, Origins.Sounds.Krunch);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            for(int i = 0; i<3; i++)Projectile.NewProjectile(position, new Vector2(speedX, speedY).RotatedByRandom(i/10f), type, damage, knockBack, player.whoAmI);
            return false;
        }
	}
}
