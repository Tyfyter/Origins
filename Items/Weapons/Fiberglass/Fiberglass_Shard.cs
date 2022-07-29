using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Fiberglass {
	public class Fiberglass_Shard : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Shard");
			Tooltip.SetDefault("Be careful, it's sharp");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ThrowingKnife);
			//item.damage-=4;
			Item.width = 12;
			Item.height = 14;
			Item.useTime-=6;
			Item.useAnimation-=6;
            Item.value*=4;
			Item.shootSpeed+=4;
			Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Fiberglass_Shard_Proj>();
		}
    }
    public class Fiberglass_Shard_Proj : ModProjectile {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Shard");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.PoisonDart);
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.trap = false;
			Projectile.width = 5;
			Projectile.height = 5;
            Projectile.extraUpdates = 1;
			Projectile.penetrate = 7;
            Projectile.hide = true;
            Projectile.light = 0.025f;
        }
        public override void AI() {
            if (Main.rand.NextBool(200)){
	            Main.dust[Dust.NewDust(Projectile.Center+new Vector2(6,6).RotatedBy(Projectile.rotation), 0, 0, DustID.TreasureSparkle, 0f, 0f, 0, new Color(255,255,255), 1f)].velocity*=0.2f;
            }
            if(Projectile.damage == 1) {
                Vector2 center = Projectile.Center;// - new Vector2(0, 4).RotatedBy(projectile.rotation);
                double rot = Math.Round(Projectile.rotation / MathHelper.PiOver4);
                Vector2 offset = new Vector2(0, 8).RotatedBy(rot*MathHelper.PiOver4);//new Vector2(4, 0).RotatedBy(projectile.rotation);
	            //Main.dust[Dust.NewDust(center-offset, 0, 0, 204, 0f, 0f, 0, new Color(255,255,255), 1f)].velocity*=0f;
	            //Main.dust[Dust.NewDust(center+offset, 0, 0, 204, 0f, 0f, 0, new Color(255,255,255), 1f)].velocity*=0f;
                if(Collision.CanHitLine(center-offset, 0, 0, center+offset, 0, 0)) {
	                //Main.dust[Dust.NewDust(center, 0, 0, 27, 0f, 0f, 0, new Color(255,0,0), 1f)].velocity*=0.2f;
                    Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<Fiberglass_Shard>());
                    Projectile.Kill();
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(Main.rand.NextBool(Projectile.penetrate))return true;
            Projectile.aiStyle = 0;
            Projectile.hostile = true;
            Projectile.friendly = true;
            Projectile.damage = 1;
            Projectile.position += new Vector2(2.5f, 2.5f);
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.position -= new Vector2(7.5f, 7.5f);
            //projectile.position.X -= projectile.position.X % 4;
            //projectile.position.Y -= projectile.position.Y % 4;
            Projectile.knockBack = 0;
            oldVelocity = oldVelocity.SafeNormalize(oldVelocity);
            Projectile.position+=oldVelocity*12*(Projectile.velocity.Length()/21f);
            bool exposed = true;
            int tries = 0;
            while(exposed) {
                Projectile.position += oldVelocity;
                Vector2 center = Projectile.Center;// - new Vector2(0, 4).RotatedBy(projectile.rotation);
                double rot = System.Math.Round(Projectile.rotation / MathHelper.PiOver4);
                Vector2 offset = new Vector2(0, 8).RotatedBy(rot*MathHelper.PiOver4);
                exposed = ++tries<50&&Collision.CanHitLine(center - offset, 1, 1, center + offset, 1, 1);
            }
            Projectile.velocity*=0;
            return false;
        }
        public override bool? CanHitNPC(NPC target) {
            return Projectile.damage == 1?false:base.CanHitNPC(target);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
            if(Projectile.damage == 1) {
                Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Center, ModContent.ItemType<Fiberglass_Shard>());
                damage = 0;
                target.statLife--;
                PlayerDeathReason reason = new PlayerDeathReason();
                reason.SourceCustomReason = target.name+" cut themselves on broken glass";
                if(target.statLife<=0)target.Hurt(reason, 1, 0);
                //target.immune = true;
                Projectile.Kill();
            }
        }
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
            if (Projectile.hide) behindNPCsAndTiles.Add(index);
        }
		public override bool PreDraw(ref Color lightColor) {
            Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0,0,10,14), lightColor, Projectile.rotation, new Vector2(5, 14), 1f, SpriteEffects.None, 0);
            return false;
        }
    }
}
