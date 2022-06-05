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
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fervor");
            Description.SetDefault("10% increased attack speed");
            ID = Type;
        }
		public override void Update(Player player, ref int buffIndex) {
            player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
		}
	}
}
