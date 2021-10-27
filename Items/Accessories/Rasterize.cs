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
    public class Rasterize : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rasterizer");
            Tooltip.SetDefault("Attacks may inflict \"Rasterize\"");
        }
        public override void SetDefaults() {
            item.accessory = true;
            item.width = 22;
            item.height = 20;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().rasterize = true;
        }
    }
}
