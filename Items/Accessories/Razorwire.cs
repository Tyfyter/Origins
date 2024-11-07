using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Razorwire : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 28);
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1);
			Item.shoot = ModContent.ProjectileType<Razorwire_P>();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.razorwire = true;
			originPlayer.razorwireItem = Item;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Shackle)
			.AddIngredient(ModContent.ItemType<Return_To_Sender>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Razorwire_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SilverBullet;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.BulletHighVelocity);
			Projectile.DamageType = DamageClass.Generic;
			Projectile.penetrate = 1;
			Projectile.alpha = 0;
		}
	}
}
