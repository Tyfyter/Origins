using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
    public class OriginGlobalRecipe : GlobalRecipe {
        public override bool RecipeAvailable(Recipe recipe) {
            OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
            if (recipe.createItem.type == ItemID.BottledWater) {
                return !originPlayer.ZoneBrine;
            }
            return true;
        }
    }
    public class Brine_Recipe : ModRecipe {
        public Brine_Recipe(Mod mod) : base(mod) { }
        public override bool RecipeAvailable() {
            return Main.LocalPlayer.GetModPlayer<OriginPlayer>().ZoneBrine;
        }
    }
}
