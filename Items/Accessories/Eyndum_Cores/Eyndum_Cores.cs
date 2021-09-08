using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Items.Accessories.Eyndum_Cores {
    public abstract class Eyndum_Core : ModItem {
        public abstract Color CoreGlowColor { get; }
    }
    public class Agility_Core : Eyndum_Core {
        public override Color CoreGlowColor => new Color(255, 220, 0, 160);
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Agility Core");
        }
        public override void UpdateEquip(Player player) {
            player.wingTimeMax *= 2;
            player.moveSpeed *= 1.5f;
            player.runAcceleration *= 1.5f;
            player.maxRunSpeed *= 1.5f;
            player.jumpSpeedBoost += 5;
        }
    }
    public class Combat_Core : Eyndum_Core {
        public override Color CoreGlowColor => new Color(160, 0, 255, 160);
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Combat Core");
        }
        public override void UpdateEquip(Player player) {
        }
    }
    public class Construction_Core : Eyndum_Core {
        public override Color CoreGlowColor => new Color(255, 160, 0, 160);
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Construction Core");
        }
        public override void UpdateEquip(Player player) {
        }
    }
    public class Lifeforce_Core : Eyndum_Core {
        public override Color CoreGlowColor => new Color(255, 0, 75, 160);
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Lifeforce Core");
        }
        public override void UpdateEquip(Player player) {
            player.statLifeMax2 += player.statLifeMax2 / 2;
        }
    }
}
