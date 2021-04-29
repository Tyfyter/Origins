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
using Origins.World;
using Origins.Projectiles;
using Origins.Dusts;

namespace Origins.Items.Weapons.Ammo {
    public class Thermite_Canister : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Thermite Canister");
            //Tooltip.SetDefault();
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.RocketI);
            item.damage = 30;
            item.shoot = ModContent.ProjectileType<Thermite_Canister_P>();
            item.ammo = item.type;
        }
    }
    public class White_Solution : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("White Solution");
            //Tooltip.SetDefault();
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.GreenSolution);
            item.shoot = ModContent.ProjectileType<White_Solution_P>()-ProjectileID.PureSpray;
        }
    }
    public class White_Solution_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.PureSpray);
            projectile.aiStyle = 0;
        }
        public override void AI() {
            OriginGlobalProj.ClentaminatorAI(projectile, OriginWorld.origin_conversion_type, ModContent.DustType<Solution_D>(), Color.GhostWhite);
        }
    }
}
