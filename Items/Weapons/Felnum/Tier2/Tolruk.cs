using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Felnum.Tier2 {
	public class Tolruk : ModItem {
        int charge = 0;
        public static short[] glowmasks;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Tolruk");
			Tooltip.SetDefault("[tɵlɹɘk]\nSprite needs reshaping to be less like dart rifle");
            glowmasks = new short[]{
                -1,//Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_0"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_1"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_2"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_3"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_4"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_5"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_6"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_7"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_8"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_9"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_10")
            };
		}
		public override void SetDefaults() {
			item.damage = 30;
			item.ranged = true;
            item.noMelee = true;
			item.width = 18;
			item.height = 36;
			item.useTime = 10;
			item.useAnimation = 10;
			item.useStyle = 5;
			item.knockBack = 1;
			item.value = 500000;
			item.shootSpeed = 14;
			item.autoReuse = true;
            item.useAmmo = AmmoID.Bullet;
            item.shoot = ProjectileID.Bullet;
			item.rare = ItemRarityID.Lime;
			item.UseSound = SoundID.Item11;
		}
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 15);
            recipe.AddIngredient(ItemID.OrichalcumBar, 9);
            recipe.SetResult(this);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddRecipe();
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 15);
            recipe.AddIngredient(ItemID.MythrilBar, 9);
            recipe.SetResult(this);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddRecipe();
        }
        public override void HoldStyle(Player player) {
            if(charge>0) {
                charge--;
            }
        }
        public override void HoldItem(Player player) {
            item.glowMask = glowmasks[Math.Min(charge/4,10)];
        }
		public override Vector2? HoldoutOffset(){
			return new Vector2(-12,0);
		}
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Projectile p = Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), type, damage+charge, knockBack, player.whoAmI);
            if(p.penetrate>0&&Main.rand.Next(5-charge/10)==0) {
                p.penetrate++;
                p.localNPCHitCooldown = 10;
                p.usesLocalNPCImmunity = true;
            }
            PlaySound("DeepBoom", position, 0.5f);
            if(charge >= 40) {
                Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), ModContent.ProjectileType<Tolruk_Bolt>(), damage*3, knockBack, player.whoAmI);
                charge = 0;
                PlaySound("DeepBoom", position, 2);
                Main.PlaySound(2, (int)position.X, (int)position.Y, 122, 3f, 1f);
            } else charge+=4;
            return false;
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if(item.glowMask>-1)spriteBatch.Draw(Main.glowMaskTexture[item.glowMask], position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
    public class Tolruk_Bolt : ModProjectile {
        (Vector2?, Vector2)[] oldPos = new (Vector2?,Vector2)[14];
        public override string Texture => "Terraria/Projectile_3";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Tolruk Bolt");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.width = 15;
            projectile.height = 15;
            projectile.aiStyle = 0;
            projectile.extraUpdates = 5;
            projectile.hostile = false;
            projectile.friendly = true;
            projectile.timeLeft*=3;
            projectile.penetrate = -1;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            projectile.friendly = false;
            projectile.tileCollide = false;
            projectile.position+=oldVelocity;
            projectile.velocity = Vector2.Zero;
            projectile.timeLeft = 14;
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = 1;
            height = 1;
            return true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            int l = Math.Min(projectile.timeLeft, 14);
            for(int i = l; --i>0;) {
                oldPos[i] = oldPos[i-1];
                oldPos[i].Item1+=oldPos[i].Item2;
            }
            Vector2 dir = Main.rand.NextVector2Unit();
            oldPos[0] = (projectile.Center+dir*3, dir);
            for(int i = l; --i>0;) {
                if(oldPos[i].Item1.HasValue)spriteBatch.DrawLine(new Color(100,255,255), oldPos[i].Item1.Value, oldPos[i-1].Item1.Value, 2);
            }
            if(oldPos[0].Item1.HasValue)spriteBatch.DrawLine(new Color(100,255,255), oldPos[0].Item1.Value, projectile.Center+Main.rand.NextVector2Unit()*2, 2);
            return false;
        }
    }
}
