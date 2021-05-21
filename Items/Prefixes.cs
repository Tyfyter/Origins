using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Origins.Items {
    public class Heavy_Cal_Prefix : ModPrefix {
        public override PrefixCategory Category => PrefixCategory.Ranged;
        public override void SetDefaults() {
            DisplayName.SetDefault("Heavy Caliber");
        }
        public override void ModifyValue(ref float valueMult) {
            int a = Type;
            valueMult = 1.5f;
        }
        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
            damageMult += 0.20f;// dmg
            critBonus += 10;// crit
            shootSpeedMult -= 0.15f;// vel
            knockbackMult += 0.20f;// kb
        }
    }
}
