using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;

namespace Origins.Items {
    public class Heavy_Cal_Prefix : ModPrefix {
		public override PrefixCategory Category => PrefixCategory.Ranged;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Heavy Caliber");
        }
        public override float RollChance(Item item) {
            return 1f;
        }
        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus) {
            damageMult += 0.20f;// damage
            critBonus += 10;// crit
            shootSpeedMult -= 0.15f;// velocity
            knockbackMult += 0.20f;// knockback
        }
    }
}
