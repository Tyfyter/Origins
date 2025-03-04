using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Laser_Line_Bow : ModItem {
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.laserBow = true;
			});
		}
		public override void SetDefaults() {
			Item.damage = 53;
			Item.crit = 6;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 29;
			Item.useTime = 29;
			Item.width = 56;
			Item.height = 26;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.useAmmo = AmmoID.Arrow;
			Item.UseSound = SoundID.Item5;
			Item.shootSpeed = 12.75f;
			Item.value = Item.sellPrice(gold: 2, silver: 20);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Eitrite_Bar>(14)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
	}
}
