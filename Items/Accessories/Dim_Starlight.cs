using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Dim_Starlight : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Dim Starlight");
            Tooltip.SetDefault("Spawns mana stars on crits");
        }
        public override void SetDefaults() {
            item.accessory = true;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().dimStarlight = true;
            Lighting.AddLight(player.Center,0.1f,0.1f,0.1f);
        }
    }
}
