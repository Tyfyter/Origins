using Origins.Items.Materials;
using Terraria;
using Origins.Buffs;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Defiled {
    [AutoloadEquip(EquipType.Head)]
    public class Defiled_Helmet : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Helmet");
            Tooltip.SetDefault("Increases mana regeneration rate");
        }
        public override void SetDefaults() {
            item.defense = 6;
            item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.manaRegen+=2;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Defiled_Breastplate>() && legs.type == ModContent.ItemType<Defiled_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "15% of damage is redirected to mana";
            player.GetModPlayer<OriginPlayer>().defiledSet = true;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15);
            recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 10);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();
        }
    }
    [AutoloadEquip(EquipType.Body)]
    public class Defiled_Breastplate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Breastplate");
            Tooltip.SetDefault("5% increased magic damage");
        }
        public override void SetDefaults() {
            item.defense = 7;
            item.wornArmor = true;
            item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.magicDamage+=0.05f;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25);
            recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 20);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class Defiled_Greaves : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Greaves");
            Tooltip.SetDefault("5% increased movement speed");
        }
        public override void SetDefaults() {
            item.defense = 6;
            item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.moveSpeed+=0.05f;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20);
            recipe.AddIngredient(ModContent.ItemType<Undead_Chunk>(), 15);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();
        }
    }
}
namespace Origins.Buffs {
    public class Defiled_Exhaustion_Buff : ModBuff {
        public override void SetDefaults() {
            DisplayName.SetDefault("Defiled Exhaustion");
        }
        public override void Update(Player player, ref int buffIndex) {
            player.manaRegenBuff = false;
            player.manaRegen = 0;
            player.manaRegenCount = 0;
            player.manaRegenBonus = 0;
        }
    }
}
