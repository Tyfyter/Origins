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
using Terraria.GameContent.Creative;

namespace Origins.Items.Accessories {
    public class Exploder_Emblem : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Exploder Emblem");
            Tooltip.SetDefault("+15% explosive damage");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 28;
            Item.height = 28;
        }
        public override void UpdateEquip(Player player) {
            //player.GetModPlayer<OriginPlayer>().explosiveDamage -= 0.2f;
        }
    }
}
