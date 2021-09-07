using Origins.Items.Materials;
using Terraria;
using Origins.Buffs;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Eyndum {
    [AutoloadEquip(EquipType.Head)]
    public class Eyndum_Helmet : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Eyndum Helmet");
        }
        public override void SetDefaults() {
            item.defense = 8;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Eyndum_Breastplate>() && legs.type == ModContent.ItemType<Eyndum_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.GetModPlayer<OriginPlayer>().eyndumSet = true;
            Origins.instance.SetEyndumCoreUI();
        }
        public override void AddRecipes() {
            /*ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15);
            //recipe.AddIngredient(ModContent.ItemType<>(), 10);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();*/
        }
    }
    [AutoloadEquip(EquipType.Body)]
    public class Eyndum_Breastplate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Eyndum Breastplate");
        }
        public override void SetDefaults() {
            item.defense = 16;
            item.wornArmor = true;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {

        }
        public override void AddRecipes() {
            /*ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25);
            //recipe.AddIngredient(ModContent.ItemType<>(), 20);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();*/
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class Eyndum_Greaves : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Eyndum Greaves");
        }
        public override void SetDefaults() {
            item.defense = 12;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {

        }
        public override void AddRecipes() {
            /*ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20);
            //recipe.AddIngredient(ModContent.ItemType<>(), 15);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();*/
        }
    }
}
