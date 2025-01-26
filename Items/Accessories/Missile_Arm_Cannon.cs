using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Missile_Armcannon : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ExplosiveBoostAcc"
		];
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type] = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher];
            glowmask = Origins.AddGlowMask(this);
        }
        static short glowmask;
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
            Item.glowMask = glowmask;
        }
		public override void AddRecipes() {
			CreateRecipe()
            .AddIngredient(ItemID.TitanGlove)
            .AddIngredient(ModContent.ItemType<Destructive_Claws>())
			.AddIngredient(ModContent.ItemType<Gun_Glove>())
            .AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
            player.GetModPlayer<OriginPlayer>().explosiveBlastRadius += 0.15f;
			player.GetKnockback(DamageClasses.Explosive) += 0.2f;
            originPlayer.explosiveThrowSpeed += 0.25f;
            originPlayer.destructiveClaws = true;
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
		}
	}
}
