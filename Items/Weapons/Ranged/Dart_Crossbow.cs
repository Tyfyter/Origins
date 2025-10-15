using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Dart_Crossbow : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.DartLauncher
        ];
        public override void SetDefaults() {
			Item.damage = 62;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.crit = 10;
			Item.width = 52;
			Item.height = 16;
			Item.useTime = 50;
			Item.useAnimation = 50;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
			Item.shootSpeed = 20f;
			Item.shoot = ProjectileID.PurificationPowder;
			Item.useAmmo = AmmoID.Dart;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item99;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
	}
}
