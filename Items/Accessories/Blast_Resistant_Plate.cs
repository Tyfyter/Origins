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
    public class Blast_Resistant_Plate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Blast Resistant Plate");
            Tooltip.SetDefault("Reduces explosive self-damage by 20%");
        }
        public override void SetDefaults() {
            item.accessory = true;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage-=0.2f;
        }
    }
}
