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
    public class Decaying_Scale : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Decaying Scale");
            Tooltip.SetDefault("Attacks inflict Toxic Shock and Solvent on enemies\nEffects are stronger while Acrid Armor is equipped");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
        }
        public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.decayingScale = true;
        }
    }
}
