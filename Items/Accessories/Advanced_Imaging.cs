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
    public class Advanced_Imaging : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Advanced Imaging");
            Tooltip.SetDefault("Increased projectile speed\nImmunity to Confusion\n\"The future is now.\"");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.YoYoGlove);
            item.handOffSlot = -1;//the current spritesheet is in 1.4's format, so it's not usable yet
            item.handOnSlot = -1;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().advancedImaging = true;
            player.buffImmune[BuffID.Confused] = true;
        }
    }
}
