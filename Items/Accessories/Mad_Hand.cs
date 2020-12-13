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
    public class Mad_Hand : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Madman’s Hand");
            Tooltip.SetDefault("‘Take my hand, and give them a slap in the face.’");
        }
        public override void SetDefaults() {
            item.accessory = true;
            item.width = 22;
            item.height = 20;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().madHand = true;
            player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed+=0.75f;
        }
    }
}
