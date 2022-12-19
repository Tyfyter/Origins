using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Defiled {
    public class Dismay : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Dismay");
            Tooltip.SetDefault("Very pointy for a book");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CursedFlames);
            Item.damage = 50;
            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.crit = 6;
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 5;
            Item.useAnimation = 25;
            Item.knockBack = 5;
            //item.shootSpeed = 20f;
            Item.shoot = ModContent.ProjectileType<Dismay_Spike>();
            Item.shootSpeed *= 1.2f;
            Item.useTurn = false;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.SoulofNight, 15);
            recipe.AddIngredient(ItemID.SpellTome);
            recipe.AddIngredient(ModContent.ItemType<Shaping_Matter>(), 20);
            recipe.AddTile(TileID.Bookcases);
            recipe.Register();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		    int n = (player.itemAnimationMax-player.itemAnimation)/player.itemTime+1;
            velocity = velocity.RotatedBy(((n/2)*((n&1)==0?1:-1))*0.3f);
        }
    }
    public class Dismay_Spike : ModProjectile {
        public override string Texture => "Origins/Projectiles/Weapons/Dismay_End";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.timeLeft = 25;
			Projectile.width = 18;
			Projectile.height = 18;
            Projectile.aiStyle = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
            Projectile.ownerHitCheck = true;
        }
        public float movementFactor{
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
        public override void AI() {
			Player projOwner = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);
            Projectile.direction = projOwner.direction;
            Projectile.spriteDirection = projOwner.direction;
			Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
			if (movementFactor == 0f){
                movementFactor = 1f;
                //if(projectile.timeLeft == 25)projectile.timeLeft = projOwner.itemAnimationMax-1;
				Projectile.netUpdate = true;
			}
			if (Projectile.timeLeft > 17){
				movementFactor+=1f;
            }
			Projectile.position += Projectile.velocity * movementFactor;
			Projectile.rotation = Projectile.velocity.ToRotation();
			Projectile.rotation += MathHelper.PiOver2;
        }
        public override bool PreDraw(ref Color lightColor) {
            float totalLength = Projectile.velocity.Length() * movementFactor;
            int avg = (lightColor.R + lightColor.G + lightColor.B)/3;
            lightColor = Color.Lerp(lightColor, new Color(avg, avg, avg), 0.5f);
            Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center-Main.screenPosition, new Rectangle(0, 0, 18, System.Math.Min(58, (int)totalLength)), lightColor, Projectile.rotation, new Vector2(9,0), Projectile.scale, SpriteEffects.None, 0);
            totalLength -= 58;
            Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero) * 58;
            Texture2D texture = Mod.Assets.Request<Texture2D>("Projectiles/Weapons/Dismay_Mid").Value;
            int c = 0;
            Vector2 pos;
            for(int i = (int)totalLength; i > 0; i -= 58) {
                c++;
                pos = (Projectile.Center - Main.screenPosition) - (offset * c);
                //lightColor = new Color(Lighting.GetSubLight(pos));//projectile.GetAlpha(new Color(Lighting.GetSubLight(pos)));
                Main.EntitySpriteDraw(texture, pos, new Rectangle(0, 0, 18, System.Math.Min(58, i)), lightColor, Projectile.rotation, new Vector2(9,0), Projectile.scale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
