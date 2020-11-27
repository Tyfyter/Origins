using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Projectiles {
    public class OriginGlobalProj : GlobalProjectile {
        public override bool InstancePerEntity => true;
        public override bool CloneNewInstances => true;
        //bool init = true;
        bool felnumEffect = false;
        public static bool felnumEffectNext = false;
        public static bool hostileNext = false;
        public override void SetDefaults(Projectile projectile) {
            if(hostileNext) {
                projectile.hostile = true;
                hostileNext = false;
            }
            felnumEffect = felnumEffectNext;
            felnumEffectNext = false;
        }
        public override void AI(Projectile projectile) {
            if(felnumEffect) {
                if(projectile.melee) {
                    if(Main.player[projectile.owner].GetModPlayer<OriginPlayer>().felnumShock>19)Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1)*0.5f, Scale:0.5f);
                }else Dust.NewDustPerfect(projectile.Center, 226, projectile.velocity.RotatedByRandom(0.1)*0.5f, Scale:0.5f);
            }
        }
    }
}
