using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Olid_Organ : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Olid Organ");
            Tooltip.SetDefault("5% increased damage and critical strike chance\nAttacks inflict 'Toxic Shock' and 'Solvent' on enemies\nEnemies are less likely to target you");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaultsKeepSlots(ItemID.YoYoGlove);
            Item.rare = ItemRarityID.LightPurple;
            Item.value = Item.sellPrice(gold: 4);
        }
        public override void UpdateEquip(Player player) {
            player.aggro -= 150;
            player.GetDamage(DamageClass.Generic) *= 1.05f;
            player.GetCritChance(DamageClass.Generic) *= 1.05f;
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.decayingScale = true;
        }
        public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.PutridScent);
            recipe.AddIngredient(ModContent.ItemType<Decaying_Scale>());
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
