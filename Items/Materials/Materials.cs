using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
    public class Angelium : ModItem {
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
            DisplayName.SetDefault("Silicon Wafer");
        }
        public override void SetDefaults() {
            item.maxStack = 999;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SandBlock, 3);
            recipe.AddTile(TileID.GlassKiln);
            recipe.AddTile(TileID.AlchemyTable);
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
}
