using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Origins.NPCs;

namespace Origins.Buffs {
    public class Fervor_Buff : ModBuff {
        public static int ID { get; private set; } = -1;
        public override void SetDefaults() {
            DisplayName.SetDefault("Fervor");
            Description.SetDefault("10% increased attack speed");
            ID = Type;
        }
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_160";
            return true;
        }
		public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<OriginPlayer>().fervorPotion = true;
		}
	}
}
