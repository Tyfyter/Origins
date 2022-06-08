using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Origins.Items.Weapons.Other {
    //implemented in 10 minutes, so it might have an issue or two
    public class Cleaver_Rifle : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cleaver Rifle");
            Tooltip.SetDefault("Crude and dangerous");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Gatligator);
            Item.damage = 39;
            Item.useAnimation = Item.useTime = 10;
            Item.shootSpeed*=2;
            Item.width = 106;
            Item.height = 32;
            Item.scale = 0.7f;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-18, 0);
    }
}
