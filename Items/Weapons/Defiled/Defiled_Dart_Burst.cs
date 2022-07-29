using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Defiled {
    public class Defiled_Dart_Burst : ModItem {
		public override string Texture => "Origins/Items/Weapons/Defiled/Defiled_Burst";
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("The Kruncher but it shoots darts");
            Tooltip.SetDefault("Very darty");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.damage = 25;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.crit = -4;
            Item.width = 56;
            Item.height = 18;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
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
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    for(int i = 0; i<2; i++)Projectile.NewProjectile(source, position, velocity.RotatedByRandom(i/10f), type, damage, knockback, player.whoAmI);
            return false;
        }
	}
}
