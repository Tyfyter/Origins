using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Dart_Crossbow : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "DartLauncher"
        ];
        public override void SetDefaults() {
			Item.damage = 36;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.crit = 6;
			Item.width = 52;
			Item.height = 16;
			Item.useTime = 30;
			Item.useAnimation = 30;
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
	}
}
