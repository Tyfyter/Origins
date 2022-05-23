using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Origins.NPCs;

namespace Origins.Buffs {
    public class Mimic_Buff : ModBuff {
        public static int ID { get; private set; } = -1;
        public override void SetDefaults() {
            DisplayName.SetDefault("Mimic_Buff");
            Description.SetDefault("Increased combat stats");
            ID = Type;
        }
        public override bool Autoload(ref string name, ref string texture) {
            texture = "Terraria/Buff_160";
            return true;
        }
		public override void Update(Player player, ref int buffIndex) {
            player.GetModPlayer<OriginPlayer>().fervorPotion = true;
            player.allDamageMult *= 1.15f;
            player.meleeCrit += 15;
            player.rangedCrit += 15;
            player.magicCrit += 15;
            player.lifeRegenCount += 20;
        }
	}
}
