using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.Tiles.Dusk;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
    public class Acid_Bottle : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Acid Bottle");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 30;
        }
        public override void SetDefaults() {
            Item.maxStack = 1;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.Bottle);
            recipe.AddIngredient(ItemID.Stinger, 2);
            recipe.AddIngredient(ModContent.ItemType<Brine_Sample>(), 1);
            recipe.AddTile(TileID.AlchemyTable);
            recipe.Register();
        }
    }
    public class Acrid_Bar : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Acrid Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.TitaniumBar, 1);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 10);
            recipe.AddIngredient(ModContent.ItemType<Acid_Bottle>(), 3);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
            recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.AdamantiteBar, 1);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 10);
            recipe.AddIngredient(ModContent.ItemType<Acid_Bottle>(), 3);
            recipe.AddTile(TileID.DemonAltar);
            recipe.Register();
        }
    }
    public class Adhesive_Wrap : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Adhesive Wrap");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Tree_Sap>(), 3);
            recipe.AddIngredient(ModContent.ItemType<Silicon_Wafer>(), 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
    }
	public class Angelium : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        //add lore here
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Bark : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(ModContent.ItemType<Rubber>());
            recipe.AddIngredient(this, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();
        }
    }
    public class Bat_Hide : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bat Hide");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(ItemID.Leather);
            recipe.AddIngredient(this, 4);
            recipe.AddTile(TileID.HeavyWorkBench);
            recipe.Register();
        }
    }
    public class Bleeding_Obsidian_Shard : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bleeding Obsidian Shard");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 48;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(ModContent.ItemType<Bleeding_Obsidian_Item>());
            recipe.AddIngredient(this, 6);
            recipe.Register();
            recipe = Mod.CreateRecipe(Type, 6);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>());
            recipe.Register();
        }
    }
    public class Brine_Sample : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Brine Sample");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.Bottle);
            recipe.AddCondition(
                Terraria.Localization.NetworkText.FromLiteral("Brine"),
                (_) => Main.LocalPlayer.adjWater && Main.LocalPlayer.GetModPlayer<OriginPlayer>().ZoneBrine
            );
            recipe.Register();
            recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddIngredient(ModContent.ItemType<Sulphur_Stone_Item>()); //Forgot to implement Decaying Mush...
            recipe.AddTile(TileID.AlchemyTable);
            recipe.Register();
        }
    }
    public class Busted_Servo : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Busted Servo");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Conductor_Rod : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Conductor Rod");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.CopperBar, 3);
            recipe.AddTile(TileID.Anvils); //Fabricator not implemented yet
            recipe.Register();
        }
    }
    public class Chromtain_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Chromtain Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddIngredient(ItemID.FragmentSolar, 4);
            recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 4);
            recipe.AddTile(TileID.Anvils); //Omni-Printer also not implemented
            recipe.Register();
        }
    }
    public class Defiled_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.Register();
        }
    }
    public class Defiled_Key : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Key");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
			Item.width = 14;
			Item.height = 20;
			Item.maxStack = 99;
        }
    }
    public class Element36_Bundle : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Element-36 Bundle");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.FragmentNebula, 2);
            recipe.AddIngredient(ItemID.FragmentStardust, 2);
            recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 4);
            recipe.AddIngredient(ModContent.ItemType<Fibron_Plating>(), 4);
            recipe.AddTile(TileID.Anvils); //Omni-Printer also not implemented
            recipe.Register();
        }
    }
    public class Ember_Onyx : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ember Onyx");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Eyndum_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Eyndum Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.GoldBar, 2);
            recipe.AddIngredient(ItemID.FragmentVortex, 4);
            recipe.AddIngredient(ModContent.ItemType<Void_Spark>(), 6);
            recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 4);
            recipe.AddTile(TileID.Anvils); //Omni-Printer also not implemented, still maybe a unique forge and dimension
            recipe.Register();
        }
    }
    public class Felnum_Bar : ModItem {
        /*
         * brown color in its natural form
         * tinted silver color when hardened
         * exhibits a property named "electrical greed" where it grows hard blue crystals from anywhere it would lose electrons, to effectively "reclaim" them, even if it already has a strong negative charge
         */
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Felnum Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Ore_Item>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.Register();
        }
    }
    public class Fibron_Plating : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fibron Plating");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Formium_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Formium Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Formium_Scrap>(), 6);
            recipe.Register();
        }
    }
    public class Formium_Scrap : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Formium Scrap");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Infested_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Infested Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Infested_Ore_Item>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.Register();
        }
    }
    public class Lunar_Token : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Lunar Token");
            Tooltip.SetDefault("Valuable to the demented.");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Shaping_Matter : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Meta Gel");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 99;
        }
    }
    public class Modular_Plating : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Modular Plating");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.HallowedBar, 6);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Peat_Moss : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Peat Moss");
            Tooltip.SetDefault("The Demolitionist might find this interesting...");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
            Item.value = 300;//3 silver
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(ItemID.ExplosivePowder);
            recipe.AddIngredient(this, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();
        }
    }
    public class Power_Core : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Power Core");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
        }
        public override void SetDefaults() {
            Item.maxStack = 99;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.HallowedBar, 4);
            recipe.AddIngredient(ModContent.ItemType<Acrid_Bar>(), 8);
            recipe.AddIngredient(ModContent.ItemType<Conductor_Rod>(), 2);
            recipe.AddTile(TileID.Anvils); //Fabricator not implemented yet
            recipe.Register();
        }
    }
    public class Rivenform : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 99;
        }
    }
    public class Riven_Key : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Key");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.width = 14;
            Item.height = 20;
            Item.maxStack = 99;
        }
    }
    public class Riven_Sample : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Sample");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 99;
        }
    }
    public class Rotor : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type, 5);
            recipe.AddIngredient(ModContent.ItemType<Silicon_Wafer>(), 2);
            recipe.AddIngredient(ModContent.ItemType<Conductor_Rod>(), 2);
            recipe.AddIngredient(ItemID.HallowedBar, 2);
            recipe.AddTile(TileID.Anvils); //Fabricator not implemented yet
            recipe.Register();
        }
    }
    public class Rubber : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Silicon_Wafer : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Silicon Packet");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.SandBlock, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();
        }
    }
    public class Space_Goo : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Space Goo");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Space_Rock : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Space Rock");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
    public class Stellar_Spark : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Stellar Spark");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.FallenStar, 12);
            recipe.AddIngredient(ItemID.FragmentSolar, 2);
            recipe.AddIngredient(ItemID.FragmentStardust, 2);
            recipe.AddIngredient(ModContent.ItemType<Lunar_Token>());
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Strange_String : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Strange String");
            Tooltip.SetDefault("'Involuntary neurectomy'");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        //add lore here
        public override void SetDefaults() {
            Item.maxStack = 99;
        }
    }
    public class Superconductor : ModItem {
        public override void SetStaticDefaults() {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 99;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type, 3);
            recipe.AddIngredient(ItemID.GoldBar, 7);
            recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 3);
            recipe.AddIngredient(ModContent.ItemType<Conductor_Rod>(), 5);
            recipe.AddIngredient(ModContent.ItemType<Rubber>(), 20);
            recipe.AddTile(TileID.Anvils); //No Omni-Printer
            recipe.Register();
        }
    }
    public class Thruster_Component : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Thruster Component");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type, 2);
            recipe.AddIngredient(ModContent.ItemType<Fibron_Plating>(), 8);
            recipe.AddIngredient(ModContent.ItemType<Space_Goo>(), 4);
            recipe.AddTile(TileID.Anvils); //No Omni-Printer
            recipe.Register();
        }
    }
    public class Tree_Sap : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Tree Sap");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(ModContent.ItemType<Rubber>());
            recipe.AddIngredient(this, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.Register();
        }
    }
    public class Undead_Chunk : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Undead Chunk");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 99;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(ItemID.ObsidianHelm);
            recipe.AddIngredient(ItemID.Silk, 10);
            recipe.AddIngredient(ItemID.Obsidian, 20);
            recipe.AddIngredient(this, 5);
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
            recipe = Mod.CreateRecipe(ItemID.ObsidianShirt);
            recipe.AddIngredient(ItemID.Silk, 10);
            recipe.AddIngredient(ItemID.Obsidian, 20);
            recipe.AddIngredient(this, 10);
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
            recipe = Mod.CreateRecipe(ItemID.ObsidianPants);
            recipe.AddIngredient(ItemID.Silk, 10);
            recipe.AddIngredient(ItemID.Obsidian, 20);
            recipe.AddIngredient(this, 5);
            recipe.AddTile(TileID.Hellforge);
            recipe.Register();
        }
    }
    public class Unpowered_Eyndum_Core : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Unpowered Eyndum Core");
            Tooltip.SetDefault("'Limitless potential'");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 2;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 8;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Superconductor>(), 10);
            recipe.AddIngredient(ModContent.ItemType<Eyndum_Bar>(), 8);
            recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 4);
            recipe.AddTile(TileID.Anvils); //No Omni-Printer
            recipe.Register();
        }
    }
    public class Valkyrum_Bar : ModItem {
        //Alloy of Felnum and Angelium
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Valkyrum Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 1);
            //recipe.AddIngredient(ModContent.ItemType<_Bar>(), 1);
            recipe.AddIngredient(ItemID.Ectoplasm, 1);
            recipe.AddTile(TileID.Furnaces);
            recipe.Register();
        }
    }
    public class Viridium_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Viridium Bar");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.ChlorophyteOre, 3); //Need Taranum
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Void_Spark : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Void Spark");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
            
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ItemID.FragmentVortex, 4);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 12);
            recipe.AddIngredient(ModContent.ItemType<Lunar_Token>());
            recipe.AddTile(TileID.Anvils); //You guessed it, no Omni-Printer
            recipe.Register();
        }
    }
    public class Wilting_Rose_Item : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Wilting Rose");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
        }
        public override void SetDefaults() {
            Item.maxStack = 999;
        }
    }
}