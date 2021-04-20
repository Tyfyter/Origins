using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
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
			//item.damage-=4;
			item.width = 12;
			item.height = 14;
			item.useTime-=6;
			item.useAnimation-=6;
            item.value*=4;
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
			projectile.penetrate = 7;
            projectile.hide = true;
            projectile.light = 0.025f;
        }
        public override void AI() {
            if (Main.rand.Next(200)==0){
	            Main.dust[Dust.NewDust(projectile.Center+new Vector2(6,6).RotatedBy(projectile.rotation), 0, 0, 204, 0f, 0f, 0, new Color(255,255,255), 1f)].velocity*=0.2f;
            }
            if(projectile.damage == 1) {
                Vector2 center = projectile.Center;// - new Vector2(0, 4).RotatedBy(projectile.rotation);
                double rot = Math.Round(projectile.rotation / MathHelper.PiOver4);
                Vector2 offset = new Vector2(0, 8).RotatedBy(rot*MathHelper.PiOver4);//new Vector2(4, 0).RotatedBy(projectile.rotation);
	            //Main.dust[Dust.NewDust(center-offset, 0, 0, 204, 0f, 0f, 0, new Color(255,255,255), 1f)].velocity*=0f;
	            //Main.dust[Dust.NewDust(center+offset, 0, 0, 204, 0f, 0f, 0, new Color(255,255,255), 1f)].velocity*=0f;
                if(Collision.CanHitLine(center-offset, 0, 0, center+offset, 0, 0)) {
	                //Main.dust[Dust.NewDust(center, 0, 0, 27, 0f, 0f, 0, new Color(255,0,0), 1f)].velocity*=0.2f;
                    Item.NewItem(projectile.Center, ModContent.ItemType<Fiberglass_Shard>());
                    projectile.Kill();
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(Main.rand.Next(projectile.penetrate)==0)return true;
            projectile.aiStyle = 0;
            projectile.hostile = true;
            projectile.friendly = true;
            projectile.damage = 1;
            projectile.position += new Vector2(2.5f, 2.5f);
            projectile.width = 15;
            projectile.height = 15;
            projectile.position -= new Vector2(7.5f, 7.5f);
            //projectile.position.X -= projectile.position.X % 4;
            //projectile.position.Y -= projectile.position.Y % 4;
            projectile.knockBack = 0;
            oldVelocity = oldVelocity.SafeNormalize(oldVelocity);
            projectile.position+=oldVelocity*12*(projectile.velocity.Length()/21f);
            bool exposed = true;
            int tries = 0;
            while(exposed) {
                projectile.position += oldVelocity;
                Vector2 center = projectile.Center;// - new Vector2(0, 4).RotatedBy(projectile.rotation);
                double rot = System.Math.Round(projectile.rotation / MathHelper.PiOver4);
                Vector2 offset = new Vector2(0, 8).RotatedBy(rot*MathHelper.PiOver4);
                exposed = ++tries<50&&Collision.CanHitLine(center - offset, 1, 1, center + offset, 1, 1);
            }
            projectile.velocity*=0;
            return false;
        }
        public override bool? CanHitNPC(NPC target) {
            return projectile.damage == 1?false:base.CanHitNPC(target);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
            if(projectile.damage == 1) {
                Item.NewItem(projectile.Center, ModContent.ItemType<Fiberglass_Shard>());
                damage = 0;
                target.statLife--;
                PlayerDeathReason reason = new PlayerDeathReason();
                reason.SourceCustomReason = target.name+" cut themselves on broken glass";
                if(target.statLife<=0)target.Hurt(reason, 1, 0);
                //target.immune = true;
                projectile.Kill();
            }
        }
        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI){
            if(projectile.hide)drawCacheProjsBehindNPCsAndTiles.Add(index);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0,0,10,14), lightColor, projectile.rotation, new Vector2(5, 14), 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
