using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Accessories {
    public class Mad_Hand : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Madman’s Hand");
            Tooltip.SetDefault("‘Take my hand, and give them a slap in the face.’");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 22;
            Item.height = 20;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().madHand = true;
            player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed+=0.75f;
        }
    }
}
