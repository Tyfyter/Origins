using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Origins.NPCs;

namespace Origins.Buffs {
    public class Flagellash_Buff_0 : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            ID = Type;
        }
    }
    public class Flagellash_Buff_1 : Flagellash_Buff_0 {
        public static new int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            ID = Type;
        }
    }
    public class Flagellash_Buff_2 : Flagellash_Buff_0 {
        public static new int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            ID = Type;
        }
    }
}
