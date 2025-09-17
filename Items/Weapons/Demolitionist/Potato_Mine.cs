using Origins.Dev;
using Origins.Items.Other.Consumables.Food;
using Origins.Projectiles;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Potato_Mine : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"OtherExplosive",
			"ExpendableWeapon"
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LandMine);
			Item.damage = 50;
			Item.createTile = ModContent.TileType<Potato_Mine_Tile>();
			Item.value = Item.sellPrice(silver: 1,  copper: 20);
			Item.rare = ItemRarityID.Blue;
			Item.ammo = ModContent.ItemType<Potato>();
			Item.notAmmo = true;
			Item.noMelee = !OriginsModIntegrations.CheckAprilFools();
			Item.mech = false;
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.NewProjectile(player.GetSource_OnHit(target), player.itemLocation, default, ModContent.ProjectileType<Potato_Mine_Melee_Explosion>(), 50, 6);
			Item.stack--;
		}
		public override void HoldItem(Player player) {
			Item.shoot = ProjectileID.None;
		}
		public override void UpdateInventory(Player player) {
			Item.shoot = Potato_Mine_P.ID; // has to be done here somewhere like this because it blocks placing the tile if it's not 0 when the player uses the item
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 150)
			.AddIngredient(ItemID.ExplosivePowder, 15)
			.AddIngredient(ModContent.ItemType<Potato>())
			.Register();
		}
	}
	public class Potato_Mine_Melee_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.Explosive;
		public override int Size => 96;
		public override bool DealsSelfDamage => true;
	}
}
