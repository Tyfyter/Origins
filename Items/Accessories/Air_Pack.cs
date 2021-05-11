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
    public class Air_Pack : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Air Pack");
            Tooltip.SetDefault("Excessively extends underwater breathing\nImmunity to ‘Suffocation’");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.YoYoGlove);
            item.handOffSlot = -1;
            item.handOnSlot = -1;
        }
        public override void UpdateEquip(Player player) {
            player.buffImmune[BuffID.Suffocation] = true;
            player.breathMax+=560;
        }
    }
}
