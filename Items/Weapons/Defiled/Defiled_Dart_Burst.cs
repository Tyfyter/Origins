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
            Tooltip.SetDefault("Very darty");
        }
        public override void SetDefaults() {
            Item.damage = 25;
            Item.ranged = true;
            Item.noMelee = true;
            Item.crit = -4;
            Item.width = 56;
            Item.height = 18;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = 5;
            Item.knockBack = 5;
            Item.shootSpeed = 20f;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useAmmo = AmmoID.Dart;
            Item.value = 80000;
            Item.useTurn = false;
            Item.rare = ItemRarityID.Pink;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item99;//new LegacySoundStyle(SoundID.Item, Origins.Sounds.Krunch);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            for(int i = 0; i<2; i++)Projectile.NewProjectile(position, new Vector2(speedX, speedY).RotatedByRandom(i/10f), type, damage, knockBack, player.whoAmI);
            return false;
        }
	}
}
