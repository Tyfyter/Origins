using Origins.Items.Materials;
using Terraria;
using Origins.Buffs;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Armor.Rift {
    [AutoloadEquip(EquipType.Head)]
    public class Rift_Helmet : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rift Helmet");
            Tooltip.SetDefault("Increased explosive velocity");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Rift/Rift_Helmet_Head_Glow");
            }
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.defense = 8;
            Item.rare = ItemRarityID.Pink;
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
            /*Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 15);
            //recipe.AddIngredient(ModContent.ItemType<>(), 10);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();*/
        }
    }
    [AutoloadEquip(EquipType.Body)]
    public class Rift_Breastplate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rift Breastplate");
            Tooltip.SetDefault("Increases maximum health by 20");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Rift/Rift_Breastplate_Body_Glow");
                Origins.AddBreastplateGlowmask(-Item.bodySlot, "Items/Armor/Rift/Rift_Breastplate_FemaleBody_Glow");
            }
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.defense = 16;
            Item.wornArmor = true;
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.statLifeMax2+=20;
        }
        public override void AddRecipes() {
            /*Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 25);
            //recipe.AddIngredient(ModContent.ItemType<>(), 20);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();*/
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class Rift_Greaves : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rift Greaves");
            Tooltip.SetDefault("20% increased movement speed");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddLeggingGlowMask(Item.legSlot, "Items/Armor/Rift/Rift_Greaves_Legs_Glow");
            }
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.defense = 12;
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.moveSpeed += 0.2f;
            player.runAcceleration += 0.02f;
        }
        public override void AddRecipes() {
            /*Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 20);
            //recipe.AddIngredient(ModContent.ItemType<>(), 15);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();*/
        }
    }
}
