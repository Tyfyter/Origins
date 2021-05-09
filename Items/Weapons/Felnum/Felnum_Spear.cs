using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum {
	public class Felnum_Spear : ModItem {
        public const int baseDamage = 18;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Boar Spear");
			Tooltip.SetDefault("Recieves 50% higher damage bonuses");
		}
		public override void SetDefaults() {
			item.damage = baseDamage;
			item.melee = true;
            item.noMelee = true;
            item.noUseGraphic = true;
			item.width = 48;
			item.height = 48;
			item.useTime = 24;
			item.useAnimation = 24;
			item.useStyle = 5;
			item.knockBack = 6;
			item.value = 5000;
			item.autoReuse = true;
            item.useTurn = true;
			item.shootSpeed = 3;
            item.shoot = ModContent.ProjectileType<Felnum_Spear_Stab>();
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item1;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 8);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();
        }
        public override void GetWeaponDamage(Player player, ref int damage) {
            if(!OriginPlayer.ItemChecking)damage+=(damage-baseDamage)/2;
        }
    }
    public class Felnum_Spear_Stab : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Felnum/Felnum_Spear_P";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Boar Spear");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Spear);
            projectile.timeLeft = 24;
			projectile.width = 18;
			projectile.height = 18;
            //projectile.scale*=1.25f;
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
			projectile.position.X = ownerMountedCenter.X - (projectile.width / 2);
			projectile.position.Y = ownerMountedCenter.Y - (projectile.height / 2);
			if (!projOwner.frozen) {
				if (movementFactor == 0f){
                    movementFactor = 2.5f;
                    if(projectile.timeLeft == 26)projectile.timeLeft = projOwner.itemAnimationMax;
					projectile.netUpdate = true;
				}
				if (projOwner.itemAnimation < projOwner.itemAnimationMax / 2 - 1){
					movementFactor-=2.5f;
				}else if (projOwner.itemAnimation > projOwner.itemAnimationMax / 2 + 1){
					movementFactor+=2.7f;
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
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage+=(damage-18)/2;
            Player player = Main.player[projectile.owner];
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            if(originPlayer.felnumShock>29) {
                damage+=(int)(originPlayer.felnumShock/30);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            spriteBatch.Draw(mod.GetTexture("Items/Weapons/Felnum/Felnum_Spear_P"), (projectile.Center) - Main.screenPosition, new Rectangle(0, 0, 72, 72), lightColor, projectile.rotation, new Vector2(62,8), projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
