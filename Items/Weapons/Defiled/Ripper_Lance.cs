using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
	public class Ripper_Lance : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ripper Lance");
			Tooltip.SetDefault("Very pointy");
		}
		public override void SetDefaults() {
			Item.damage = 24;
			Item.melee = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
			Item.width = 52;
			Item.height = 56;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.useStyle = 5;
			Item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Ripper_Lance_P>();
            Item.shootSpeed = 3.75f;
			Item.value = 5000;
            Item.useTurn = false;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Projectile.NewProjectile(position, new Vector2(speedX, speedY).RotatedBy(-0.5f*player.direction), type, damage, knockBack, player.whoAmI);
            return false;
        }
	}
    public class Ripper_Lance_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Defiled/Ripper_Lance_P";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ripper Lance");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Spear);
            Projectile.timeLeft = 24;
			Projectile.width = 18;
			Projectile.height = 18;
            //projectile.scale*=1.25f;
        }
        public float movementFactor{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			projOwner.heldProj = Projectile.whoAmI;
            Projectile.direction = projOwner.direction;
            Projectile.spriteDirection = projOwner.direction;
			projOwner.itemTime = projOwner.itemAnimation;
			Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 0f){
                    movementFactor = 2.5f;
                    if(Projectile.timeLeft == 24)Projectile.timeLeft = projOwner.itemAnimationMax-1;
					Projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2){
					movementFactor-=2.1f;
				}else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1){
					movementFactor+=2.3f;
                }
			}
            Projectile.velocity = Projectile.velocity.RotatedBy(projOwner.direction*0.025f);
			Projectile.position += Projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0) {
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            if (Projectile.spriteDirection == 1) {
				Projectile.rotation -= MathHelper.PiOver2;
			}
		}
        static bool handleHit = false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            handleHit = false;
            if(projHitbox.Intersects(targetHitbox)||(handleHit=Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Main.player[Projectile.owner].MountedCenter+Projectile.velocity*2, Projectile.Center))) {
                return true;
            }
            return null;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            if(handleHit) {
                damage = (int)(damage*0.65f);
            }
        }
        public override bool PreDraw(ref Color lightColor){
            spriteBatch.Draw(Mod.GetTexture("Items/Weapons/Defiled/Ripper_Lance_P"), Projectile.Center - Main.screenPosition + Projectile.velocity*3, new Rectangle(0, 0, 80, 84), lightColor, Projectile.rotation, new Vector2(40+40*Projectile.spriteDirection,0), Projectile.scale, Projectile.spriteDirection>0?SpriteEffects.None:SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
    }
}
