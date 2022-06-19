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
            DisplayName.SetDefault("Bleeding Obsidian Maniac Mask");
            Tooltip.SetDefault("Increased explosive velocity");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Rift/Rift_Helmet_Head_Glow");
            }
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.defense = 9;
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
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.ObsidianHelm);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 12);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Body)]
    public class Rift_Breastplate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bleeding Obsidian Trenchcoat");
            Tooltip.SetDefault("-25% explosive self-damage");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddBreastplateGlowmask(Item.bodySlot, "Items/Armor/Rift/Rift_Breastplate_Body_Glow");
                Origins.AddBreastplateGlowmask(-Item.bodySlot, "Items/Armor/Rift/Rift_Breastplate_FemaleBody_Glow");
            }
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.defense = 13;
            Item.wornArmor = true;
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage-=0.25f;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.ObsidianShirt);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 36);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class Rift_Greaves : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bleeding Obsidian Chaps");
            Tooltip.SetDefault("15% increased movement speed");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddLeggingGlowMask(Item.legSlot, "Items/Armor/Rift/Rift_Greaves_Legs_Glow");
            }
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.defense = 11;
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            player.moveSpeed += 0.15f;
            player.runAcceleration += 0.02f;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.ObsidianPants);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 24);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
