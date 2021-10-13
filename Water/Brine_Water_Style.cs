using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Water {
    public class Brine_Water_Style : ModWaterStyle {
        public override bool ChooseWaterStyle() => Main.LocalPlayer.GetModPlayer<OriginPlayer>().ZoneBrine;
        public override int ChooseWaterfallStyle() => mod.GetWaterfallStyleSlot<Brine_Waterfall_Style>();
        public override int GetDropletGore() => GoreID.ChimneySmoke1 + Main.rand.Next(3);
        public override int GetSplashDust() => 99;
        public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
            r = 0.5f;
            g = 0.9f;
            b = 0.83f;
        }
        public override Color BiomeHairColor() => new Color(20, 102, 87);
    }
    public class Brine_Waterfall_Style : ModWaterfallStyle {}
}
