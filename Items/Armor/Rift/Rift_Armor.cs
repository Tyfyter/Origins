using Origins.Items.Materials;
using Terraria;
using Origins.Buffs;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Rift {
    [AutoloadEquip(EquipType.Head)]
    public class Rift_Helmet : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rift Helmet");
            Tooltip.SetDefault("Increased explosive velocity");
        }
        public override void SetDefaults() {
            item.defense = 8;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed+=0.2f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Rift_Breastplate>() && legs.type == ModContent.ItemType<Rift_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Explosive projectiles have a chance to be duplicated";
            player.GetModPlayer<OriginPlayer>().riftSet = true;
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
    public class Rift_Breastplate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rift Breastplate");
            Tooltip.SetDefault("5% increased magic damage");
        }
        public override void SetDefaults() {
            item.defense = 16;
            item.wornArmor = true;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.statLifeMax2+=20;
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
    public class Rift_Greaves : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rift Greaves");
            Tooltip.SetDefault("20% increased movement speed");
        }
        public override void SetDefaults() {
            item.defense = 12;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.moveSpeed += 0.2f;
            player.runAcceleration += 0.02f;
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
