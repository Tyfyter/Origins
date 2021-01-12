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
    public class Cleaver_Rifle : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cleaver Rifle");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Gatligator);
            item.useAnimation = item.useTime = 5;
            item.width = 106;
            item.height = 32;
            item.scale = 0.7f;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-18, 0);
    }
}
