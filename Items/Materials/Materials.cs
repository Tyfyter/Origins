using Origins.Tiles;
using Origins.Tiles.Defiled;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
    public class Angelium : ModItem {
        //add lore here
        public override void SetDefaults() {
            item.maxStack = 999;
        }
    }
    public class Bark : ModItem {
        public override void SetDefaults() {
            item.maxStack = 999;
        }
    }
    public class Bat_Hide : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bat Hide");
            //Tooltip.SetDefault();
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(this, 4);
            recipe.AddTile(TileID.HeavyWorkBench);
            recipe.SetResult(ItemID.Leather);
            recipe.AddRecipe();
        }
    }
    public class Ember_Onyx : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ember Onyx");
        }
    }
    public class Rubber : ModItem {
        public override void SetDefaults() {
            item.maxStack = 999;
        }
    }
    public class Silicon_Wafer : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Silicon Packet");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SandBlock, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class Tree_Sap : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sap");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
    }
    public class Viridium_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Viridium Bar");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
    }
    public class Peat_Moss : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Peat Moss");
            Tooltip.SetDefault("The demolitionist might find this interesting");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
            item.value = 200;//2 silver
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
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Ore_Item>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class Valkyrum_Bar : ModItem {
        //Alloy of Felnum and Angelium
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Valkyrum Bar");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 1);
            //recipe.AddIngredient(ModContent.ItemType<_Bar>(), 1);
            recipe.AddIngredient(ItemID.Ectoplasm, 1);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class Defiled_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Bar");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
    public class Undead_Chunk : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Undead Chunk");
        }
        public override void SetDefaults() {
            item.maxStack = 99;
        }
    }
    public class Infested_Bar : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Infested Bar");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
        /*public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Defiled_Ore_Item>(), 3);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }*/
    }
    public class Riven_Sample : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Sample");
        }
        public override void SetDefaults() {
            item.maxStack = 99;
        }
    }
    public class Wilting_Rose_Item : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Wilting Rose");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
    }
    public class Defiled_Key : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Key");
        }
        public override void SetDefaults() {
			item.width = 14;
			item.height = 20;
			item.maxStack = 99;
        }
    }
    public class Riven_Key : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Key");
        }
        public override void SetDefaults() {
            item.width = 14;
            item.height = 20;
            item.maxStack = 99;
        }
    }
    public class Brine_Sample : ModItem {
        //add lore here
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Brine Sample");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.ShadowScale);
        }
        public override void AddRecipes() {
            Brine_Recipe recipe = new Brine_Recipe(mod);
            recipe.AddIngredient(ItemID.Bottle);
            recipe.needWater = true;
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}