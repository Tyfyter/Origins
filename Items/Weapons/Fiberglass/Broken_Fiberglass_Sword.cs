using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Fiberglass {
	public class Broken_Fiberglass_Sword : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Broken Fiberglass Sword");
			Tooltip.SetDefault("It's even sharper now");
		}
		public override void SetDefaults() {
			item.damage = 18;
			item.melee = true;
            item.noMelee = true;
            item.noUseGraphic = true;
			item.width = 24;
			item.height = 26;
			item.useTime = 14;
			item.useAnimation = 14;
			item.useStyle = 5;
			item.knockBack = 6;
			item.value = 5000;
			item.autoReuse = true;
            item.useTurn = true;
			item.shootSpeed = 3;
            item.shoot = ModContent.ProjectileType<Broken_Fiberglass_Sword_Stab>();
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item1;
		}
	}
    public class Broken_Fiberglass_Sword_Stab : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Fiberglass/Broken_Fiberglass_Sword";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Broken Fiberglass Sword");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Spear);
            projectile.timeLeft = 14;
			projectile.width = 20;
			projectile.height = 20;
        }
        public float movementFactor{
			get => projectile.ai[0];
			set => projectile.ai[0] = value;
		}

		public override void AI() {
			Player projOwner = Main.player[projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
			projectile.direction = projOwner.direction;
			projOwner.heldProj = projectile.whoAmI;
			projOwner.itemTime = projOwner.itemAnimation;
			projectile.position.X = ownerMountedCenter.X - (float)(projectile.width / 2);
			projectile.position.Y = ownerMountedCenter.Y - (float)(projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 0f){
                    movementFactor = 4.7f;
                    if(projectile.timeLeft == 26)projectile.timeLeft = projOwner.itemAnimationMax;
					projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 7){
					movementFactor-=1.7f;
				}else if (projOwner.itemAnimation > projOwner.itemAnimationMax*6f / 7){
					movementFactor+=1.3f;
                }
			}
			projectile.position += projectile.velocity * movementFactor;
			if (projOwner.itemAnimation == 0) {
				projectile.Kill();
			}
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(135f);
            if (projectile.spriteDirection == 1) {
				projectile.rotation -= MathHelper.Pi/2f;
			}
		}
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            spriteBatch.Draw(mod.GetTexture("Items/Weapons/Fiberglass/Broken_Fiberglass_Sword"), (projectile.Center - projectile.velocity*2) - Main.screenPosition, new Rectangle(0, 0, 24, 26), lightColor, projectile.rotation, new Vector2(12,13), 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
