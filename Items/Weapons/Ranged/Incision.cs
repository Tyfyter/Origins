using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Incision : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.DartLauncher
        ];
        public override void SetDefaults() {
			Item.damage = 34;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.crit = -4;
			Item.width = 52;
			Item.height = 16;
			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
			Item.shootSpeed = 11f;
			Item.shoot = ProjectileID.PurificationPowder;
			Item.useAmmo = AmmoID.Dart;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item99;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 0; i < 2; i++) Projectile.NewProjectile(source, position, velocity.RotatedByRandom(i / 10f), type, damage, knockback, player.whoAmI);
			return false;
		}
	}
}
