using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
    public class Dismay : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Dismay");
            Tooltip.SetDefault("Very pointy for a book");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.CursedFlames);
            item.damage = 50;
            item.magic = true;
            item.noMelee = true;
            item.crit = 6;
            item.width = 28;
            item.height = 30;
            item.useTime = 5;
            item.useAnimation = 25;
            item.knockBack = 5;
            //item.shootSpeed = 20f;
            item.shoot = ModContent.ProjectileType<Dismay_Spike>();
            item.useTurn = false;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            int n = (player.itemAnimationMax-player.itemAnimation)/player.itemTime+1;
            Vector2 velocity = new Vector2(speedX, speedY);
            velocity = velocity.RotatedBy(((n/2)*((n&1)==0?1:-1))*0.3f);
            speedX = velocity.X;
            speedY = velocity.Y;
            return true;
        }
    }
    public class Dismay_Spike : ModProjectile {
        public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            projectile.timeLeft = 25;
			projectile.width = 18;
			projectile.height = 18;
            projectile.aiStyle = 0;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 25;
            projectile.ownerHitCheck = true;
        }
        public float movementFactor{
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}
        public override void AI() {
			Player projOwner = Main.player[projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
            projectile.direction = projOwner.direction;
            projectile.spriteDirection = projOwner.direction;
			projectile.position.X = ownerMountedCenter.X - (projectile.width / 2);
			projectile.position.Y = ownerMountedCenter.Y - (projectile.height / 2);
			if (movementFactor == 0f){
                movementFactor = 1f;
                //if(projectile.timeLeft == 25)projectile.timeLeft = projOwner.itemAnimationMax-1;
				projectile.netUpdate = true;
			}
			if (projectile.timeLeft > 15){
				movementFactor+=1f;
            }
			projectile.position += projectile.velocity * movementFactor;
			projectile.rotation = projectile.velocity.ToRotation();
			projectile.rotation += MathHelper.PiOver2;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            float totalLength = projectile.velocity.Length() * movementFactor;
            int avg = (lightColor.R + lightColor.G + lightColor.B)/3;
            lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center-Main.screenPosition, new Rectangle(0, 0, 18, System.Math.Min(58, (int)totalLength)), lightColor, projectile.rotation, new Vector2(9,0), projectile.scale, SpriteEffects.None, 0);
            totalLength -= 58;
            Vector2 offset = projectile.velocity.SafeNormalize(Vector2.Zero) * 58;
            Texture2D texture = mod.GetTexture("Projectiles/Weapons/Dismay_Mid");
            int c = 0;
            Vector2 pos;
            for(int i = (int)totalLength; i > 0; i -= 58) {
                c++;
                pos = (projectile.Center - Main.screenPosition) - (offset * c);
                //lightColor = new Color(Lighting.GetSubLight(pos));//projectile.GetAlpha(new Color(Lighting.GetSubLight(pos)));
                spriteBatch.Draw(texture, pos, new Rectangle(0, 0, 18, System.Math.Min(58, i)), lightColor, projectile.rotation, new Vector2(9,0), projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
