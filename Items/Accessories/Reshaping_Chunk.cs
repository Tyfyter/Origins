using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Reshaping_Chunk : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Reshaping Chunk");
            Tooltip.SetDefault("Strengthens the set bonus of Defiled Armor\nReduces damage taken by 5% if Defiled Armor is not equipped");
			Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(9, 3));
        }
        public override void SetDefaults() {
            item.accessory = true;
            item.width = 22;
            item.height = 20;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().reshapingChunk = true;
        }
    }
}
