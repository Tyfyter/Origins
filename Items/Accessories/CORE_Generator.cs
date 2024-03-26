using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Waist)]
	public class CORE_Generator : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.LightPurple;
			Item.accessory = true;
            Item.damage = 10;
            Item.DamageType = DamageClasses.Explosive;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.shootSpeed = 5;
            Item.useAmmo = AmmoID.Rocket; //and mines?
        }
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Last_Descendant>());
            recipe.AddIngredient(ModContent.ItemType<Missile_Armcannon>());
            recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
            player.GetModPlayer<OriginPlayer>().explosiveBlastRadius += 0.15f;
            player.GetKnockback(DamageClasses.Explosive) += 0.5f;
            originPlayer.explosiveProjectileSpeed += 0.3f;

            originPlayer.destructiveClaws = true;
            originPlayer.gunGlove = true;
            originPlayer.gunGloveItem = Item;
            player.GetModPlayer<OriginPlayer>().guardedHeart = true;
            //player.GetModPlayer<OriginPlayer>().coreMeltdown = true;
            player.longInvince = true;
			player.starCloakItem = Item;
			player.starCloakItem_starVeilOverrideItem = Item;
		}
	}
}
