using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Origins.Items.Weapons.Explosives;

namespace Origins.Items.Weapons.Ammo {
    public class Thermite_Canister : ModItem {
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.RocketI);
            item.damage = 30;
            item.shoot = ModContent.ProjectileType<Thermite_Canister_P>();
            item.ammo = item.type;
        }
    }
}
