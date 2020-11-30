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
    public class Bomb_Yeeter : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Bomb Handling Device");
            Tooltip.SetDefault("Also commonly referred to as the 'Bomb Yeeter'");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.YoYoGlove);
            item.handOffSlot = -1;//the current spritesheet is in 1.4's format, so it's not usable yet
            item.handOnSlot = -1;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().bombHandlingDevice = true;
        }
    }
}
