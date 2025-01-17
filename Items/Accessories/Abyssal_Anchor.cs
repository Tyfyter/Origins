using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
    public class Abyssal_Anchor : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"ManaShielding",
			"MagicBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 12);
			Item.rare = ItemRarityID.Yellow;
		}
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Binding_Book>())
            .AddIngredient(ModContent.ItemType<Celestial_Stone_Mask>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
        public override void UpdateAccessory(Player player, bool hideVisual) {
			player.manaMagnet = true;
			player.magicCuffs = true;
			player.statManaMax2 += 20;
            player.GetModPlayer<OriginPlayer>().manaShielding += 0.25f;

            player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
			player.GetDamage(DamageClass.Generic) += 0.1f;
			player.GetCritChance(DamageClass.Generic) += 4f;
			player.lifeRegen += 4;
			player.statDefense += 8;
			player.pickSpeed -= 0.25f;
			player.GetKnockback(DamageClass.Summon).Base += 1;

			player.moveSpeed *= 0.75f;
			player.jumpSpeedBoost -= 2.2f;
		}
	}
}
