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
    public class Rasterize : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Freeze Frame");
            Tooltip.SetDefault("Attacks may inflict \"Rasterize\"");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.width = 22;
            Item.height = 20;
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().rasterize = true;
        }
    }
}
