using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum {
    public class Felnum_Wand : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Zeus");
            Tooltip.SetDefault("Recieves 50% higher damage bonuses");
			Item.staff[item.type] = true;
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.CrystalVileShard);
            item.shoot = ModContent.ProjectileType<Felnum_Zap>();
            item.damage = 21;
            item.shootSpeed*=0.66f;
            item.UseSound = null;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 7);
            recipe.AddIngredient(ItemID.FallenStar);
            recipe.SetResult(this);
            recipe.AddTile(TileID.Anvils);
            recipe.AddRecipe();
        }
        public override void GetWeaponDamage(Player player, ref int damage) {
            //if(!OriginPlayer.ItemChecking)
            damage+=(damage-21)/2;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 speed = new Vector2(speedX, speedY);
            //damage+=(damage-35)/2;
			Main.PlaySound(2, (int)player.Center.X, (int)player.Center.Y, 122, 2f, 1f);
            Projectile.NewProjectile(position, speed, type, damage, knockBack, item.owner);
            return false;
        }
    }
    public class Felnum_Zap : ModProjectile {
        (Vector2?, Vector2)[] oldPos = new (Vector2?,Vector2)[7];
        public override string Texture => "Terraria/Projectile_3";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Zeus");
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.width = 15;
            projectile.height = 15;
            projectile.aiStyle = 0;
            projectile.extraUpdates = 1;
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
            projectile.timeLeft = 7;
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = 1;
            height = 1;
            return true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            int l = Math.Min(projectile.timeLeft, 7);
            if((projectile.timeLeft&1)==0) {
                for(int i = l; --i>0;) {
                    oldPos[i] = oldPos[i-1];
                    oldPos[i].Item1+=oldPos[i].Item2;
                }
                Vector2 dir = Main.rand.NextVector2Unit();
                oldPos[0] = (projectile.Center+dir*2, dir/2);
            }
            for(int i = l; --i>0;) {
                if(oldPos[i].Item1.HasValue)spriteBatch.DrawLine(Color.Aqua, oldPos[i].Item1.Value, oldPos[i-1].Item1.Value, 2);
            }
            if(oldPos[0].Item1.HasValue)spriteBatch.DrawLine(Color.Aqua, oldPos[0].Item1.Value, projectile.Center+Main.rand.NextVector2Unit()*2, 2);
            return false;
        }
    }
}
