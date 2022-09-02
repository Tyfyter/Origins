using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Riven {
    public class Decimator : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Decimator");
			Tooltip.SetDefault("");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.damage = 22;
			Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
			Item.width = 66;
			Item.height = 68;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
            Item.shoot = ModContent.ProjectileType<Decimator_P>();
            Item.shootSpeed = 3.75f;
			Item.value = 5000;
            Item.useTurn = false;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.glowMask = glowmask;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    Projectile.NewProjectile(source, position, velocity.RotatedBy(-0.5f*player.direction), type, damage, knockback, player.whoAmI);
            return false;
        }
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 9);
            recipe.AddIngredient(ModContent.ItemType<Riven_Sample>(), 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
	}
    public class Decimator_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Riven/Decimator_P";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Decimator");
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
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition + Projectile.velocity*5, new Rectangle(0, 0, 100, 98), lightColor, Projectile.rotation, new Vector2(40+40*Projectile.spriteDirection,0), Projectile.scale, Projectile.spriteDirection>0?SpriteEffects.None:SpriteEffects.FlipHorizontally, 0);
            return false;
        }
    }
}