using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
    public class Angelium : ModItem {}
    public class Bark : ModItem {}
    public class Bat_Hide : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bat Hide");
            //Tooltip.SetDefault();
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
    public class Rubber : ModItem {}
    public class Silicon_Wafer : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Silicon Wafer");
        }
    }
    public class Tree_Sap : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Sap");
        }
    }
    public class Viridium_Bar : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Viridium Bar");
        }
    }
}
