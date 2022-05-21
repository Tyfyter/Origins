using Origins.Items.Materials;
using Terraria;
using Origins.Buffs;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Mimic {
    [AutoloadEquip(EquipType.Head)]
    public class Mimic_Helmet : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Mimic Helmet");
            Tooltip.SetDefault("Slightly increased explosive velocity");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddHelmetGlowmask(item.headSlot, "Items/Armor/Mimic/Mimic_Helmet_Head_Glow");
            }
        }
        public override void SetDefaults() {
            item.defense = 8;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed+=0.1f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Mimic_Breastplate>() && legs.type == ModContent.ItemType<Mimic_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Not yet implemented";
            player.GetModPlayer<OriginPlayer>().mimicSet = true;
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
    public class Mimic_Breastplate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Mimic Breastplate");
            Tooltip.SetDefault("Increased life regeneration");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddBreastplateGlowmask(item.bodySlot, "Items/Armor/Mimic/Mimic_Breastplate_Body_Glow");
                Origins.AddBreastplateGlowmask(-item.bodySlot, "Items/Armor/Mimic/Mimic_Breastplate_FemaleBody_Glow");
            }
        }
        public override void SetDefaults() {
            item.defense = 16;
            item.wornArmor = true;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.lifeRegenCount+=20;
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
    public class Mimic_Greaves : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Mimic Greaves");
            Tooltip.SetDefault("10% increased explosive damage");
            if (Main.netMode != NetmodeID.Server) {
                //Origins.AddLeggingGlowMask(item.legSlot, "Items/Armor/Mimic/Mimic_Greaves_Legs_Glow");
            }
        }
        public override void SetDefaults() {
            item.defense = 12;
            item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveDamage += 0.1f;
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
