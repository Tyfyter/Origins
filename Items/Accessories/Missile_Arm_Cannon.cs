using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	//[AutoloadEquip(EquipType.HandsOn)]
	public class Missile_Armcannon : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Missile Armcannon");
			Tooltip.SetDefault("30% increased explosive throwing velocity\nIncreases attack speed of thrown explosives\nEnables autouse for all explosive weapons\nShoots rockets as you swing\n'Payload not included'");
			SacrificeTotal = 1;
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type] = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher];
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(38, 20);
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;

			Item.damage = 10;
			Item.DamageType = DamageClasses.Explosive;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.shootSpeed = 5;
			Item.useAmmo = AmmoID.Rocket;
			Item.UseSound = SoundID.Item61;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Destructive_Claws>());
			recipe.AddIngredient(ModContent.ItemType<Gun_Glove>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
			originPlayer.destructiveClaws = true;
			originPlayer.explosiveThrowSpeed += 0.3f;
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
		}
	}
}
