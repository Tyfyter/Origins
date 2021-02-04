using Origins.Items.Materials;
using Terraria;
using Origins.Buffs;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Riven {
    [AutoloadEquip(EquipType.Head)]
    public class Riven_Mask : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Mask");
            Tooltip.SetDefault("Increases minion damage by 10%");
        }
        public override void SetDefaults() {
            item.defense = 6;
            item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.minionDamage+=0.1f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Riven_Coat>() && legs.type == ModContent.ItemType<Riven_Pants>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Increases minion damage by up to 30% when over half health";
            player.GetModPlayer<OriginPlayer>().rivenSet = true;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15);
            //recipe.AddIngredient(ModContent.ItemType<>(), 10);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();
        }
    }
    [AutoloadEquip(EquipType.Body)]
    public class Riven_Coat : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Coat");
            Tooltip.SetDefault("Increases your max number of minions by 1");
        }
        public override void SetDefaults() {
            item.defense = 7;
            item.wornArmor = true;
            item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.maxMinions++;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25);
            //recipe.AddIngredient(ModContent.ItemType<>(), 20);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class Riven_Pants : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Pants");
            Tooltip.SetDefault("Increases jump height");
        }
        public override void SetDefaults() {
            item.defense = 6;
            item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.jumpSpeedBoost+=1f;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20);
            //recipe.AddIngredient(ModContent.ItemType<>(), 15);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();
        }
    }
}
