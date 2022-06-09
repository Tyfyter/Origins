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
    public class Dim_Starlight : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Dim Starlight");
            Tooltip.SetDefault("Spawns mana stars on crits");
            glowmask = Origins.AddGlowMask(this);
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.glowMask = glowmask;
        }
        public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.dimStarlight = true;
            float light = 0.1f+(originPlayer.dimStarlightCooldown/1000f);
            Lighting.AddLight(player.Center,light,light,light);
        }
    }
}
