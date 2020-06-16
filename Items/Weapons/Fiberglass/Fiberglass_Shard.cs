using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Fiberglass {
	public class Fiberglass_Shard : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Shard");
			Tooltip.SetDefault("Be careful, it's sharp");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.ThrowingKnife);
			item.damage+=6;
			item.width = 12;
			item.height = 14;
			item.useTime-=5;
			item.useAnimation-=5;
            item.value*=5;
			item.shootSpeed+=4;
			item.autoReuse = true;
            item.shoot = ModContent.ProjectileType<Fiberglass_Shard_Proj>();
		}
    }
    public class Fiberglass_Shard_Proj : ModProjectile {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Shard");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.PoisonDart);
            projectile.hostile = false;
            projectile.friendly = true;
            projectile.trap = false;
			projectile.width = 5;
			projectile.height = 5;
            projectile.extraUpdates = 1;
            projectile.hide = true;
            projectile.light = 0;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.position+=oldVelocity.SafeNormalize(oldVelocity)*12*(projectile.velocity.Length()/21f);
            projectile.velocity*=0;
            projectile.aiStyle = 0;
            projectile.hostile = true;
            projectile.friendly = true;
            projectile.damage = 1;
            projectile.width = 15;
            projectile.height = 15;
            return Main.rand.Next(7)==0;
        }
        public override bool? CanHitNPC(NPC target) {
            return projectile.damage == 1?false:base.CanHitNPC(target);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
            if(projectile.damage == 1) {
                Item.NewItem(projectile.Center, ModContent.ItemType<Fiberglass_Shard>());
                damage = 0;
                target.immune = true;
                projectile.Kill();
            }
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI){
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
    }
}
