using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Origins {
    [Label("Settings")]
    public class OriginConfig : ModConfig {
        public static OriginConfig Instance;
        public override ConfigScope Mode => ConfigScope.ServerSide;
        [Header("Vanilla Buffs")]

        [Label("Infected Wood Items")]
        [DefaultValue(true)]
        public bool WoodBuffs = true;
    }
}
