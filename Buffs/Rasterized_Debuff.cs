using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Origins.NPCs;

namespace Origins.Buffs {
    public class Rasterized_Debuff : ModBuff {
        public static int ID { get; private set; } = -1;
        public override void SetDefaults() {
            DisplayName.SetDefault("Solvent");
            ID = Type;
        }
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_160";
            return true;
        }
    }
}
